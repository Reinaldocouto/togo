using Togo.Application.ClinicalEvolutions.UseCases;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.ClinicalEvolutions.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.ClinicalEvolutions;

public sealed class ListClinicalEvolutionsByAttendanceUseCaseTests
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime TestCreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime RegisteredAt = new(2026, 6, 20, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ExecuteAsync_ShouldReturnSuccessWithList_WhenAttendanceExists()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        attendanceRepository.AddAttendanceForLookup(10, CreateOpenAttendance());
        var repository = new FakeClinicalEvolutionRepository();
        repository.AddClinicalEvolution(ClinicalEvolution.Create(10, RegisteredAt, EvolutionType.ClinicalNote, "Sensitive text", TestUserId, TestCreatedAt));

        var result = await new ListClinicalEvolutionsByAttendanceUseCase(attendanceRepository, repository).ExecuteAsync(10, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        var item = Assert.Single(result.Data!);
        Assert.Equal(10, item.AttendanceId);
        Assert.DoesNotContain(item.GetType().GetProperties(), property => property.Name == "Text");
        Assert.Equal(1, repository.ListByAttendanceIdCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnEmptyList_WhenAttendanceExistsAndHasNoEvolutions()
    {
        var attendanceRepository = new FakeAttendanceRepository();
        attendanceRepository.AddAttendanceForLookup(10, CreateOpenAttendance());
        var result = await new ListClinicalEvolutionsByAttendanceUseCase(attendanceRepository, new FakeClinicalEvolutionRepository()).ExecuteAsync(10, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.Empty(result.Data!);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenAttendanceIdIsInvalid(long attendanceId)
    {
        var repository = new FakeClinicalEvolutionRepository();
        var result = await new ListClinicalEvolutionsByAttendanceUseCase(new FakeAttendanceRepository(), repository).ExecuteAsync(attendanceId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal(0, repository.ListByAttendanceIdCallsCount);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenAttendanceDoesNotExist()
    {
        var repository = new FakeClinicalEvolutionRepository();
        var result = await new ListClinicalEvolutionsByAttendanceUseCase(new FakeAttendanceRepository(), repository).ExecuteAsync(10, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.Equal(0, repository.ListByAttendanceIdCallsCount);
    }

    private static Attendance CreateOpenAttendance() => Attendance.Create(1, "ATT-001", RegisteredAt.AddHours(-1), AttendanceType.Consultation, TestUserId, TestCreatedAt);
}
