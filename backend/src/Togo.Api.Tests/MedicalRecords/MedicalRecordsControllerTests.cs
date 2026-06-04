using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Togo.Api.Controllers;
using Togo.Api.Security;
using Togo.Application.Auditing;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.MedicalRecords.Repositories;
using Togo.Application.MedicalRecords.UseCases;
using Togo.Application.MedicalRecords.Validators;
using Togo.Application.Pets;
using Togo.Application.Pets.Contracts;
using Togo.Application.Security;
using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Togo.Domain.Security;

namespace Togo.Api.Tests.MedicalRecords;

public sealed class MedicalRecordsControllerTests
{
    [Fact]
    public async Task Get_ShouldReturnOk_WhenMedicalRecordExists()
    {
        using var app = CreateApp();
        var patientId = 1L;
        app.PetRepository.AddPatient(patientId);
        var medicalRecord = MedicalRecord.Create(patientId, "General", "{\"risk\":false}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow.AddMinutes(-5));
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
    public async Task Get_ShouldReturnForbidden_WithTokenWithoutProfileClaim()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(10);
        await app.MedicalRecordRepository.AddAsync(MedicalRecord.Create(10, "read", null, Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));

        var response = await app.CreateAuthenticatedClientWithoutProfile().GetAsync("/api/patients/10/medical-record");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(UserProfiles.Reception)]
    [InlineData(UserProfiles.ReadOnly)]
    public async Task Get_ShouldReturnForbidden_WhenProfileCannotRead(string profile)
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(11);
        await app.MedicalRecordRepository.AddAsync(MedicalRecord.Create(11, "read", null, Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));

        var response = await app.CreateAuthenticatedClient(profile).GetAsync("/api/patients/11/medical-record");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_ShouldReturnOk_WhenProfileIsAssistant()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(12);
        await app.MedicalRecordRepository.AddAsync(MedicalRecord.Create(12, "read", null, Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));

        var response = await app.CreateAuthenticatedClient(UserProfiles.Assistant).GetAsync("/api/patients/12/medical-record");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        var persisted = await app.MedicalRecordRepository.GetByPatientIdAsync(3);
        Assert.NotNull(persisted);
        Assert.Equal(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), persisted!.CreatedByUserId);
        Assert.Equal(persisted.CreatedByUserId, persisted.UpdatedByUserId);
        Assert.Equal(persisted.CreatedAt, persisted.UpdatedAt);
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


    [Theory]
    [InlineData(UserProfiles.Assistant)]
    [InlineData(UserProfiles.Reception)]
    [InlineData(UserProfiles.ReadOnly)]
    public async Task Post_ShouldReturnForbidden_WhenProfileCannotCreate(string profile)
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(13);

