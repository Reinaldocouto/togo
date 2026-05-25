using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Togo.Api.Controllers;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Application.MedicalRecords.UseCases;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Pets;
using Togo.Application.Pets.Contracts;
using Togo.Domain.Entities;
using Togo.Domain.Enums;

namespace Togo.Api.Tests.MedicalRecords;

public sealed class MedicalRecordsControllerTests
{
    [Fact]
    public async Task Get_ShouldReturnOk_WhenMedicalRecordExists()
    {
        using var app = CreateApp();
        var patientId = 1L;
        app.PetRepository.AddPatient(patientId);
        var medicalRecord = MedicalRecord.Create(patientId, "General", "{\"risk\":false}", DateTime.UtcNow.AddMinutes(-5));
        await app.MedicalRecordRepository.AddAsync(medicalRecord);

        var response = await app.AuthorizedClient.GetAsync($"/api/patients/{patientId}/medical-record");
        var body = await response.Content.ReadFromJsonAsync<MedicalRecordResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body!.Id > 0);
        Assert.Equal(patientId, body.PatientId);
        Assert.Equal("General", body.GeneralNotes);
        Assert.Equal("{\"risk\":false}", body.FlagsJson);
        Assert.NotEqual(default, body.UpdatedAt);
    }

    [Theory]
    [InlineData("/api/patients/0/medical-record")]
    public async Task Get_ShouldReturnBadRequest_WhenPatientIdIsInvalid(string url)
    {
        using var app = CreateApp();
        var response = await app.AuthorizedClient.GetAsync(url);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_ShouldReturnUnauthorized_WithoutToken()
    {
        using var app = CreateApp();
        var response = await app.UnauthorizedClient.GetAsync("/api/patients/1/medical-record");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenPatientDoesNotExist()
    {
        using var app = CreateApp();
        var response = await app.AuthorizedClient.GetAsync("/api/patients/999/medical-record");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenMedicalRecordDoesNotExist()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(2);
        var response = await app.AuthorizedClient.GetAsync("/api/patients/2/medical-record");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_ShouldReturnCreated_WhenValid()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(3);
        var response = await app.AuthorizedClient.PostAsJsonAsync("/api/patients/3/medical-record", new CreateMedicalRecordRequest(" note ", " {\"a\":1} "));
        var body = await response.Content.ReadFromJsonAsync<MedicalRecordResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body!.Id > 0);
        Assert.Equal(3, body.PatientId);
        Assert.Equal("note", body.GeneralNotes);
        Assert.Equal("{\"a\":1}", body.FlagsJson);
        Assert.NotEqual(default, body.UpdatedAt);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Post_ShouldAcceptNullAndBlankFields_NormalizingToNull()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(4);
        var nullResponse = await app.AuthorizedClient.PostAsJsonAsync("/api/patients/4/medical-record", new CreateMedicalRecordRequest(null, null));
        var nullBody = await nullResponse.Content.ReadFromJsonAsync<MedicalRecordResponse>();
        Assert.Equal(HttpStatusCode.Created, nullResponse.StatusCode);
        Assert.Null(nullBody!.GeneralNotes);
        Assert.Null(nullBody.FlagsJson);

        app.PetRepository.AddPatient(5);
        var blankResponse = await app.AuthorizedClient.PostAsJsonAsync("/api/patients/5/medical-record", new CreateMedicalRecordRequest("   ", "   "));
        var blankBody = await blankResponse.Content.ReadFromJsonAsync<MedicalRecordResponse>();
        Assert.Equal(HttpStatusCode.Created, blankResponse.StatusCode);
        Assert.Null(blankBody!.GeneralNotes);
        Assert.Null(blankBody.FlagsJson);
    }

    [Fact] public async Task Post_ShouldReturnBadRequest_WhenPatientIdIsInvalid() { using var app = CreateApp(); var response = await app.AuthorizedClient.PostAsJsonAsync("/api/patients/0/medical-record", new CreateMedicalRecordRequest("x", "y")); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); }
    [Fact] public async Task Post_ShouldReturnUnauthorized_WithoutToken() { using var app = CreateApp(); var response = await app.UnauthorizedClient.PostAsJsonAsync("/api/patients/1/medical-record", new CreateMedicalRecordRequest("x", "y")); Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); }
    [Fact] public async Task Post_ShouldReturnNotFound_WhenPatientDoesNotExist() { using var app = CreateApp(); var response = await app.AuthorizedClient.PostAsJsonAsync("/api/patients/777/medical-record", new CreateMedicalRecordRequest("x", "y")); Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); }

    [Fact]
    public async Task Post_ShouldReturnConflict_WhenMedicalRecordAlreadyExists()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(6);
        await app.MedicalRecordRepository.AddAsync(MedicalRecord.Create(6, "existing", null, DateTime.UtcNow));
        var response = await app.AuthorizedClient.PostAsJsonAsync("/api/patients/6/medical-record", new CreateMedicalRecordRequest("new", null));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Put_ShouldReturnOk_WhenUpdated()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(7);
        var existing = MedicalRecord.Create(7, "before", "{\"v\":1}", DateTime.UtcNow.AddMinutes(-10));
        await app.MedicalRecordRepository.AddAsync(existing);
        var previousUpdatedAt = existing.UpdatedAt;

        var response = await app.AuthorizedClient.PutAsJsonAsync("/api/patients/7/medical-record", new UpdateMedicalRecordRequest(" after ", " {\"v\":2} "));
        var body = await response.Content.ReadFromJsonAsync<MedicalRecordResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(existing.Id, body!.Id);
        Assert.Equal(7, body.PatientId);
        Assert.Equal("after", body.GeneralNotes);
        Assert.Equal("{\"v\":2}", body.FlagsJson);
        Assert.True(body.UpdatedAt > previousUpdatedAt);
    }

    [Fact] public async Task Put_ShouldReturnBadRequest_WhenPatientIdIsInvalid() { using var app = CreateApp(); var response = await app.AuthorizedClient.PutAsJsonAsync("/api/patients/0/medical-record", new UpdateMedicalRecordRequest("x", "y")); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); }
    [Fact] public async Task Put_ShouldReturnUnauthorized_WithoutToken() { using var app = CreateApp(); var response = await app.UnauthorizedClient.PutAsJsonAsync("/api/patients/1/medical-record", new UpdateMedicalRecordRequest("x", "y")); Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); }
    [Fact] public async Task Put_ShouldReturnNotFound_WhenPatientDoesNotExist() { using var app = CreateApp(); var response = await app.AuthorizedClient.PutAsJsonAsync("/api/patients/999/medical-record", new UpdateMedicalRecordRequest("x", "y")); Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); }
    [Fact] public async Task Put_ShouldReturnNotFound_WhenMedicalRecordDoesNotExist() { using var app = CreateApp(); app.PetRepository.AddPatient(8); var response = await app.AuthorizedClient.PutAsJsonAsync("/api/patients/8/medical-record", new UpdateMedicalRecordRequest("x", "y")); Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); }

    [Fact]
    public async Task Put_ShouldNormalizeBlankFields_ToNull()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(9);
        await app.MedicalRecordRepository.AddAsync(MedicalRecord.Create(9, "old", "old", DateTime.UtcNow.AddMinutes(-10)));
        var response = await app.AuthorizedClient.PutAsJsonAsync("/api/patients/9/medical-record", new UpdateMedicalRecordRequest("   ", "   "));
        var body = await response.Content.ReadFromJsonAsync<MedicalRecordResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(body!.GeneralNotes);
        Assert.Null(body.FlagsJson);
    }

    private static ApiTestContext CreateApp()
    {
        var medicalRecordRepository = new InMemoryMedicalRecordRepository();
        var petRepository = new InMemoryPetRepository();

        var server = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddControllers().AddApplicationPart(typeof(MedicalRecordsController).Assembly);
                services.AddScoped<IMedicalRecordRepository>(_ => medicalRecordRepository);
                services.AddScoped<IPetRepository>(_ => petRepository);
                services.AddScoped<CreateMedicalRecordUseCase>();
                services.AddScoped<GetMedicalRecordByPatientIdUseCase>();
                services.AddScoped<UpdateMedicalRecordUseCase>();
                services.AddScoped<MedicalRecordPatientExistsValidator>();
                services.AddScoped<MedicalRecordUniquenessValidator>();
                services.AddScoped<MedicalRecordExistsValidator>();
                services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder("Test").RequireAuthenticatedUser().Build();
                });
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            }));

        var authorizedClient = server.CreateClient();
        authorizedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "ok");
        return new ApiTestContext(server, authorizedClient, server.CreateClient(), medicalRecordRepository, petRepository);
    }

    private sealed record ApiTestContext(TestServer Server, HttpClient AuthorizedClient, HttpClient UnauthorizedClient, InMemoryMedicalRecordRepository MedicalRecordRepository, InMemoryPetRepository PetRepository) : IDisposable
    {
        public void Dispose() { AuthorizedClient.Dispose(); UnauthorizedClient.Dispose(); Server.Dispose(); }
    }

    private sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.Authorization.Count == 0) return Task.FromResult(AuthenticateResult.NoResult());
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "test-user")], Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }

    private sealed class InMemoryMedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly Dictionary<long, MedicalRecord> _items = [];
        private long _nextId = 1;
        public Task<MedicalRecord?> GetByIdAsync(long id) => Task.FromResult(_items.Values.FirstOrDefault(x => x.Id == id));
        public Task<MedicalRecord?> GetByPatientIdAsync(long patientId) => Task.FromResult(_items.TryGetValue(patientId, out var value) ? value : null);
        public Task<bool> ExistsByPatientIdAsync(long patientId) => Task.FromResult(_items.ContainsKey(patientId));
        public Task AddAsync(MedicalRecord medicalRecord) { SetId(medicalRecord, _nextId++); _items[medicalRecord.PatientId] = medicalRecord; return Task.CompletedTask; }
        public Task UpdateAsync(MedicalRecord medicalRecord) { _items[medicalRecord.PatientId] = medicalRecord; return Task.CompletedTask; }

        private static void SetId(MedicalRecord medicalRecord, long id) => typeof(MedicalRecord).GetProperty(nameof(MedicalRecord.Id))!.SetValue(medicalRecord, id);
    }

    private sealed class InMemoryPetRepository : IPetRepository
    {
        private readonly HashSet<long> _patientIds = [];
        public void AddPatient(long patientId) => _patientIds.Add(patientId);
        public Task<PetDetailsProjection?> GetByPatientIdAsync(long patientId, CancellationToken cancellationToken)
        {
            if (!_patientIds.Contains(patientId)) return Task.FromResult<PetDetailsProjection?>(null);
            return Task.FromResult<PetDetailsProjection?>(new PetDetailsProjection(patientId, 1, "Pet", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)), "Active", "Dog", "Mixed", PetSex.NotInformed, null, null, DateTime.UtcNow, DateTime.UtcNow));
        }
        public Task<IReadOnlyList<PetListItemProjection>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<PetListItemProjection>>([]);
        public Task<bool> TutorExistsAsync(long tutorId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> MicrochipExistsAsync(string microchip, long? ignorePatientId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<PetDetailsProjection> CreateAsync(CreatePetRepositoryData data, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<PetDetailsProjection?> UpdateAsync(UpdatePetRepositoryData data, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(long patientId, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
