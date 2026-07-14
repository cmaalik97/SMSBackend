# ⚙️ Student Management System — Backend (ASP.NET Core Web API)
A REST API built with C# ASP.NET Core using ADO.NET to access a SQL Server database.  
It handles authentication with JWT tokens and enforces role-based access for Admin, Teacher, and Student.
---
# 🗂 Project Structure
----
```
Student_Management_System/
├── Controllers/
│   ├── AuthController.cs        # POST /api/auth/login and /api/auth/register-admin
│   ├── DashboardController.cs   # GET  /api/dashboard/summary (Admin only)
│   ├── StudentsController.cs    # CRUD /api/students
│   ├── TeachersController.cs    # CRUD /api/teachers
│   ├── ClassesController.cs     # CRUD /api/classes
│   ├── SubjectsController.cs    # CRUD /api/subjects
│   ├── AttendanceController.cs  # CRUD /api/attendance
│   ├── FeesController.cs        # CRUD /api/fees
│   ├── ResultsController.cs     # CRUD /api/results
│   └── UsersController.cs       # CRUD /api/users (Admin only)
│
├── Repositories/
│   ├── StudentRepository.cs     # All SQL queries for Students table
│   ├── TeacherRepository.cs     # All SQL queries for Teachers table
│   ├── ClassRepository.cs       # All SQL queries for Classes table
│   ├── SubjectRepository.cs     # All SQL queries for Subjects table
│   ├── AttendanceRepository.cs  # All SQL queries for Attendance table
│   ├── FeeRepository.cs         # All SQL queries for Fees table
│   ├── ResultRepository.cs      # All SQL queries for Results table
│   └── UserRepository.cs        # All SQL queries for Users table
│
├── Models/
│   ├── Student.cs               # Student data shape (matches Students table)
│   ├── Teacher.cs               # Teacher data shape
│   ├── ClassRoom.cs             # Class data shape (ClassRoom to avoid C# keyword)
│   ├── Subject.cs               # Subject data shape
│   ├── Attendance.cs            # Attendance data shape
│   ├── Fee.cs                   # Fee data shape
│   ├── Result.cs                # Result data shape
│   └── User.cs                  # User + LoginDto + RegisterAdminDto + CreateUserDto
│
├── Data/
│   ├── DbHelper.cs              # Opens SqlConnection using the connection string
│   └── ReaderExtensions.cs      # Helper methods for reading NULL columns safely
│
├── Services/
│   └── TokenService.cs          # Creates the JWT token after successful login
│
├── Dockerfile                   # For deploying to Render.com
├── Program.cs                   # App setup: registers services, JWT, CORS, routes
└── appsettings.json             # Connection string + JWT config (empty in production)
```
---
# ⚙️ Setup & Run Locally
```bash
# 1. Make sure SQL Server is running on your PC
# 2. Run database/schema.sql in SSMS to create all tables

# 3. Open appsettings.json and set your local connection string:
{
  "ConnectionStrings": {
    "Default": "Data Source=YOUR_SERVER;Initial Catalog=StudentManagementSystem;
                Integrated Security=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "any-long-random-secret-at-least-32-characters",
    "Issuer": "SMS.Api",
    "Audience": "SMS.Client"
  }
}

# 4. Install required NuGet packages
dotnet add package Microsoft.Data.SqlClient
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next

# 5. Run the API
dotnet run
# API starts at http://localhost:5231
# Swagger UI at http://localhost:5231/swagger
```
---
# 🔐 Authentication Flow
```
React sends:  POST /api/auth/login  { email, password }
                        ↓
API checks:   UserRepository.GetByEmail(email)
                        ↓
API verifies: BCrypt.Verify(password, storedHash)
                        ↓
API creates:  JWT token containing { UserId, FullName, Email, Role }
                        ↓
React stores: token in localStorage
                        ↓
All later requests: Authorization: Bearer <token>
                        ↓
C# reads role from token → allows or blocks the action
```
# Creating User 
```
# Sign Up (`POST /api/auth/register-admin`) creates Admin accounts only.  
Teacher and Student logins are created by an Admin via `POST /api/users`.
```
---
# 🗄️ How Data Access Works (ADO.NET Pattern)
Every repository follows the same 4-step pattern your teacher showed with WinForms — just inside a Web API instead of a Form:
```csharp
// 1. Open connection
using SqlConnection conn = _db.GetConnection();

// 2. Write SQL with @parameters (never paste values directly — SQL injection risk)
string sql = "SELECT * FROM Students WHERE StudentId = @Id";
using SqlCommand cmd = new SqlCommand(sql, conn);

// 3. Add the parameter value safely
cmd.Parameters.AddWithValue("@Id", id);

// 4. Execute and read results
conn.Open();
using SqlDataReader r = cmd.ExecuteReader();
while (r.Read()) { /* map row to C# object */ }
```
# Method	When to use
```
`cmd.ExecuteReader()`	SELECT — returns rows you loop through
`cmd.ExecuteNonQuery()`	INSERT / UPDATE / DELETE — returns rows affected
`cmd.ExecuteScalar()`	SELECT that returns one value (e.g. new ID after INSERT)
----
```
# 📡 API Endpoints
```
--Auth
Method----------Endpoint----------------------Who---------Description.
** POST	        `/api/auth/login` 	         Everyone	    Login, returns JWT token
** POST	      `/api/auth/register-admin`   	  Public	        Register new Admin account
--------------
-- Dashboard
Method--------Endpoint----------------------Who----------Description
** GET	     `/api/dashboard/summary`	       Admin	    Summary numbers for dashboard cards
--------------
```
# Students, Teachers, Classes, Subjects, Fees, Results, Users
```
## Each of these 7 tables follows the same REST pattern:
Method---------Endpoint-------------Who-----------------Description
** GET	        `/api/{table}`	        Admin/Teacher	    Get all records
** GET	        `/api/{table}/{id}` 	Admin/Teacher	    Get one record
** POST	    `/api/{table}`	        Admin	            Create a record
** PUT	        `/api/{table}/{id}`	Admin	Update a record
** DELETE	`/api/{table}/{id}`	Admin	Delete a record
```
# Role rules applied automatically:
```
A Student calling `GET /api/students` only gets their own row
A Teacher calling `GET /api/teachers` only gets their own row
A Teacher calling `GET /api/subjects` only gets subjects assigned to them
`POST/PUT/DELETE` on most tables = Admin only
---------
```
# Attendance
```
Method---------Endpoint------------------Who------------------Description
** GET	        `/api/attendance`	    All logged-in	      Admin/Teacher: all records. Student: own only
** POST     	`/api/attendance`	    Admin + Teacher	      Mark attendance (auto-stamps TeacherId)
** PUT	        `/api/attendance/{id}`	Admin + Teacher	      Edit a record
** DELETE	    `/api/attendance/{id}`	Admin	              Delete a record
---
```
# 🛡️ Role-Based Access
Roles are stored in the `Roles` table (Admin, Teacher, Student).  
After login, the role is embedded inside the JWT token.  
Controllers read it with:
```csharp
var role = User.FindFirstValue(ClaimTypes.Role);
```
And restrict actions with:
```csharp
[Authorize(Roles = "Admin")]          // Admin only
[Authorize(Roles = "Admin,Teacher")]  // Admin or Teacher
[Authorize]                           // Any logged-in user
```
---
# ✅ Built-in Validations
```
Table----------------------------------Validation
**Results                            	Duplicate blocked: same student + subject + exam type + year
**Attendance                            Duplicate blocked: same student + date
**Fees	                                Duplicate blocked: same student + fee type + month
**Fees                              	Status auto-calculated: 0 paid=Unpaid, partial=Partial, full=Paid
**Users	                                Delete blocked if a Student or Teacher profile is still linked
---
```

# 📦 NuGet Packages Used
```
Package      	Purpose
`Microsoft.Data.SqlClient`	ADO.NET SQL Server driver
`Microsoft.AspNetCore.Authentication.JwtBearer`	JWT authentication
`BCrypt.Net-Next`	Password hashing (never store plain text passwords)
```
