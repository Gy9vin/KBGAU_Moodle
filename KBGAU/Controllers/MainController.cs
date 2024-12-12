using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KBGAU.Data;
using Microsoft.EntityFrameworkCore;


[Authorize]
public class MainController : Controller
{
    private readonly ApplicationDbContext _context;

    public MainController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
        
        string greeting;
        var now = DateTime.Now.Hour;

        if (now >= 5 && now < 12)
        {
            greeting = "Доброе утро";
        }
        else if (now >= 12 && now < 18)
        {
            greeting = "Добрый день";
        }
        else if (now >= 18 && now < 22)
        {
            greeting = "Добрый вечер";
        }
        else
        {
            greeting = "Доброй ночи";
        }

        ViewData["Greeting"] = $"{greeting}, {user.FirstName} {user.MiddleName}!";

        return View();
    }
}