namespace Student_Management_System.Models
{
    public class ClassRoom
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string? RoomNo { get; set; }
        public int? ClassTeacherId { get; set; }
        public string? ClassTeacherName { get; set; } // from JOIN

    }
}
