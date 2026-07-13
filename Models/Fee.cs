namespace Student_Management_System.Models
{
    public class Fee
    {
        public int FeeId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; } // from JOIN
        public string FeeType { get; set; } = string.Empty;
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string Status { get; set; } = "Unpaid"; // Paid, Unpaid, Partial

    }
}
