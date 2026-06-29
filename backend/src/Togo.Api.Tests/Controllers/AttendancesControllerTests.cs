using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Togo.Api.Controllers;
using Togo.Api.Security;
using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Attendances.UseCases;
using Togo.Application.Attendances.Validators;
using Togo.Application.Auditing;
using Togo.Application.Pets;
using Togo.Application.Security;
using Togo.Application.Pets.Contracts;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Api.Tests.Controllers;

public sealed class AttendancesControllerTests
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime TestCreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    [Fact]
    public void Controller_ShouldRequireAuthorization()
    {
        var attribute = Assert.Single(typeof(AttendancesController).GetCustomAttributes<AuthorizeAttribute>());

        Assert.Null(attribute.Policy);
    }

    [Theory]
    [InlineData(nameof(AttendancesController.List), AttendancePolicies.Read)]
    [InlineData(nameof(AttendancesController.GetById), AttendancePolicies.Read)]
    [InlineData(nameof(AttendancesController.Create), AttendancePolicies.Create)]
    [InlineData(nameof(AttendancesController.Close), AttendancePolicies.Close)]
    [InlineData(nameof(AttendancesController.Cancel), AttendancePolicies.Cancel)]
    public void Actions_ShouldRequireExpectedAttendancePolicy(string actionName, string expectedPolicy)
    {
        var method = typeof(AttendancesController)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.Name == actionName);

        var attribute = Assert.Single(method.GetCustomAttributes<AuthorizeAttribute>());

        Assert.Equal(expectedPolicy, attribute.Policy);
    }

    [Fact]
    public async Task List_ShouldReturnOk_WhenUseCaseReturnsSuccess()
    {
        var context = CreateControllerContext();
        await context.Repository.AddAsync(CreateAttendance("ATT-001"));
        var result = await context.Controller.List(CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        var body = Assert.IsAssignableFrom<IReadOnlyList<AttendanceListItemResponse>>(ok.Value);
        Assert.Single(body);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenAttendanceExists()
    {
        var context = CreateControllerContext();
        var attendance = CreateAttendance("ATT-001");

        context.Repository.MapAttendance(10, attendance);

        var result = await context.Controller.GetById(10, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<AttendanceResponse>(ok.Value);

        Assert.Equal(attendance.Id, body.Id);
        Assert.Equal(attendance.PatientId, body.PatientId);
        Assert.Equal("ATT-001", body.AttendanceNumber);
        Assert.Equal(AttendanceStatus.Open, body.Status);
        Assert.Equal(AttendanceType.Consultation, body.Type);
    }
    [Fact] public async Task GetById_ShouldReturnBadRequest_WhenIdIsInvalid() { var context = CreateControllerContext(); var result = await context.Controller.GetById(0, CancellationToken.None); Assert.IsType<BadRequestObjectResult>(result); }
    [Fact] public async Task GetById_ShouldReturnNotFound_WhenAttendanceDoesNotExist() { var context = CreateControllerContext(); var result = await context.Controller.GetById(999, CancellationToken.None); Assert.IsType<NotFoundObjectResult>(result); }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenRequestIsValid()
    {
        var context = CreateControllerContext(patientExists: true);
        var request = new CreateAttendanceRequest(1, "ATT-NEW", DateTime.UtcNow, AttendanceType.Consultation);
        var result = await context.Controller.Create(request, CancellationToken.None);
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(AttendancesController.GetById), created.ActionName);
        Assert.True(created.RouteValues!.ContainsKey("id"));
        var body = Assert.IsType<AttendanceResponse>(created.Value);
        Assert.Equal(created.RouteValues["id"], body.Id);
    }

    [Fact] public async Task Create_ShouldReturnBadRequest_WhenValidationFails() { var context = CreateControllerContext(patientExists: true); var request = new CreateAttendanceRequest(1, "   ", DateTime.UtcNow, AttendanceType.Consultation); var result = await context.Controller.Create(request, CancellationToken.None); Assert.IsType<BadRequestObjectResult>(result); }
    [Fact] public async Task Create_ShouldReturnNotFound_WhenPatientDoesNotExist() { var context = CreateControllerContext(patientExists: false); var request = new CreateAttendanceRequest(1, "ATT-404", DateTime.UtcNow, AttendanceType.Consultation); var result = await context.Controller.Create(request, CancellationToken.None); Assert.IsType<NotFoundObjectResult>(result); }
    [Fact] public async Task Create_ShouldReturnConflict_WhenAttendanceNumberAlreadyExistsOrPatientHasOpenAttendance() { var context = CreateControllerContext(patientExists: true); context.Repository.ExistingAttendanceNumbers.Add("ATT-CONFLICT"); var request = new CreateAttendanceRequest(1, "ATT-CONFLICT", DateTime.UtcNow, AttendanceType.Consultation); var result = await context.Controller.Create(request, CancellationToken.None); Assert.IsType<ConflictObjectResult>(result); }

    [Fact] public async Task Close_ShouldReturnOk_WhenAttendanceIsClosed() { var context = CreateControllerContext(); var attendance = CreateAttendance("ATT-CLOSE"); context.Repository.MapAttendance(20, attendance); var result = await context.Controller.Close(20, new CloseAttendanceRequest(DateTime.UtcNow.AddMinutes(5)), CancellationToken.None); var ok = Assert.IsType<OkObjectResult>(result); var body = Assert.IsType<AttendanceResponse>(ok.Value); Assert.Equal(AttendanceStatus.Closed, body.Status); }
    [Fact] public async Task Close_ShouldReturnBadRequest_WhenValidationFails() { var context = CreateControllerContext(); var attendance = CreateAttendance("ATT-CLOSE-INVALID"); context.Repository.MapAttendance(21, attendance); var result = await context.Controller.Close(21, new CloseAttendanceRequest(default), CancellationToken.None); Assert.IsType<BadRequestObjectResult>(result); }
    [Fact] public async Task Close_ShouldReturnNotFound_WhenAttendanceDoesNotExist() { var context = CreateControllerContext(); var result = await context.Controller.Close(999, new CloseAttendanceRequest(DateTime.UtcNow), CancellationToken.None); Assert.IsType<NotFoundObjectResult>(result); }
    [Fact] public async Task Close_ShouldReturnConflict_WhenAttendanceCannotBeClosed() { var context = CreateControllerContext(); var attendance = CreateAttendance("ATT-CLOSE-CONFLICT"); attendance.Cancel(TestUserId, TestCreatedAt.AddHours(1)); context.Repository.MapAttendance(22, attendance); var result = await context.Controller.Close(22, new CloseAttendanceRequest(DateTime.UtcNow), CancellationToken.None); Assert.IsType<ConflictObjectResult>(result); }

    [Fact] public async Task Cancel_ShouldReturnOk_WhenAttendanceIsCanceled() { var context = CreateControllerContext(); var attendance = CreateAttendance("ATT-CANCEL"); context.Repository.MapAttendance(30, attendance); var result = await context.Controller.Cancel(30, CancellationToken.None); var ok = Assert.IsType<OkObjectResult>(result); var body = Assert.IsType<AttendanceResponse>(ok.Value); Assert.Equal(AttendanceStatus.Canceled, body.Status); }
    [Fact] public async Task Cancel_ShouldReturnBadRequest_WhenIdIsInvalid() { var context = CreateControllerContext(); var result = await context.Controller.Cancel(0, CancellationToken.None); Assert.IsType<BadRequestObjectResult>(result); }
    [Fact] public async Task Cancel_ShouldReturnNotFound_WhenAttendanceDoesNotExist() { var context = CreateControllerContext(); var result = await context.Controller.Cancel(999, CancellationToken.None); Assert.IsType<NotFoundObjectResult>(result); }
    [Fact] public async Task Cancel_ShouldReturnConflict_WhenAttendanceCannotBeCanceled() { var context = CreateControllerContext(); var attendance = CreateAttendance("ATT-CANCEL-CONFLICT"); attendance.Close(DateTime.UtcNow.AddMinutes(3), TestUserId, TestCreatedAt.AddHours(1)); context.Repository.MapAttendance(31, attendance); var result = await context.Controller.Cancel(31, CancellationToken.None); Assert.IsType<ConflictObjectResult>(result); }

    private static TestContext CreateControllerContext(bool patientExists = true)
    {
        var repository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository(patientExists);
        var auditLogWriter = new FakeClinicalAuditLogWriter();
        var createUseCase = new CreateAttendanceUseCase(repository, new AttendancePatientExistsValidator(petRepository, new NullLogger<AttendancePatientExistsValidator>()), new AttendanceNumberUniqueValidator(repository, new NullLogger<AttendanceNumberUniqueValidator>()), new OpenAttendanceValidator(repository, new NullLogger<OpenAttendanceValidator>()), new FakeCurrentUserService(), auditLogWriter, new NullLogger<CreateAttendanceUseCase>());
        var controller = new AttendancesController(createUseCase, new GetAttendanceByIdUseCase(repository, new NullLogger<GetAttendanceByIdUseCase>()), new ListAttendancesUseCase(repository, new NullLogger<ListAttendancesUseCase>()), new CloseAttendanceUseCase(repository, new FakeCurrentUserService(), auditLogWriter, new NullLogger<CloseAttendanceUseCase>()), new CancelAttendanceUseCase(repository, new FakeCurrentUserService(), auditLogWriter, new NullLogger<CancelAttendanceUseCase>()), new NullLogger<AttendancesController>());
        return new TestContext(controller, repository);
    }

    private static Attendance CreateAttendance(string number) => Attendance.Create(1, number, DateTime.UtcNow, AttendanceType.Consultation, TestUserId, TestCreatedAt);
    private sealed record TestContext(AttendancesController Controller, FakeAttendanceRepository Repository);

    private sealed class NullLogger<T> : ILogger<T>
    { public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null; public bool IsEnabled(LogLevel logLevel) => false; public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { } }

    private sealed class FakeAttendanceRepository : IAttendanceRepository
    {
        private readonly Dictionary<long, Attendance> _byId = [];
        private readonly List<Attendance> _items = [];
        private long _nextId = 100;
        public HashSet<string> ExistingAttendanceNumbers { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<long> PatientsWithOpenAttendance { get; } = [];
        public void MapAttendance(long id, Attendance attendance) => _byId[id] = attendance;
        public Task<Attendance?> GetByIdAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult(_byId.TryGetValue(id, out var value) ? value : _items.FirstOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<Attendance>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Attendance>>(_items.ToList());
        public Task<IReadOnlyList<Attendance>> ListByPatientIdAsync(long patientId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Attendance>>(_items.Where(x => x.PatientId == patientId).ToList());
        public Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default) { _items.Add(attendance); ExistingAttendanceNumbers.Add(attendance.AttendanceNumber); MapAttendance(_nextId++, attendance); return Task.CompletedTask; }
        public Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExistsByAttendanceNumberAsync(string attendanceNumber, CancellationToken cancellationToken = default) => Task.FromResult(ExistingAttendanceNumbers.Contains(attendanceNumber));
        public Task<bool> HasOpenAttendanceForPatientAsync(long patientId, CancellationToken cancellationToken = default) => Task.FromResult(PatientsWithOpenAttendance.Contains(patientId));
    }
    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public CurrentUserInfo GetCurrentUser() => new(TestUserId, Profile: "Veterinarian", IsAuthenticated: true);
    }

    private sealed class FakeClinicalAuditLogWriter : IClinicalAuditLogWriter
    {
        public Task WriteAsync(ClinicalAuditEvent auditEvent, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakePetRepository(bool patientExists) : IPetRepository
    {
        public Task<IReadOnlyList<PetListItemProjection>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<PetListItemProjection>>([]);
        public Task<PetDetailsProjection?> GetByPatientIdAsync(long patientId, CancellationToken cancellationToken)
        {
            if (!patientExists)
            {
                return Task.FromResult<PetDetailsProjection?>(null);
            }

            var projection = new PetDetailsProjection(
                PatientId: patientId,
                ClinicId: 1,
                TutorId: 1,
                Name: "Pet",
                BirthDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                Status: "Active",
                Species: "Dog",
                Breed: "Breed",
                Sex: PetSex.NotInformed,
                WeightKg: null,
                Microchip: null,
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow);

            return Task.FromResult<PetDetailsProjection?>(projection);
        }
        public Task<bool> TutorExistsAsync(long tutorId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> TutorBelongsToClinicAsync(long tutorId, long clinicId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> MicrochipExistsAsync(string microchip, long? ignorePatientId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<PetDetailsProjection> CreateAsync(CreatePetRepositoryData data, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PetDetailsProjection?> UpdateAsync(UpdatePetRepositoryData data, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(long patientId, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
