using Microsoft.EntityFrameworkCore;
using Togo.Application.Services;
using Togo.Application.Tutors;
using Togo.Application.Tutors.UseCases;
using Togo.Domain.Interfaces;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Repositories;
using Togo.Infrastructure.Security;
using Togo.Infrastructure.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ====== DATABASE CONNECTION ======
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ====== DEPENDENCY INJECTION ======
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITutorRepository, TutorRepository>();
builder.Services.AddScoped<IAuthenticateUser, AuthenticateUserService>();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<ListTutorsUseCase>();
builder.Services.AddScoped<GetTutorByIdUseCase>();
builder.Services.AddScoped<CreateTutorUseCase>();
builder.Services.AddScoped<UpdateTutorUseCase>();
builder.Services.AddScoped<DeleteTutorUseCase>();
builder.Services.AddSingleton<ITokenService, InMemoryTokenService>();

// ====== CORS ======
const string AllowFrontendPolicy = "AllowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowFrontendPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ====== MVC / SWAGGER ======
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(AllowFrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
