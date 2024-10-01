using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Shortly.Client.Data.ViewModels;
using Shortly.Data.Models;
using Shortly.Data.Services;

namespace Shortly.Client.Controllers
{
    public class AuthenticationController : Controller
    {
        private IUsersService _usersService;
        private SignInManager<AppUser> _signInManager;
        private UserManager<AppUser> _userManager;
        private IConfiguration _configuration;

        public AuthenticationController(IUsersService usersService,
             SignInManager<AppUser> signInManager,
             UserManager<AppUser> userManager,
             IConfiguration configuration)
        {
            _usersService = usersService;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;

        }
        public async Task<IActionResult> Users()
        {
            var users = await _usersService.GetUsersAsync();
            return View(users);
        }
        
        public async Task<IActionResult> Login()
        {
            /* var loginVM = new LoginVM()
             {
                 Schemes = await _signInManager.GetExternalAuthenticationSchemesAsync()
             };*/

            return View(new LoginVM());
        }
        
        public async Task<IActionResult> LoginSubmitted(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", loginVM);
            }

            var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
            if (user != null)
            {
                var userPasswordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
                if (userPasswordCheck)
                {
                    var userLoggedIn = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);

                    if (userLoggedIn.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (userLoggedIn.IsNotAllowed)
                    {
                        return RedirectToAction("EmailConfirmation");
                    }
                    else if (userLoggedIn.RequiresTwoFactor)
                    {
                        return RedirectToAction("TwoFactorConfirmation", new { loggedInUserId = user.Id });
                    }

                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt. Please, check your username and password");
                        return View("Login", loginVM);
                    }
                }
                else
                {
                    await _userManager.AccessFailedAsync(user);

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", "Your account is locked, please try again in 10 mins");
                        return View("Login", loginVM);
                    }

                    ModelState.AddModelError("", "Invalid login attempt. Please, check your username and password");
                    return View("Login", loginVM);
                }
            }


            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}
