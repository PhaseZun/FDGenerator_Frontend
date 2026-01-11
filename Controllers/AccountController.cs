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
            HttpContext.Session.GetString("userId");
            HttpContext.Session.GetString("Token");
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

        LoginResponse? result;
        try
        {
            result = await _authService.LoginAsync(request);
        }
        catch (UnauthorizedAccessException ex)
        {
            ViewBag.Message = ex.Message;
            return View();
        }

        // Login failed (no token returned)
        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            ViewBag.Message = "Invalid username or password.";
            return View();
        }

        // Login success
        HttpContext.Session.SetString("UserId", result.UserId.ToString());
        HttpContext.Session.SetString("Token", result.Token);

        return RedirectToAction(
            "Index",
            "FDList",
            new { token = result.Token, userId = result.UserId }
        );
    }

    }
}
