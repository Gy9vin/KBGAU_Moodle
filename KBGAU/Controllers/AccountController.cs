using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string login, string password)
    {
        // Проверьте учетные данные пользователя и создайте ClaimsPrincipal
        if (IsValidUser(login, password))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, login)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Main");
        }

        ModelState.AddModelError(string.Empty, "Неправильный логин или пароль");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    private bool IsValidUser(string login, string password)
    {
        // Проверьте учетные данные пользователя в базе данных
        return true; // Для примера, реальная проверка должна быть на основе данных из базы
    }
}