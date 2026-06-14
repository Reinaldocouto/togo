using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Tests.MedicalRecords.Fakes;
using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.MedicalRecords.Validators;

public sealed class MedicalRecordExistsValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_ShouldReturnValidationError_WhenPatientIdIsInvalid(long patientId)
    {
        var validator = new MedicalRecordExistsValidator(new FakeMedicalRecordRepository(), new TestLogger<MedicalRecordExistsValidator>());

        var result = await validator.ValidateAsync(patientId, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Patient id is invalid.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnNotFound_WhenMedicalRecordDoesNotExist()
    {
        var validator = new MedicalRecordExistsValidator(new FakeMedicalRecordRepository(), new TestLogger<MedicalRecordExistsValidator>());

        var result = await validator.ValidateAsync(55, CancellationToken.None);

        Assert.Equal(ApplicationResultType.NotFound, result.Type);
        Assert.False(result.IsSuccess);
        Assert.Equal("Medical record not found.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenMedicalRecordExists()
    {
        var repository = new FakeMedicalRecordRepository();
        repository.AddExisting(MedicalRecord.Create(55, "note", "{}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));
        var validator = new MedicalRecordExistsValidator(repository, new TestLogger<MedicalRecordExistsValidator>());

        var result = await validator.ValidateAsync(55, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ValidateAsync_ShouldPassCancellationToken_ToRepository()
    {
        var repository = new FakeMedicalRecordRepository();
        var validator = new MedicalRecordExistsValidator(repository, new TestLogger<MedicalRecordExistsValidator>());
        using var cts = new CancellationTokenSource();

        await validator.ValidateAsync(55, cts.Token);

        Assert.Equal(cts.Token, repository.LastExistsByPatientIdCancellationToken);
    }

}
