using KBGAU.Data;
using KBGAU.Models;
using KBGAU.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KBGAU.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ApplicationDbContext context, ILogger<CourseController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CourseInfo courseInfo)
        {
            courseInfo.UserId = User.Identity.Name;
            _context.CourseInfos.Add(courseInfo);
            await _context.SaveChangesAsync();
            return RedirectToAction("AddQuestion", new { courseId = courseInfo.Id });
        }
        public IActionResult AddQuestion(int courseId)
        {
            var userId = User.Identity.Name;
            var questions = _context.Questions
                .Where(q => q.UserId == userId && q.CourseInfoId == courseId)
                .Include(q => q.Answers)
                .ToList();
            var existingQuestions = _context.Questions
                .Where(q => q.UserId == userId && q.CourseInfoId == courseId)
                .Include(q => q.Answers)
                .ToList();

            var model = new AddQuestionViewModel
            {
                CourseId = courseId,
                UserId = userId,
                Questions = questions,
                ExistingQuestions = existingQuestions,
                NewQuestion = new Question { CourseInfoId = courseId }
            };
            return View(model);
        }
[HttpPost]
public async Task<IActionResult> SaveQuestion(AddQuestionViewModel model)
{
    var question = model.NewQuestion;
    question.UserId = User.Identity.Name;

    if (string.IsNullOrEmpty(question.Text))
    {
        ModelState.AddModelError("", "Поле вопроса не может быть пустым");
        return RedirectToAction("AddQuestion", new { courseId = question.CourseInfoId });
    }

    // Нормализация текста вопроса
    var normalizedText = TextUtils.NormalizeText(question.Text);

    // Извлечение вопросов
    var existingQuestions = _context.Questions
        .Where(q => q.CourseInfoId == question.CourseInfoId && q.UserId == question.UserId)
        .AsEnumerable()
        .Select(q => new { q.Id, NormalizedText = TextUtils.NormalizeText(q.Text), q.Type })
        .ToList();

    // Проверка на дубликат при совпадающих тексте и типе
    if (existingQuestions.Any(q => q.NormalizedText == normalizedText && q.Type == question.Type))
    {
        ModelState.AddModelError("", "Вопрос уже существует.");
        
        if (!ModelState.IsValid)
        {
            var questions = _context.Questions
                .Where(q => q.CourseInfoId == question.CourseInfoId && q.UserId == question.UserId)
                .Include(q => q.Answers)
                .ToList();
            model.Questions = questions;
            return View("AddQuestion", model);
        }
    }
    question.Answers = new List<Answer>();

    // В зависимости от типа вопроса формируем заново список ответов
    if (question.Type == QuestionType.MultipleChoice)
    {
        question.Answers = new List<Answer>();
        int index = 0;
        while (Request.Form.ContainsKey($"NewQuestion.Answers[{index}].Text") 
               && !string.IsNullOrEmpty(Request.Form[$"NewQuestion.Answers[{index}].Text"]))
        {
            var answer = new Answer
            {
                Text = Request.Form[$"NewQuestion.Answers[{index}].Text"],
                IsCorrect = string.Equals(Request.Form[$"NewQuestion.Answers[{index}].IsCorrect"], "true", StringComparison.OrdinalIgnoreCase)
            };

            if (string.IsNullOrEmpty(answer.Text))
            {
                ModelState.AddModelError("", "Поле ответа не может быть пустым");
                return RedirectToAction("AddQuestion", new { courseId = question.CourseInfoId });
            }

            question.Answers.Add(answer);
            index++;
        }
    }
    else if (question.Type == QuestionType.TrueFalse)
    {
        // При выборе "Истина/Ложь" сбрасываем старые ответы и создаём новые
        question.Answers = new List<Answer>
        {
            new Answer 
            { 
                Text = "Истина", 
                IsCorrect = string.Equals(Request.Form["NewQuestion.Answers[0].IsCorrect"], "true", StringComparison.OrdinalIgnoreCase) 
            },
            new Answer 
            { 
                Text = "Ложь", 
                IsCorrect = string.Equals(Request.Form["NewQuestion.Answers[0].IsCorrect"], "false", StringComparison.OrdinalIgnoreCase) 
            }
        };
    }
    else if (question.Type == QuestionType.Matching)
    {
        // Для Matching так же сбрасываем ответы и заполняем заново
        question.Answers = new List<Answer>();
        int index = 0;
        while (Request.Form.ContainsKey($"NewQuestion.Answers[{index}].Text") 
               && !string.IsNullOrEmpty(Request.Form[$"NewQuestion.Answers[{index}].Text"]))
        {
            var answer = new Answer
            {
                Text = Request.Form[$"NewQuestion.Answers[{index}].Text"],
                MatchText = Request.Form.ContainsKey($"NewQuestion.Answers[{index}].MatchText")
                    ? Request.Form[$"NewQuestion.Answers[{index}].MatchText"].FirstOrDefault()
                    : null
            };
            question.Answers.Add(answer);
            index++;
        }
    }

    _context.Questions.Add(question);
    await _context.SaveChangesAsync();

    // Обновление количества вопросов
    var course = await _context.CourseInfos.FindAsync(question.CourseInfoId);
    if (course != null)
    {
        course.QuestionCount = await _context.Questions.CountAsync(q => q.CourseInfoId == question.CourseInfoId);
        _context.Update(course);
        await _context.SaveChangesAsync();
    }

    return RedirectToAction("AddQuestion", new { courseId = question.CourseInfoId });
}

        [HttpPost]
        public async Task<IActionResult> SaveQuestionCount(int courseId, string userId)
        {
            var course = await _context.CourseInfos.FindAsync(courseId);
            if (course != null)
            {
                course.QuestionCount = await _context.Questions.CountAsync(q => q.CourseInfoId == courseId);

                _logger.LogDebug(
                    $"Updating Course: Id={course.Id}, ControlPointNumber={course.ControlPointNumber}, CourseNumber={course.CourseNumber}, ProgramName={course.ProgramName}, QuestionCount={course.QuestionCount}, SemesterNumber={course.SemesterNumber}, SubjectName={course.SubjectName}, UserId={course.UserId}");
                _context.Update(course);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Course updated successfully.");
            }
            else
            {
                _logger.LogError($"Course with Id: {courseId} not found.");
            }
            return RedirectToAction("MyCourses");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                var course = await _context.CourseInfos.FindAsync(question.CourseInfoId);
                if (course != null)
                {
                    course.QuestionCount =
                        await _context.Questions.CountAsync(q => q.CourseInfoId == question.CourseInfoId);
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("AddQuestion", new { courseId = question.CourseInfoId });
        }

        public IActionResult MyCourses()
        {
            var userId = User.Identity.Name;
            var courses = _context.CourseInfos
                .Where(c => c.UserId == userId)
                .Include(c => c.Questions)
                .ToList();
            return View(courses);
        }
        public IActionResult EditCourse(int courseId)
        {
            var course = _context.CourseInfos
                .Include(c => c.Questions)
                .FirstOrDefault(c => c.Id == courseId); 
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }
        [HttpPost]
        public async Task<IActionResult> EditCourse(CourseInfo courseInfo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogDebug($"Editing Course: {courseInfo.SubjectName}");
                    _context.Update(courseInfo);
                    await _context.SaveChangesAsync();
                    _logger.LogDebug("Course updated successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseInfoExists(courseInfo.Id))
                    {
                        _logger.LogError("Course not found.");
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("Concurrency error.");
                        throw;
                    }
                }
                return RedirectToAction("MyCourses");
            }
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError($"ModelState Error: {error.ErrorMessage}");
            }
            return View(courseInfo);
        }
        public IActionResult EditQuestion(int questionId)
        {
            var question = _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefault(q => q.Id == questionId);
            if (question == null)
            {
                return NotFound();
            }
            var model = new EditQuestionViewModel
            {
                QuestionId = question.Id,
                CourseInfoId = question.CourseInfoId,
                Text = question.Text,
                Answers = question.Answers.ToList(),
                Type = question.Type 
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditQuestion(EditQuestionViewModel model)
        {
            _logger.LogDebug("EditQuestion: POST request received");
            if (ModelState.IsValid)
            {
                var question = _context.Questions
                    .Include(q => q.Answers)
                    .FirstOrDefault(q => q.Id == model.QuestionId);
                if (question != null)
                {
                    _logger.LogDebug($"EditQuestion: Found question with ID {model.QuestionId}");
                    question.Text = model.Text;
                    question.Type = model.Type; 
                    _context.Answers.RemoveRange(question.Answers); 
                    question.Answers = model.Answers; 
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                    _logger.LogDebug("EditQuestion: Changes saved successfully");

                    return RedirectToAction("AddQuestion",
                        new { courseId = model.CourseInfoId }); 
                }
                else
                {
                    _logger.LogWarning($"EditQuestion: Question with ID {model.QuestionId} not found");
                }
            }
            else
            {
                _logger.LogWarning("EditQuestion: Model state is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"ModelState Error: {error.ErrorMessage}");
                }
            }
            return View(model); 
        }
        private bool CourseInfoExists(int id)
        {
            return _context.CourseInfos.Any(e => e.Id == id);
        }
    }
}