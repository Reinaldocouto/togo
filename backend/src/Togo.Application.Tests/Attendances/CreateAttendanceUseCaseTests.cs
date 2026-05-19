using Togo.Application.Attendances.Contracts;
using Togo.Application.Attendances.UseCases;
using Togo.Application.Attendances.Validators;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Attendances;

public sealed class CreateAttendanceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var useCase = CreateUseCase(attendanceRepository, petRepository);
        var request = CreateValidRequest(patientId: patientId);

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(patientId, result.Data.PatientId);
        Assert.Equal(request.AttendanceNumber, result.Data.AttendanceNumber);
        Assert.Equal(request.OpenedAt, result.Data.OpenedAt);
        Assert.Equal(AttendanceStatus.Open, result.Data.Status);
        Assert.Null(result.Data.ClosedAt);
        Assert.Equal(request.Type, result.Data.Type);

        Assert.Equal(1, attendanceRepository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnError_WhenPatientDoesNotExist()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var useCase = CreateUseCase(attendanceRepository, petRepository);
        var request = CreateValidRequest(patientId: 999);

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal("Patient not found.", result.Error);
        Assert.Equal(0, attendanceRepository.AddCallsCount);
        Assert.Null(attendanceRepository.LastExistsByAttendanceNumberInput);
        Assert.Null(attendanceRepository.LastHasOpenAttendancePatientIdInput);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenAttendanceNumberAlreadyExists()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        attendanceRepository.AddExistingAttendanceNumber("ATT-001");
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var useCase = CreateUseCase(attendanceRepository, petRepository);
        var request = CreateValidRequest(patientId: patientId, attendanceNumber: "ATT-001");

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.Equal("An attendance with this number already exists.", result.Error);
        Assert.Equal(0, attendanceRepository.AddCallsCount);
        Assert.Null(attendanceRepository.LastHasOpenAttendancePatientIdInput);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenPatientAlreadyHasOpenAttendance()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        attendanceRepository.AddOpenAttendancePatient(patientId);
        var useCase = CreateUseCase(attendanceRepository, petRepository);
        var request = CreateValidRequest(patientId: patientId);

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.Equal("Patient already has an open attendance.", result.Error);
        Assert.Equal(0, attendanceRepository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenAttendanceNumberIsInvalid()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var useCase = CreateUseCase(attendanceRepository, petRepository);
        var request = CreateValidRequest(patientId: patientId, attendanceNumber: "   ");

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal("Attendance number is required.", result.Error);
        Assert.Equal(0, attendanceRepository.AddCallsCount);
    }


    [Fact]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenDomainThrowsArgumentException()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var useCase = CreateUseCase(attendanceRepository, petRepository);
        var request = CreateValidRequest(patientId: patientId, openedAt: default(DateTime));

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.StartsWith("Date is required", result.Error);
        Assert.Equal(0, attendanceRepository.AddCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldTrimAttendanceNumber_WhenCreatingAttendance()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        var petRepository = new FakePetRepository();
        var patientId = petRepository.AddPet();
        var useCase = CreateUseCase(attendanceRepository, petRepository);
        var request = CreateValidRequest(patientId: patientId, attendanceNumber: "  ATT-TRIM-001  ");

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.Equal("ATT-TRIM-001", result.Data!.AttendanceNumber);

        var persisted = (await attendanceRepository.ListByPatientIdAsync(patientId, CancellationToken.None)).Single();
        Assert.Equal("ATT-TRIM-001", persisted.AttendanceNumber);
    }

    private static CreateAttendanceUseCase CreateUseCase(
        FakeAttendanceRepository attendanceRepository,
        FakePetRepository petRepository)
    {
        var patientExistsValidator = new AttendancePatientExistsValidator(
            petRepository,
            new TestLogger<AttendancePatientExistsValidator>());
        var attendanceNumberUniqueValidator = new AttendanceNumberUniqueValidator(
            attendanceRepository,
            new TestLogger<AttendanceNumberUniqueValidator>());
        var openAttendanceValidator = new OpenAttendanceValidator(
            attendanceRepository,
            new TestLogger<OpenAttendanceValidator>());

        return new CreateAttendanceUseCase(
            attendanceRepository,
            patientExistsValidator,
            attendanceNumberUniqueValidator,
            openAttendanceValidator,
            new TestLogger<CreateAttendanceUseCase>());
    }

    private static CreateAttendanceRequest CreateValidRequest(
        long patientId,
        string attendanceNumber = "ATT-001",
        DateTime? openedAt = null,
        AttendanceType type = AttendanceType.Consultation)
        => new(
            patientId,
            attendanceNumber,
            openedAt ?? new DateTime(2026, 01, 15, 10, 30, 00, DateTimeKind.Utc),
            type);
}
