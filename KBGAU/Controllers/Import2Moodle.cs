using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using KBGAU.Data;
using KBGAU.Models;
using Microsoft.AspNetCore.Mvc;

namespace Import2Moodle
{
    public class MoodleService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _context;
        private readonly string _baseUrl;
        private readonly string _token;

        public MoodleService(HttpClient httpClient, ApplicationDbContext context, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _context = context;
            _baseUrl = configuration["MoodleSettings:BaseUrl"];
            _token = configuration["MoodleSettings:Token"];
        }

        public async Task<string> FetchQuestionsFromMoodleAsync()
        {
            var url = $"{_baseUrl}/webservice/rest/server.php?wstoken={_token}&wsfunction=core_question_get_questions&moodlewsrestformat=xml";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch questions: {response.ReasonPhrase}. Details: {errorDetails}");
            }
        }


        public async Task ParseAndSaveQuestionsAsync(string xmlData)
        {
            var xdoc = XDocument.Parse(xmlData);
            foreach (var question in xdoc.Descendants("question"))
            {
                var courseInfoId = (int?)question.Element("courseinfoid") ?? 0;

                if (courseInfoId == 533)
                {
                    var typeValue = (int?)question.Element("type") ?? 0;
                    var newQuestion = new Question
                    {
                        Text = question.Element("questiontext")?.Value ?? string.Empty,
                        UserId = question.Element("userid")?.Value ?? "Unknown",
                        CourseInfoId = courseInfoId,
                        Type = (QuestionType)typeValue // Ensure the value matches the enum
                    };

                    _context.Questions.Add(newQuestion);
                    await _context.SaveChangesAsync();

                    foreach (var answer in question.Descendants("answer"))
                    {
                        var isCorrectValue = (int?)answer.Element("iscorrect") ?? 0;
                        var newAnswer = new Answer
                        {
                            IsCorrect = isCorrectValue == 1, // Convert int to bool
                            MatchText = answer.Element("matchtext")?.Value,
                            QuestionId = newQuestion.Id,
                            Text = answer.Element("text")?.Value ?? string.Empty
                        };

                        _context.Answers.Add(newAnswer);
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task CreateTestingElementAsync(int sectionId, string name)
        {
            var url = $"{_baseUrl}/webservice/rest/server.php";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("wstoken", _token),
                new KeyValuePair<string, string>("wsfunction", "core_course_create_test_element"),
                new KeyValuePair<string, string>("sectionid", sectionId.ToString()),
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("moodlewsrestformat", "json")
            });

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to create testing element: {response.ReasonPhrase}");
            }
        }
    }

    [ApiController]
    [Route("api/moodle")]
    public class MoodleController : ControllerBase
    {
        private readonly MoodleService _moodleService;

        public MoodleController(MoodleService moodleService)
        {
            _moodleService = moodleService;
        }

        [HttpPost("import-questions")]
        public async Task<IActionResult> ImportQuestions()
        {
            try
            {
                // Fetch and parse questions
                var xmlData = await _moodleService.FetchQuestionsFromMoodleAsync();
                await _moodleService.ParseAndSaveQuestionsAsync(xmlData);

                // Create a testing element in section 4147
                await _moodleService.CreateTestingElementAsync(4147, "Пробный импорт");

                return Ok("Questions imported and test created successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
