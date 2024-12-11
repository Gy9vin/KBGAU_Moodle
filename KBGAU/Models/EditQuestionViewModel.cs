using System.Collections.Generic;

namespace KBGAU.Models
{
    public class EditQuestionViewModel
    {
        public int QuestionId { get; set; }
        public int CourseInfoId { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; } // Добавьте это поле
        public List<Answer> Answers { get; set; }
    }
}