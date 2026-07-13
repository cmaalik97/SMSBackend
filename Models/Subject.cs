namespace Student_Management_System.Models
{
    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public int? ClassId { get; set; }
        public string? ClassName { get; set; }   // from JOIN
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; } // from JOIN

    }
}
