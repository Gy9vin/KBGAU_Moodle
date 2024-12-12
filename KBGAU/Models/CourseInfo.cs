using System.Collections.Generic;

namespace KBGAU.Models
{
    public class CourseInfo
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string SubjectName { get; set; }
        public string ProgramName { get; set; }
        public int CourseNumber { get; set; }
        public int ControlPointNumber { get; set; }
        public int SemesterNumber { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>(); // Инициализация коллекции

        public int QuestionCount { get; set; }
    }
}