using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Togo.Api.Middlewares;
using Togo.Api.Security;
using Togo.Application.Services;
using Togo.Application.Auditing;
using Togo.Application.Security;
using Togo.Application.Attendances.UseCases;
using Togo.Application.Attendances.Validators;
using Togo.Application.Attendances.Repositories;
using Togo.Application.ClinicalEvolutions.Repositories;
using Togo.Application.ClinicalEvolutions.UseCases;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Application.MedicalRecords.UseCases;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Pets;
using Togo.Application.Prescriptions.Repositories;
using Togo.Application.Prescriptions.UseCases;
using Togo.Application.Pets.UseCases;
using Togo.Application.Pets.Validators;
using Togo.Application.Tutors;
using Togo.Application.Tutors.UseCases;
using Togo.Application.Tutors.Validators;
using Togo.Domain.Interfaces;
using Togo.Infrastructure.Persistence;
using Togo.Infrastructure.Auditing;
using Togo.Infrastructure.Repositories;
using Togo.Infrastructure.Security;
using Togo.Infrastructure.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ====== DATABASE CONNECTION ======
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is not configured.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is not configured.");
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT secret is not configured.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

// ====== DEPENDENCY INJECTION ======
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITutorRepository, TutorRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IClinicalEvolutionRepository, ClinicalEvolutionRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IClinicalAuditLogWriter, EfClinicalAuditLogWriter>();
builder.Services.AddScoped<IAuthenticateUser, AuthenticateUserService>();
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<ListTutorsUseCase>();
builder.Services.AddScoped<GetTutorByIdUseCase>();
builder.Services.AddScoped<CreateTutorUseCase>();
builder.Services.AddScoped<UpdateTutorUseCase>();
builder.Services.AddScoped<CreatePetUseCase>();
builder.Services.AddScoped<ListPetsUseCase>();
builder.Services.AddScoped<GetPetByIdUseCase>();
builder.Services.AddScoped<UpdatePetUseCase>();
builder.Services.AddScoped<DeletePetUseCase>();
builder.Services.AddScoped<CreateAttendanceUseCase>();
builder.Services.AddScoped<GetAttendanceByIdUseCase>();
builder.Services.AddScoped<ListAttendancesUseCase>();
builder.Services.AddScoped<CloseAttendanceUseCase>();
builder.Services.AddScoped<CancelAttendanceUseCase>();
builder.Services.AddScoped<CreateClinicalEvolutionUseCase>();
builder.Services.AddScoped<ListClinicalEvolutionsByAttendanceUseCase>();
builder.Services.AddScoped<CreatePrescriptionUseCase>();
builder.Services.AddScoped<ListPrescriptionsByAttendanceUseCase>();
builder.Services.AddScoped<CreateMedicalRecordUseCase>();
builder.Services.AddScoped<GetMedicalRecordByPatientIdUseCase>();
builder.Services.AddScoped<UpdateMedicalRecordUseCase>();
builder.Services.AddScoped<SoftDeleteMedicalRecordUseCase>();
builder.Services.AddScoped<AttendancePatientExistsValidator>();
builder.Services.AddScoped<AttendanceNumberUniqueValidator>();
builder.Services.AddScoped<OpenAttendanceValidator>();
builder.Services.AddScoped<MedicalRecordPatientExistsValidator>();
builder.Services.AddScoped<MedicalRecordUniquenessValidator>();
builder.Services.AddScoped<MedicalRecordExistsValidator>();
builder.Services.AddScoped<TutorDocumentUniquenessValidator>();
builder.Services.AddScoped<PetTutorExistsValidator>();
builder.Services.AddScoped<PetMicrochipUniquenessValidator>();
builder.Services.AddScoped<DeleteTutorUseCase>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddMedicalRecordPolicies();
    options.AddAttendancePolicies();
    options.AddClinicalEvolutionPolicies();
});

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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {seu_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

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
