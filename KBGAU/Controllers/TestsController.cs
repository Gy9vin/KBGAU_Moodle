using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KBGAU.Data;
using KBGAU.Models;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class TestsController : Controller
{
    private readonly ApplicationDbContext _context;

    public TestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        var userId = User.Identity.Name;
        var questions = _context.Questions
            .Where(q => q.UserId == userId)
            .Include(q => q.Answers)
            .ToList();

        return View(questions);
    }

    [HttpPost]
    public async Task<IActionResult> SaveQuestion(Question question)
    {
        question.UserId = User.Identity.Name;
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return RedirectToAction("Create");
    }

    [HttpPost]
    public async Task<IActionResult> SaveFinalQuestions()
    {
        var userId = User.Identity.Name;
        var questions = _context.Questions
            .Where(q => q.UserId == userId)
            .Include(q => q.Answers)
            .ToList();

        foreach (var tempQuestion in questions)
        {
            var question = new Question
            {
                Text = tempQuestion.Text,
                UserId = tempQuestion.UserId,
                Answers = tempQuestion.Answers.Select(a => new Answer
                {
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
            };

            _context.Questions.Add(question);
        }

        _context.Questions.RemoveRange(questions);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Main");
    }
}