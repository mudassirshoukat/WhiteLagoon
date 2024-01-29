using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public AmenityController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var Amenities = unitOfWork.AmenityRepo.GetAll(includeProperties: "Villa");
            return View(Amenities);
        }

        public IActionResult Create()
        {
            var model = new AmenityVM
            {
                VillaList = unitOfWork.VillaRepo.GetAll().Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
            };

            return View(model);
        }
        //amenityId
        [HttpPost]
        public async Task<IActionResult> Create(AmenityVM model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    unitOfWork.AmenityRepo.Add(model.Amenity);
                    await unitOfWork.SaveAllAsync();
                    TempData["success"] = "New Amenity HasBeen Added";
                }
                catch (Exception)
                {

                    TempData["error"] = "Failed To Add New Amenity";
                    return RedirectToAction(nameof(Create));
                }
            }
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Update(int amenityId)
        {
            AmenityVM model = new()
            {
                Amenity = unitOfWork.AmenityRepo.GetSingle(x => x.Id == amenityId),
                VillaList = unitOfWork.VillaRepo.GetAll().Select(x =>
                new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
            };
            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> Update(AmenityVM model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    unitOfWork.AmenityRepo.UpdateAmenity(model.Amenity);
                    await unitOfWork.SaveAllAsync();
                    TempData["success"] = "Updated SuccessFully";
                }
                catch (Exception)
                {

                    TempData["error"] = "Failed To Update Amenity";
                    return RedirectToAction(nameof(Update));
                }
            }
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Delete(int amenityId)
        {

            var model = unitOfWork.AmenityRepo.GetSingle(includeProperties: "Villa",filter:x => x.Id == amenityId);
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> Delete(Amenity model)
        {
            var AmenityFromDb = unitOfWork.AmenityRepo.GetSingle(x => x.Id == model.Id);
            if (AmenityFromDb is not null)
            {
                unitOfWork.AmenityRepo.Remove(AmenityFromDb);
                await unitOfWork.SaveAllAsync();
                TempData["success"] = "Amenity Deleted Successfully";
            }
            else
            {
                TempData["error"] = "Failed To Delete Amenity";
                //return RedirectToAction(nameof(Delete(model.Id)));
            }
            return RedirectToAction(nameof(Index));

        }




    }
}
