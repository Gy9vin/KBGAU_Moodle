using System.Collections.Generic;

namespace KBGAU.Models
{
    public class EditQuestionViewModel
    {
        public int QuestionId { get; set; }
        public int CourseInfoId { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; } // Тип вопроса
        public List<Answer> Answers { get; set; }

        // Добавляем дополнительные свойства для использования в представлении
        public bool IsTrue { get; set; } // Для типа True/False
        public string Match1 { get; set; } // Для Matching
        public string Match2 { get; set; } // Для Matching
    }
}