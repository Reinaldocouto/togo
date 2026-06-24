using Togo.Application.Prescriptions.Repositories;
using Togo.Application.Prescriptions.UseCases;
using Togo.Application.Tests.Attendances.Fakes;
using Togo.Application.Tests.Prescriptions.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Application.Tests.Prescriptions;

public class ListPrescriptionsByAttendanceUseCaseTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime Now = new(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc);

    [Fact] public async Task ExecuteAsync_ShouldReturnList_WhenAttendanceExists() { var (uc, repo) = CreateUseCase(); repo.AddListItem(new PrescriptionListItemProjection(1, 10, Now, 2)); var result = await uc.ExecuteAsync(10, CancellationToken.None); Assert.Equal(ApplicationResultType.Success, result.Type); var item = Assert.Single(result.Data!); Assert.Equal(2, item.ItemCount); Assert.DoesNotContain(item.GetType().GetProperties(), p => p.Name is "Notes" or "Dosage" or "Items"); }
    [Fact] public async Task ExecuteAsync_ShouldReturnEmptyList_WhenNoPrescriptions() { var (uc, _) = CreateUseCase(); var result = await uc.ExecuteAsync(10, CancellationToken.None); Assert.Equal(ApplicationResultType.Success, result.Type); Assert.Empty(result.Data!); }
    [Theory][InlineData(0)][InlineData(-1)] public async Task ExecuteAsync_ShouldValidateAttendanceId(long id) { var repo = new FakePrescriptionRepository(); var result = await new ListPrescriptionsByAttendanceUseCase(new FakeAttendanceRepository(), repo).ExecuteAsync(id, CancellationToken.None); Assert.Equal(ApplicationResultType.ValidationError, result.Type); Assert.Equal(0, repo.ListByAttendanceIdCallsCount); }
    [Fact] public async Task ExecuteAsync_ShouldReturnNotFound_WhenAttendanceMissing() { var repo = new FakePrescriptionRepository(); var result = await new ListPrescriptionsByAttendanceUseCase(new FakeAttendanceRepository(), repo).ExecuteAsync(10, CancellationToken.None); Assert.Equal(ApplicationResultType.NotFound, result.Type); Assert.Equal(0, repo.ListByAttendanceIdCallsCount); }
    [Fact] public void Repository_ShouldNotExposeGlobalListMethod() { Assert.DoesNotContain(typeof(IPrescriptionRepository).GetMethods(), method => method.Name is "ListAsync" or "GetAllAsync"); }

    private static (ListPrescriptionsByAttendanceUseCase, FakePrescriptionRepository) CreateUseCase() { var attendanceRepo = new FakeAttendanceRepository(); attendanceRepo.AddAttendanceForLookup(10, Attendance.Create(1, "ATT", Now, AttendanceType.Consultation, UserId, Now)); var repo = new FakePrescriptionRepository(); return (new ListPrescriptionsByAttendanceUseCase(attendanceRepo, repo), repo); }
}
