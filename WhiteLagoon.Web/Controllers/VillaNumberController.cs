using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public VillaNumberController(IUnitOfWork unitOfWork)
        {

            this.unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            var villaNumbers = unitOfWork.VillaNumberRepo.GetAll(filter: null, includeProperties: "villa");
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            VillaNumberVM model = new()
            {
                VillaList = unitOfWork.VillaRepo.GetAll().Select(u =>
                  new SelectListItem
                  {
                      Text = u.Name,
                      Value = u.Id.ToString(),
                  })
            };


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VillaNumberVM model)
        {
            var IsExistedNumber = unitOfWork.VillaNumberRepo.Any(x => x.Villa_Number == model.VillaNumber.Villa_Number);
            if (IsExistedNumber)
            {
                TempData["error"] = "Villa Number " + model.VillaNumber.Villa_Number + " Already Taken";
                return RedirectToAction("Create");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    unitOfWork.VillaNumberRepo.Add(model.VillaNumber);
                  await  unitOfWork.SaveAllAsync();
                    TempData["success"] = "New Villa Number Has Been Added SuccessFully";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["error"] = "Failed to Create New Villa Number";
                    return RedirectToAction("Create");
                }
            }
            return RedirectToAction("Create");
        }




        //[HttpPut("{villaId}")]
        public IActionResult Update(int villaNumberId)
        {
            VillaNumber? villa = unitOfWork.VillaNumberRepo.GetSingle(filter: x => x.Villa_Number == villaNumberId);
            if (villa == null) return RedirectToAction("Error", "Home");
            VillaNumberVM model = new()
            {
                VillaNumber = villa,
                VillaList = unitOfWork.VillaRepo.GetAll().Select(x =>
                new SelectListItem { Text = x.Name, Value = x.Id.ToString() }),
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Update(VillaNumber villaNumber)
        {
            if (ModelState.IsValid)
            {

                unitOfWork.VillaNumberRepo.UpdateVillaNumber(villaNumber);
               await unitOfWork.SaveAllAsync();
                TempData["success"] = "VillaNumber Has Been Updated SuccessFully";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Create");
        }



        //[HttpDelete("{villaId}")]
        public IActionResult Delete(int villaNumberId)
        {
            var villaNumber = unitOfWork.VillaNumberRepo.GetSingle(filter: x => x.Villa_Number == villaNumberId, includeProperties: "villa");
            if (villaNumber == null)
                return RedirectToAction("Error", "Home");


            return View(villaNumber);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(VillaNumber villaNumber)
        {
            VillaNumber? villaNumberfromdb = unitOfWork.VillaNumberRepo.GetSingle(filter: x => x.Villa_Number == villaNumber.Villa_Number);
            if (villaNumberfromdb is not null)
            {
                unitOfWork.VillaNumberRepo.Remove(villaNumberfromdb);
                await unitOfWork.SaveAllAsync();
                TempData["success"] = "VillaNumber Has Been Deleted SuccessFully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "VillaNumber Can NotBe Deleted";
            return RedirectToAction("Error", "Home");

        }

    }
}
