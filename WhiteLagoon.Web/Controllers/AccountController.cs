using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signinManager;
        private readonly RoleManager<IdentityRole> roleManager;
       

        public AccountController(IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signinManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.signinManager = signinManager;
            this.roleManager = roleManager;
        }

        public IActionResult Login(string? returnUrl=null)
        {
            returnUrl??=Url.Content("~/");
            var loginVM = new LoginVM {
              RedirectUrl=returnUrl
            };
            return View(loginVM);
        }

        public async Task<IActionResult> Register()
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            }

            var rolesSelectList = (await roleManager.Roles.ToListAsync())
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id });

            return View(rolesSelectList);
        }




    }
}
