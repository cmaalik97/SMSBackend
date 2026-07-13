namespace Student_Management_System.Models
{
    public class Attendance
    {

        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }   // from JOIN
        public int ClassId { get; set; }
        public string? ClassName { get; set; }      // from JOIN
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } = "Present"; // Present, Absent, Late
        public int? MarkedByTeacherId { get; set; }

    }
}
