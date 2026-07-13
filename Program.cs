using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Student_Management_System.Data;
using Student_Management_System.Repositories;
using Student_Management_System.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------- ADO.NET data access (no EF Core anywhere) ----------
builder.Services.AddScoped<DbHelper>();
builder.Services.AddScoped<StudentRepository>();
builder.Services.AddScoped<TeacherRepository>();
builder.Services.AddScoped<ClassRepository>();
builder.Services.AddScoped<SubjectRepository>();
builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddScoped<FeeRepository>();
builder.Services.AddScoped<ResultRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<TokenService>();

// ---------- JWT auth ----------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization();

// ---------- Allow the React app to call this API ----------
var allowedOrigins = new List<string>
{
    "http://localhost:5173",
    "http://localhost:3000",
};

// ALLOWED_ORIGIN is set in Render.com environment variables
var extra = builder.Configuration["ALLOWED_ORIGIN"];
if (!string.IsNullOrWhiteSpace(extra))
    allowedOrigins.Add(extra);

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowReact", policy =>
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReact");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();