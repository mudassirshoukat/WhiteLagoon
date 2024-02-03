using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.utility;
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


        public IActionResult Login(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var loginVM = new LoginVM
            {
                RedirectUrl = returnUrl
            };
            return View(loginVM);
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    TempData["error"] = "Failed To Login";
                    ModelState.AddModelError("", "User With This Email Not Exist");
                    return View(model);
                }

                var result = await signinManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    TempData["success"] = "Login Success";
                    if (!string.IsNullOrEmpty(model.RedirectUrl))
                    {
                        return LocalRedirect(model.RedirectUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    TempData["error"] = "Failed To Login";
                    ModelState.AddModelError("", "Wrong Password");
                }
            }
            return View(model);


        }

        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!await roleManager.RoleExistsAsync(SD.Role_Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
            }

            RegisterVM model = new()
            {
                RedirectUrl = returnUrl,
                RoleList = (await roleManager.Roles.ToListAsync())
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Name })
            };


            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new()
                {
                    Name = model.Name,
                    Email = model.Email,
                    NormalizedEmail = model.Email.ToUpper(),
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now

                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await userManager.AddToRoleAsync(user, model.Role);
                    }
                    else
                    {
                        await userManager.AddToRoleAsync(user, SD.Role_Customer);
                    }

                    await signinManager.SignInAsync(user, isPersistent: false);
                    TempData["success"] = "New User SuccessFully Registered";
                    if (!string.IsNullOrEmpty(model.RedirectUrl))
                    {
                        return LocalRedirect(model.RedirectUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                //Getting All Error and redirating to Same View
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                        error.Description = $"Email '{model.Email}' is already taken";
                    ModelState.AddModelError("", error.Description);
                }
            }

            RegisterVM registerModel = new()
            {
                RoleList = (await roleManager.Roles.ToListAsync())
          .Select(x => new SelectListItem { Text = x.Name, Value = x.Name })
            };
            return View(registerModel);
        }


        public async Task<IActionResult> SignOut()
        {
            await signinManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult ForgetPassword() { return View(); }

    }
}
