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
            if (villa.Name==villa.Description)
            {//custom validation
            ModelState.AddModelError("Description", "Description Should Not Be Same as Name");
            }
            if (ModelState.IsValid)
         {
            context.Villas.Add(villa);
            context.SaveChanges();
            return RedirectToAction("Index");
         }
         return View();
      }

   }
}