        var response = await app.CreateAuthenticatedClient(profile)
            .PostAsJsonAsync("/api/patients/13/medical-record", new CreateMedicalRecordRequest("x", null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_ShouldReturnForbidden_WithTokenWithoutProfileClaim()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(14);

        var response = await app.CreateAuthenticatedClientWithoutProfile()
            .PostAsJsonAsync("/api/patients/14/medical-record", new CreateMedicalRecordRequest("x", null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_ShouldReturnCreated_WhenProfileIsAdmin()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(15);

        var response = await app.CreateAuthenticatedClient(UserProfiles.Admin)
            .PostAsJsonAsync("/api/patients/15/medical-record", new CreateMedicalRecordRequest("x", null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact] public async Task Post_ShouldReturnBadRequest_WhenPatientIdIsInvalid() { using var app = CreateApp(); var response = await app.AuthorizedClient.PostAsJsonAsync("/api/patients/0/medical-record", new CreateMedicalRecordRequest("x", "y")); Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); }
    [Fact] public async Task Post_ShouldReturnUnauthorized_WithoutToken() { using var app = CreateApp(); var response = await app.UnauthorizedClient.PostAsJsonAsync("/api/patients/1/medical-record", new CreateMedicalRecordRequest("x", "y")); Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); }
    [Fact] public async Task Post_ShouldReturnNotFound_WhenPatientDoesNotExist() { using var app = CreateApp(); var response = await app.AuthorizedClient.PostAsJsonAsync("/api/patients/777/medical-record", new CreateMedicalRecordRequest("x", "y")); Assert.Equal(HttpStatusCode.NotFound, response.StatusCode); }

    [Fact]
    public async Task Post_ShouldReturnConflict_WhenMedicalRecordAlreadyExists()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(6);
        await app.MedicalRecordRepository.AddAsync(MedicalRecord.Create(6, "existing", null, Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));
        var response = await app.AuthorizedClient.PostAsJsonAsync("/api/patients/6/medical-record", new CreateMedicalRecordRequest("new", null));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Put_ShouldReturnOk_WhenUpdated()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(7);
        var existing = MedicalRecord.Create(7, "before", "{\"v\":1}", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow.AddMinutes(-10));
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
        Assert.Equal(Guid.Parse("11111111-2222-3333-4444-555555555555"), existing.CreatedByUserId);
        Assert.Equal(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), existing.UpdatedByUserId);
    }


    [Theory]
    [InlineData(UserProfiles.Assistant)]
    [InlineData(UserProfiles.Reception)]
    [InlineData(UserProfiles.ReadOnly)]
    public async Task Put_ShouldReturnForbidden_WhenProfileCannotUpdate(string profile)
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(16);
        await app.MedicalRecordRepository.AddAsync(MedicalRecord.Create(16, "before", null, Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));

        var response = await app.CreateAuthenticatedClient(profile)
            .PutAsJsonAsync("/api/patients/16/medical-record", new UpdateMedicalRecordRequest("after", null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Put_ShouldReturnForbidden_WithTokenWithoutProfileClaim()
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(17);
        await app.MedicalRecordRepository.AddAsync(MedicalRecord.Create(17, "before", null, Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));

        var response = await app.CreateAuthenticatedClientWithoutProfile()
            .PutAsJsonAsync("/api/patients/17/medical-record", new UpdateMedicalRecordRequest("after", null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(UserProfiles.Admin)]
    [InlineData(UserProfiles.Veterinarian)]
    public async Task Put_ShouldReturnOk_WhenProfileCanUpdate(string profile)
    {
        using var app = CreateApp();
        app.PetRepository.AddPatient(18);
        await app.MedicalRecordRepository.AddAsync(MedicalRecord.Create(18, "before", null, Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow));

        var response = await app.CreateAuthenticatedClient(profile)
            .PutAsJsonAsync("/api/patients/18/medical-record", new UpdateMedicalRecordRequest("after", null));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
        await app.MedicalRecordRepository.AddAsync(MedicalRecord.Create(9, "old", "old", Guid.Parse("11111111-2222-3333-4444-555555555555"), DateTime.UtcNow.AddMinutes(-10)));
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
                services.AddHttpContextAccessor();
                services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
                services.AddScoped<IMedicalRecordRepository>(_ => medicalRecordRepository);
                services.AddScoped<IClinicalAuditLogWriter>(_ => new InMemoryClinicalAuditLogWriter());
                services.AddScoped<IPetRepository>(_ => petRepository);
                services.AddScoped<CreateMedicalRecordUseCase>();
                services.AddScoped<GetMedicalRecordByPatientIdUseCase>();
                services.AddScoped<UpdateMedicalRecordUseCase>();
                services.AddScoped<MedicalRecordPatientExistsValidator>();
                services.AddScoped<MedicalRecordUniquenessValidator>();
                services.AddScoped<MedicalRecordExistsValidator>();
                services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.AddAuthorization(options => options.AddMedicalRecordPolicies());
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            }));

        var authorizedClient = CreateAuthenticatedClient(server, UserProfiles.Veterinarian);
        return new ApiTestContext(server, authorizedClient, server.CreateClient(), medicalRecordRepository, petRepository);
    }

    private static HttpClient CreateAuthenticatedClient(TestServer server, string profile)
    {
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", profile);
        return client;
    }

    private sealed record ApiTestContext(TestServer Server, HttpClient AuthorizedClient, HttpClient UnauthorizedClient, InMemoryMedicalRecordRepository MedicalRecordRepository, InMemoryPetRepository PetRepository) : IDisposable
    {
        public HttpClient CreateAuthenticatedClient(string profile) => MedicalRecordsControllerTests.CreateAuthenticatedClient(Server, profile);

        public HttpClient CreateAuthenticatedClientWithoutProfile()
        {
            var client = Server.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", TestAuthHandler.AuthenticatedWithoutProfileToken);
            return client;
        }

        public void Dispose() { AuthorizedClient.Dispose(); UnauthorizedClient.Dispose(); Server.Dispose(); }
    }

    private sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticatedWithoutProfileToken = "no-profile";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.Authorization.Count == 0) return Task.FromResult(AuthenticateResult.NoResult());

            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee") };
            var profile = Request.Headers.Authorization.ToString().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1);
            if (!string.Equals(profile, AuthenticatedWithoutProfileToken, StringComparison.Ordinal))
            {
                claims.Add(new Claim(TogoClaimTypes.Profile, profile ?? string.Empty));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
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

    private sealed class InMemoryClinicalAuditLogWriter : IClinicalAuditLogWriter
    {
        public Task WriteAsync(ClinicalAuditEvent auditEvent, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
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
