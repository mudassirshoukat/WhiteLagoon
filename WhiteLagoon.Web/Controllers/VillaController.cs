using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
   {
        private readonly IUnitOfWork unitOfWork;

        //private readonly IVillaRepository villaRepo;

        public VillaController(IUnitOfWork unitOfWork)
      {
         
           
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
      {
            var villas = unitOfWork.VillaRepo.GetAll();
         return View(villas);
      }

      public IActionResult Create()
      {

         return View();
      }

      [HttpPost]
      public async Task<IActionResult> Create(Villa villa)
      {
         if (villa.Name == villa.Description)
         {//custom validation
            ModelState.AddModelError("Description", "Description Should Not Be Same as Name");
         }
         if (ModelState.IsValid)
         {
                unitOfWork.VillaRepo.Add(villa);
              await  unitOfWork.SaveAllAsync();
                TempData["success"] = "New Villa Has Been Added SuccessFully";
                return RedirectToAction("Index");
         }
         return View();
      }

      //[HttpPut("{villaId}")]
      public IActionResult Update( int villaId)
      {
            Villa? villa = unitOfWork.VillaRepo.GetSingle(x => x.Id == villaId);
         if (villa == null) return RedirectToAction("Error", "Home");
       
         return View(villa);
      }


      [HttpPost]
      public async Task<IActionResult> Update(Villa villa)
      {
         if (ModelState.IsValid)
         {

                unitOfWork.VillaRepo.UpdateVilla(villa);
              await unitOfWork.SaveAllAsync();
                TempData["success"] = "Villa Has Been Updated SuccessFully";
                return RedirectToAction("Index");
         }
         return View();
      }



      //[HttpDelete("{villaId}")]
      public  IActionResult Delete(int villaId)
      {
         var villa = unitOfWork.VillaRepo.GetSingle( x=>x.Id == villaId); 
            if (villa == null) 
            return RedirectToAction("Error","Home");
        

         return View(villa);
      }

      [HttpPost]
      public async Task<IActionResult> Delete(Villa villa)
      {
         Villa? villafromdb = unitOfWork.VillaRepo.GetSingle(x => x.Id == villa.Id);
         if (villafromdb is not null)
         {
                unitOfWork.VillaRepo.Remove(villafromdb);
               await unitOfWork.SaveAllAsync();
                TempData["success"] = "Villa Has Been Deleted SuccessFully";
            return RedirectToAction("Index");
         }
            TempData["error"] = "Villa Can NotBe Deleted";
            return RedirectToAction("Error","Home");
         
      }

   }
}
