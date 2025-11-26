using Microsoft.EntityFrameworkCore;
using Togo.Application.Interfaces;
using Togo.Application.Services;
using Togo.Domain.Entities;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Repositories;
using Togo.Infrastructure.Security;
using Togo.Infrastructure.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthenticateUser, AuthenticateUserService>();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddSingleton<ITokenService, InMemoryTokenService>();

var app = builder.Build();
await ApplyMigrationsAndSeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();

static async Task ApplyMigrationsAndSeedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    await dbContext.Database.MigrateAsync();

    if (!await dbContext.Users.AnyAsync())
    {
        const string defaultPassword = "ChangeMe123!";
        const string defaultEmail = "admin@togo.com";
        User.EnsurePasswordMeetsRules(defaultPassword);

        var admin = User.Create("Admin", defaultEmail, hasher.HashPassword(defaultPassword));
        await dbContext.Users.AddAsync(admin);
        await dbContext.SaveChangesAsync();
    }
}
