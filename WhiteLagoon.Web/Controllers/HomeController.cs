using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Repository;
using WhiteLagoon.Web.Models;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var homeVM = new HomeVM
            {
                VillaList = unitOfWork.VillaRepo.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now)
            };
            return View(homeVM);
        }


        [HttpPost]
        public IActionResult GetVillasByDate(int nights,DateOnly checkInDate)
        {

            var villaList = unitOfWork.VillaRepo.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumbersList = unitOfWork.VillaNumberRepo.GetAll().ToList();
            var bookedVillas = unitOfWork.BookingRepo.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();


            foreach (var villa in villaList)
            {
                int roomAvailable = SD.VillaRoomAvailable_Count
                    (villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);

                villa.IsAvailable = roomAvailable > 0 ? true : false;
            }
            var homeVM = new HomeVM { 
            CheckInDate=checkInDate,
            Nights=nights,
            VillaList=villaList
            };
            return PartialView("_VillaListPartial",homeVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
