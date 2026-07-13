namespace Student_Management_System.Models
{
    public class Result
    {
        public int ResultId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }  // from JOIN
        public int SubjectId { get; set; }
        public string? SubjectName { get; set; }  // from JOIN
        public string ExamType { get; set; } = string.Empty;
        public decimal Marks { get; set; }
        public decimal MaxMarks { get; set; } = 100;
        public string? Grade { get; set; }
        public DateTime ExamDate { get; set; }
    }
}
