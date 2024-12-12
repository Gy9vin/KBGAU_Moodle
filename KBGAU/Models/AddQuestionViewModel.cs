using System.Collections.Generic;

namespace KBGAU.Models
{
    public class AddQuestionViewModel
    {
        public int CourseId { get; set; }
        public string UserId { get; set; }
        public List<Question> Questions { get; set; }
        public List<Question> ExistingQuestions { get; set; }
        public Question NewQuestion { get; set; }
    }
}