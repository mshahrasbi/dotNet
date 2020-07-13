using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private SignInManager<CognitoUser> _signingManager;
        private UserManager<CognitoUser> _userManager;
        private CognitoUserPool _pool;

        public AccountsController(SignInManager<CognitoUser> signInManager,
                UserManager<CognitoUser> userManager,
                CognitoUserPool pool) 
        {
            this._pool = pool;
            this._signingManager = signInManager;
            this._userManager = userManager;
        }

        public IActionResult Signup()
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel model)
        {

            //if (!ModelState.IsValid)
            //{
            //    var errors = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.Exception));
            //    Console.WriteLine(errors);
            //}

            if (ModelState.IsValid)
            {
                var user = this._pool.GetUser(model.Email);
                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExists", "User with this email already exists");

                    return View(model);
                }

                /*
                 * Here if we go to AWS Cognito console to check to see what attributes do we have. As we
                 * know we need to specify a Name attribute, so we need to add name, if we don't add the name
                 * we will get an exception from cognito.
                 */
                user.Attributes.Add(CognitoAttribute.Name.AttributeName, model.Email);

                /**
                 * Create a user
                 * If you don't pass password you can crate user. However in this case
                 * a password will be auto generated and it will be sent as a temparay
                 * password to your user then user on the first login will have to change
                 * the password. That means you will need to create a change password form
                 * as well and or use the built in signin page of cognito.
                 */
                var createdUser = await this._userManager.CreateAsync(user, model.Password);
                if (createdUser.Succeeded)
                {
                    return RedirectToAction("Confirm", "Accounts");
                } else
                {
                    foreach (var item in createdUser.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }

                    return View(ModelState);
                }
            }
            return View();
        }

        
        [HttpGet]
        public IActionResult Confirm()
        {
            var model = new ConfirmModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            if (ModelState.IsValid)
            {
                // fetch the user by userManager
                var user = await this._userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
                if (user == null)
                {
                    ModelState.AddModelError("NotFound", "A user with the email address not found");
                    return View(model);
                }

                var result = await ((CognitoUserManager<CognitoUser>) this._userManager).ConfirmSignUpAsync(user, model.Code, true).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                } else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }

                    return View(ModelState);
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Login(LoginModel model)
        {
            return View(model);
        }
        
        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> LoginPost(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await this._signingManager.PasswordSignInAsync(
                                                    model.Email, 
                                                    model.Password, 
                                                    model.RememberMe, 
                                                    false).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    return RedirectToAction("index", "home");
                } else
                {
                    ModelState.AddModelError("LoginError", "Email or Password do not match");
                    return View(ModelState);
                }
            }

            return View(model);
        }


        public async Task<IActionResult> Signout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await this._signingManager.SignOutAsync().ConfigureAwait(false);
            }
            return RedirectToAction("Login");
        }
    }
}
