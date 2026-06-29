using Togo.Application.Tests.Pets.Fakes;
using Togo.Application.Tutors;
using Togo.Application.Tutors.Contracts;
using Togo.Application.Tutors.UseCases;
using Togo.Application.Tutors.Validators;
using Togo.Domain.Entities;

namespace Togo.Application.Tests.Tutors.UseCases;

public sealed class CreateTutorUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPersistClinicId_WhenRequestIsValid()
    {
        var repository = new FakeTutorRepository();
        var useCase = CreateUseCase(repository);
        var request = new CreateTutorRequest(2, "John Doe", "123", "john@example.com", "555");

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.ClinicId);
        Assert.Equal(2, repository.Tutors.Single().ClinicId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_ShouldReturnValidationError_WhenClinicIdIsInvalid(long clinicId)
    {
        var useCase = CreateUseCase(new FakeTutorRepository());
        var request = new CreateTutorRequest(clinicId, "John Doe", "123", null, null);

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.ValidationError, result.Type);
        Assert.Equal("ClinicId must be greater than zero.", result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConflict_WhenDocumentExistsInSameClinic()
    {
        var repository = new FakeTutorRepository();
        repository.Add(Tutor.Create(1, "Existing", "123", null, null, DateTime.UtcNow));
        var useCase = CreateUseCase(repository);
        var request = new CreateTutorRequest(1, "John Doe", "123", null, null);

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Conflict, result.Type);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowSameDocumentInDifferentClinics()
    {
        var repository = new FakeTutorRepository();
        repository.Add(Tutor.Create(1, "Existing", "123", null, null, DateTime.UtcNow));
        var useCase = CreateUseCase(repository);
        var request = new CreateTutorRequest(2, "John Doe", "123", null, null);

        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(ApplicationResultType.Success, result.Type);
    }

    private static CreateTutorUseCase CreateUseCase(FakeTutorRepository repository)
    {
        var validator = new TutorDocumentUniquenessValidator(
            repository,
            new TestLogger<TutorDocumentUniquenessValidator>());

        return new CreateTutorUseCase(repository, validator, new TestLogger<CreateTutorUseCase>());
    }

    private sealed class FakeTutorRepository : ITutorRepository
    {
        private readonly List<Tutor> _tutors = [];
        public IReadOnlyList<Tutor> Tutors => _tutors;

        public void Add(Tutor tutor) => _tutors.Add(tutor);

        public Task<Tutor?> GetByIdAsync(long id, CancellationToken cancellationToken) =>
            Task.FromResult(_tutors.FirstOrDefault(tutor => tutor.Id == id));

        public Task<IReadOnlyList<Tutor>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Tutor>>(_tutors);

        public Task AddAsync(Tutor tutor, CancellationToken cancellationToken)
        {
            _tutors.Add(tutor);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Tutor tutor, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DeleteAsync(Tutor tutor, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> ExistsByDocumentAsync(long clinicId, string document, long? ignoreTutorId, CancellationToken cancellationToken)
        {
            var normalizedDocument = document.Trim();
            return Task.FromResult(_tutors.Any(tutor =>
                tutor.ClinicId == clinicId &&
                tutor.Document == normalizedDocument &&
                (!ignoreTutorId.HasValue || tutor.Id != ignoreTutorId.Value)));
        }
    }
}
