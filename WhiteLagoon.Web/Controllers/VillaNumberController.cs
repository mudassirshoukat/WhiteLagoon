using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
   public class VillaNumberController : Controller
   {
      private readonly ApplicationDbContext context;

      public VillaNumberController(ApplicationDbContext context)
      {
         this.context = context;
      }


      public async Task<IActionResult> Index()
      {
         var villaNumbers =await context.VillaNumbers.Include(x=>x.villa).ToListAsync();
         return View(villaNumbers);
      }

      public IActionResult Create()
      {
            VillaNumberVM model = new() {
                VillaList = context.Villas.ToList().Select(u =>
                  new SelectListItem
                  {
                      Text = u.Name,
                      Value = u.Id.ToString(),
                  })
        };
           
           
            return View(model); 
      }

      [HttpPost]
      public IActionResult Create( VillaNumberVM model)
      {
            var IsExistedNumber = context.VillaNumbers.Any(x => x.Villa_Number == model.VillaNumber.Villa_Number);
            if (IsExistedNumber)
            {
                TempData["error"] = "Villa Number "+model.VillaNumber.Villa_Number+" Already Taken";
                return RedirectToAction("Create");
            }
         if (ModelState.IsValid)
         {
                try
                {
                    context.VillaNumbers.Add(model.VillaNumber);
                    context.SaveChanges();
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
      public IActionResult Update( int villaNumberId)
      {
         VillaNumber? villa= context.VillaNumbers.AsNoTracking().FirstOrDefault(x=>x.Villa_Number == villaNumberId);
         if (villa == null) return RedirectToAction("Error", "Home");
            VillaNumberVM model = new()
            { VillaNumber=villa,
            VillaList= context.Villas.ToList().Select(x=>
            new SelectListItem { Text=x.Name,Value=x.Id.ToString()}),
            };
         return View(model);
      }


          [HttpPost]
          public IActionResult Update(VillaNumber villaNumber)
          {
             if (ModelState.IsValid)
             {

                context.VillaNumbers.Update(villaNumber);
                context.SaveChanges();
                    TempData["success"] = "VillaNumber Has Been Updated SuccessFully";
                    return RedirectToAction("Index");
             }
             return RedirectToAction("Create");
          }



      //[HttpDelete("{villaId}")]
      public  IActionResult Delete(int villaNumberId)
      {
         var villaNumber= context.VillaNumbers.Include(x=>x.villa).FirstOrDefault( x=>x.Villa_Number == villaNumberId); 
            if (villaNumber == null) 
            return RedirectToAction("Error","Home");
        

         return View(villaNumber);
      }

      [HttpPost]
      public async Task<IActionResult> Delete(VillaNumber villaNumber)
      {
         VillaNumber? villaNumberfromdb = context.VillaNumbers.FirstOrDefault(x => x.Villa_Number == villaNumber.Villa_Number);
         if (villaNumberfromdb is not null)
         {
            context.VillaNumbers.Remove(villaNumberfromdb);
            await context.SaveChangesAsync();
                TempData["success"] = "VillaNumber Has Been Deleted SuccessFully";
            return RedirectToAction("Index");
         }
            TempData["error"] = "VillaNumber Can NotBe Deleted";
            return RedirectToAction("Error","Home");
         
      }

   }
}
