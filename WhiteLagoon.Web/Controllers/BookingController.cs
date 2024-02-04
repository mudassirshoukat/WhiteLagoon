using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Stripe.Checkout;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Extentions;

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
        public async Task<IActionResult> FinalizeBooking(Booking booking)
        {
            if (ModelState.IsValid)
            {
                booking.Villa = unitOfWork.VillaRepo.GetSingle(filter: x => x.Id == booking.VillaId);
                booking.TotalCost = booking.Villa.Price * booking.Nights;
                booking.Status = SD.StatusPending;
                booking.BookingDate = DateOnly.FromDateTime(DateTime.Now);

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


        public async Task<IActionResult> BookingConfirmation(int bookingId)
        {
            var booking = unitOfWork.BookingRepo.GetSingle(filter: x => x.Id == bookingId);
            if (booking.Status == SD.StatusPending)
            {
                var service = new SessionService();
                var session = await service.GetAsync(booking.StripeSessionId);
                if (session.PaymentStatus == "paid")
                {
                    await unitOfWork.BookingRepo.UpdateStatusAsync(bookingId, SD.StatusApproved);
                    await unitOfWork.BookingRepo.UpdateStripePaymentIDAsync(bookingId, session.Id, session.PaymentIntentId);
                    await unitOfWork.SaveAllAsync();
                }

            }
            return View(bookingId);
        }
    }
}
