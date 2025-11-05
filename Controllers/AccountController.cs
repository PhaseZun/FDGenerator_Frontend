using Microsoft.AspNetCore.Mvc;
using MyApp.Web.Models.Auth;
using MyApp.Web.Services;

namespace MyApp.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Please enter username and password.";
                return View();
            }

              var result = await _authService.LoginAsync(request);
            if (result.Success)
            {
                if (!string.IsNullOrEmpty(result.Token))
               {
                  HttpContext.Session.SetString("Token", result.Token);
                 return RedirectToAction("Index", "FDList");
               }
            }

            ViewBag.Message = result.Message;
            Console.WriteLine("Raw API Response:" + request.Username);
            Console.WriteLine("Raw API Response:" + request.Password);
            return View();
            
        }
    }
}
