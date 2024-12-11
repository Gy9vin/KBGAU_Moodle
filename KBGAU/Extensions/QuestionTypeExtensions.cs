using KBGAU.Models;

namespace KBGAU.Extensions
{
    public static class QuestionTypeExtensions
    {
        public static string ToFriendlyString(this QuestionType questionType)
        {
            return questionType switch
            {
                QuestionType.MultipleChoice => "Множественный/Одиночный выбор",
                QuestionType.TrueFalse => "Истина/Ложь",
                QuestionType.Matching => "Соответствие",
                _ => questionType.ToString()
            };
        }
    }
}