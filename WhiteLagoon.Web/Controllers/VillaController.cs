using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
   public class VillaController : Controller
   {
      private readonly ApplicationDbContext context;

      public VillaController(ApplicationDbContext context)
      {
         this.context = context;
      }


      public IActionResult Index()
      {
         var villa = context.Villas.ToList();
         return View(villa);
      }

      public IActionResult Create()
      {

         return View();
      }

      [HttpPost]
      public IActionResult Create(Villa villa)
      {
         if (villa.Name == villa.Description)
         {//custom validation
            ModelState.AddModelError("Description", "Description Should Not Be Same as Name");
         }
         if (ModelState.IsValid)
         {
            context.Villas.Add(villa);
            context.SaveChanges();
                TempData["success"] = "New Villa Has Been Added SuccessFully";
                return RedirectToAction("Index");
         }
         return View();
      }

      //[HttpPut("{villaId}")]
      public IActionResult Update( int villaId)
      {
         Villa? villa= context.Villas.FirstOrDefault(x=>x.Id == villaId);
         if (villa == null) return RedirectToAction("Error", "Home");
       
         return View(villa);
      }


      [HttpPost]
      public IActionResult Update(Villa villa)
      {
         if (ModelState.IsValid)
         {
            
            context.Villas.Update(villa);
            context.SaveChanges();
                TempData["success"] = "Villa Has Been Updated SuccessFully";
                return RedirectToAction("Index");
         }
         return View();
      }



      //[HttpDelete("{villaId}")]
      public  IActionResult Delete(int villaId)
      {
         var villa= context.Villas.FirstOrDefault( x=>x.Id == villaId); 
            if (villa == null) 
            return RedirectToAction("Error","Home");
        

         return View(villa);
      }

      [HttpPost]
      public async Task<IActionResult> Delete(Villa villa)
      {
         Villa? villafromdb = context.Villas.FirstOrDefault(x => x.Id == villa.Id);
         if (villafromdb is not null)
         {
            context.Villas.Remove(villafromdb);
            await context.SaveChangesAsync();
                TempData["success"] = "Villa Has Been Deleted SuccessFully";
            return RedirectToAction("Index");
         }
            TempData["error"] = "Villa Can NotBe Deleted";
            return RedirectToAction("Error","Home");
         
      }

   }
}
