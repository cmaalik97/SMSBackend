namespace Student_Management_System.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Qualification { get; set; }
        public decimal Salary { get; set; }
        public DateTime JoiningDate { get; set; }
        public string? Address { get; set; }

    }
}
