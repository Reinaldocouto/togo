using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tests.MedicalRecords.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.MedicalRecords.Validators;

public sealed class MedicalRecordUniquenessValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid(long patientId)
    {
        var validator = new MedicalRecordUniquenessValidator(new FakeMedicalRecordRepository(), new TestLogger<MedicalRecordUniquenessValidator>());

        var result = await validator.ValidateAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Patient id is invalid.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnConflict_WhenPatientAlreadyHasMedicalRecord()
    {
        var repository = new FakeMedicalRecordRepository();
        repository.AddExisting(MedicalRecord.Create(10, "note", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));
        var validator = new MedicalRecordUniquenessValidator(repository, new TestLogger<MedicalRecordUniquenessValidator>());

        var result = await validator.ValidateAsync(10, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Patient already has a medical record.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenPatientDoesNotHaveMedicalRecord()
    {
        var validator = new MedicalRecordUniquenessValidator(new FakeMedicalRecordRepository(), new TestLogger<MedicalRecordUniquenessValidator>());

        var result = await validator.ValidateAsync(10, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ValidateAsync_ShouldPassCancellationToken_ToRepository()
    {
        var repository = new FakeMedicalRecordRepository();
        var validator = new MedicalRecordUniquenessValidator(repository, new TestLogger<MedicalRecordUniquenessValidator>());
        using var cts = new CancellationTokenSource();

        await validator.ValidateAsync(55, cts.Token);

        Assert.Equal(cts.Token, repository.LastExistsIncludingSoftDeletedByPatientIdCancellationToken);
    }

}
