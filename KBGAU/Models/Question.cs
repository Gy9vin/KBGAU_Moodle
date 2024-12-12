using System.Collections.Generic;

namespace KBGAU.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } // Убедитесь, что это поле объявлено как не-nullable
        public string UserId { get; set; }
        public int CourseInfoId { get; set; }
        public CourseInfo CourseInfo { get; set; }
        public List<Answer> Answers { get; set; }
        public QuestionType Type { get; set; }  // Добавлено поле для типа вопроса
    }

    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        Matching
    }
}