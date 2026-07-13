namespace Student_Management_System.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string? RoleName { get; set; } // from JOIN: Admin, Teacher, Student
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

    }
    // What the Sign In page sends
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // What the Sign Up page sends (Admin self-registration only)
    public class RegisterAdminDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // What the Admin's "Add User" form (in the Users table) sends,
    // to create a login for a Teacher or Student
    public class CreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Student"; // "Admin" | "Teacher" | "Student"
    }

}
