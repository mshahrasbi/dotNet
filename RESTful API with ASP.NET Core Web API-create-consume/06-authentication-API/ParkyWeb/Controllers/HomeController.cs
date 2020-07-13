using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkyWeb.Models;
using ParkyWeb.Models.ViewModel;
using ParkyWeb.repository.IRepository;

namespace ParkyWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private INationalParkRepository _npRepo;
        private ITrailRepository _trailRepo;
        private IAccountRepository _accountRepository;

        public HomeController(
                ILogger<HomeController> logger, 
                INationalParkRepository npRepo, 
                ITrailRepository trailRepo,
                IAccountRepository accountRepository)
        {
            _logger = logger;
            _npRepo = npRepo;
            _trailRepo = trailRepo;
            this._accountRepository = accountRepository;
        }

        public async Task<IActionResult> Index()
        {
            IndexVM listOfParksAndTrails = new IndexVM()
            {
                NationalParkList = await _npRepo.GetAllAsync(SD.NationalParkAPIPath, HttpContext.Session.GetString("JWToken")),
                TrailList = await _trailRepo.GetAllAsync(SD.TrailAPIPath, HttpContext.Session.GetString("JWToken"))
            };
            return View(listOfParksAndTrails);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public IActionResult Login()
        {
            User userObj = new User();
            return View(userObj);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User userObj)
        {
            User obj = await this._accountRepository.LoginAsync(SD.AccountAPIPath + "authenticate/", userObj);

            if (obj.Token == null)
            {
                return View();
            }

            /*
             * Here we want to use the cookie authentication, we have to add a principal claim and
             * claim
             */
            // here we create claim identity
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            // so on this identity we are adding claims so that we can use it throughtout application. 
            // so we are adding username and role to our identity
            identity.AddClaim(new Claim(ClaimTypes.Name, obj.Username));
            identity.AddClaim(new Claim(ClaimTypes.Role, obj.Role));

            // now we are creating principal from our identity
            var principal = new ClaimsPrincipal(identity);
            // sign the user in. So this line will automatically sign the user in
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Session.SetString("JWToken", obj.Token);
            TempData["alert"] = "Welcome " + obj.Username;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User userObj)
        {
            bool result = await this._accountRepository.RegisterAsync(SD.AccountAPIPath + "register/", userObj);

            if (result == false)
            {
                return View();
            }
            TempData["alert"] = "Registeration successful for " + userObj.Username;

            return RedirectToAction("Login");
        }


        public async Task<IActionResult> Logout()
        {
            TempData["alert"] = "User loged out";
            // we sign out the user
            await HttpContext.SignOutAsync();

            HttpContext.Session.SetString("JWToken", "");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
