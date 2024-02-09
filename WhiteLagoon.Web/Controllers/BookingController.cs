
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Stripe.Checkout;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Extentions;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{

    public class BookingController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public BookingController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }




        [Authorize]
        public IActionResult Index()
        {
            return View();
        }



        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var userId = User.GetUserId();
            var appuser = unitOfWork.AppUserRepo.GetSingle(filter: x => x.Id == userId);
            if (appuser == null)
            {
                return RedirectToAction("index", "Home");
            }
            Booking booking = new()
            {
                Name = appuser.Name,
                Email = appuser.Email,
                Phone = appuser.PhoneNumber,
                UserId = userId,
                User = appuser,
                VillaId = villaId,
                Villa = unitOfWork.VillaRepo.GetSingle(filter: x => x.Id == villaId, includeProperties: "VillaAmenity"),
                Nights = nights,
                CheckInDate = checkInDate,
                CheckOutDate = checkInDate.AddDays(nights)


            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> FinalizeBooking(Booking booking)
        {
            if (ModelState.IsValid)
            {
                booking.Villa = unitOfWork.VillaRepo.GetSingle(filter: x => x.Id == booking.VillaId);
                booking.TotalCost = booking.Villa.Price * booking.Nights;
                booking.Status = SD.StatusPending;
                booking.BookingDate = DateOnly.FromDateTime(DateTime.Now);

                //check availability of villarooms again
                var villaList = unitOfWork.VillaRepo.GetAll(includeProperties: "VillaAmenity").ToList();
                var villaNumbersList = unitOfWork.VillaNumberRepo.GetAll().ToList();
                var bookedVillas = unitOfWork.BookingRepo.GetAll(u => u.Status == SD.StatusApproved ||
                u.Status == SD.StatusCheckedIn).ToList();


               
                    int roomAvailable = SD.VillaRoomAvailable_Count
                        (booking.Villa.Id, villaNumbersList, booking.CheckInDate, booking.Nights, bookedVillas);

                  if (roomAvailable == 0) {
                    TempData["error"] = "Room Has Been Sold Out!";

                    return RedirectToAction(nameof(FinalizeBooking),new {booking.VillaId,booking.CheckInDate,booking.Nights});
                }
            



                unitOfWork.BookingRepo.Add(booking);
                await unitOfWork.SaveAllAsync();


                //strip work goes on
                var domain = Request.Scheme + "://" + Request.Host.Value;
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = domain + $"/Booking/BookingConfirmation?bookingId={booking.Id}",
                    CancelUrl = domain + $"/Booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate{booking.CheckInDate}&nights={booking.Nights}",
                };
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "USD",
                        UnitAmount = (long)(booking.TotalCost * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{booking.Villa.Name}: {booking.Villa.Price}/Night",
                            Description = $"Nights: {booking.Nights}"


                        }

                    },
                    Quantity = 1
                });



                var service = new SessionService();
                Session session = service.Create(options);
                await unitOfWork.BookingRepo.UpdateStripePaymentIDAsync(booking.Id, session.Id, session.PaymentIntentId);
                await unitOfWork.SaveAllAsync();

                Response.Headers.Append("Location", session.Url);
                return new StatusCodeResult(303);


            }
            TempData["error"] = "Failed! Something Went Wrong";
            return RedirectToAction(nameof(FinalizeBooking),
                new { villaId = booking.VillaId, checkInDate = booking.CheckInDate, nights = booking.Nights });


        }







        [Authorize]
        public async Task<IActionResult> BookingConfirmation(int bookingId)
        {
            var booking = unitOfWork.BookingRepo.GetSingle(filter: x => x.Id == bookingId);
            if (booking.Status == SD.StatusPending)
            {
                var service = new SessionService();
                var session = await service.GetAsync(booking.StripeSessionId);
                if (session.PaymentStatus == "paid")
                {
                    await unitOfWork.BookingRepo.UpdateStatusAsync(bookingId, SD.StatusApproved, 0);
                    await unitOfWork.BookingRepo.UpdateStripePaymentIDAsync(bookingId, session.Id, session.PaymentIntentId);
                    await unitOfWork.SaveAllAsync();
                }
            }
            return View(bookingId);
        }



        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            var booking = unitOfWork.BookingRepo.GetSingle(z => z.Id == bookingId, includeProperties: "Villa,User");
            if (booking.VillaNumber == 0 && booking.Status == SD.StatusApproved)
            {
                var availableRooms = AssignVillaNumberByVilla(booking.VillaId);
                booking.VillaNumbers = unitOfWork.VillaNumberRepo.GetAll(x => x.VillaId == booking.VillaId
                && availableRooms.Any(u => u == x.Villa_Number)).ToList();
            }
            return View(booking);

        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> CheckIn(Booking booking)
        {
            await unitOfWork.BookingRepo.UpdateStatusAsync(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            await unitOfWork.SaveAllAsync();
            TempData["success"] = "Booking Updated SuccessFully";

            return RedirectToAction(nameof(BookingDetails),new {bookingId=booking.Id});
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> CheckOut(Booking booking)
        {
            await unitOfWork.BookingRepo.UpdateStatusAsync(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            await unitOfWork.SaveAllAsync();
            TempData["success"] = "Booking Completed SuccessFully";

            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> CancekBooking(Booking booking)
        {
            await unitOfWork.BookingRepo.UpdateStatusAsync(booking.Id, SD.StatusCancelled, 0);
            await unitOfWork.SaveAllAsync();
            TempData["success"] = "Booking Canceled SuccessFully";

            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignVillaNumberByVilla(int villaId)
        {
            List<int> availableRooms = new();
            var villaNumbers = unitOfWork.VillaNumberRepo.GetAll(x => x.VillaId == villaId);
            var CheckedInRooms = unitOfWork.BookingRepo.GetAll(x => x.VillaId == villaId && x.Status == SD.StatusCheckedIn)
                .Select(x => x.VillaNumber);

            foreach (var villaNumber in villaNumbers)
            {
                if (!CheckedInRooms.Contains(villaNumber.Villa_Number))
                {
                    availableRooms.Add(villaNumber.Villa_Number);
                }
            }

            return availableRooms;
        }

        #region API Calls

        [Authorize]
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> bookings;
            if (User.IsInRole(SD.Role_Admin))
            {
                bookings = unitOfWork.BookingRepo.GetAll(includeProperties: "Villa,User");
            }
            else
            {
                bookings = unitOfWork.BookingRepo.GetAll(filter: x => x.UserId == User.GetUserId(), includeProperties: "Villa,User");
            }
            if (!string.IsNullOrEmpty(status))
            {
                bookings = bookings.Where(x => x.Status.ToLower().Equals(status.ToLower()));
            }

            return Json(new { data = bookings });
        }



        #endregion
    }
}
