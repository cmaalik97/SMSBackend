namespace Student_Management_System.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? DOB { get; set; }
        public string? Gender { get; set; }
        public int? ClassId { get; set; }
        public string? ClassName { get; set; }   // filled in from a JOIN, not its own column
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? Address { get; set; }
        public DateTime AdmissionDate { get; set; }

    }
}
