using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Togo.Api.Controllers;
using Togo.Api.Security;
using Togo.Application.Attendances.Repositories;
using Togo.Application.Auditing;
using Togo.Application.Prescriptions.Contracts;
using Togo.Application.Prescriptions.Repositories;
using Togo.Application.Prescriptions.UseCases;
using Togo.Application.Security;
using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Togo.Domain.Security;

namespace Togo.Api.Tests.Controllers;

public sealed class PrescriptionsControllerTests
{
    [Fact]
    public void Controller_ShouldRequireAuthorizationAndExposeExpectedRoute()
    {
        Assert.Contains(typeof(PrescriptionsController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true), a => a is AuthorizeAttribute);
        var route = Assert.Single(typeof(PrescriptionsController).GetCustomAttributes(typeof(RouteAttribute), inherit: true).Cast<RouteAttribute>());
        Assert.Equal("api/attendances/{attendanceId:long}/prescriptions", route.Template);
    }

    [Fact]
    public void Actions_ShouldUsePrescriptionPoliciesAndHttpVerbs()
    {
        var list = typeof(PrescriptionsController).GetMethod(nameof(PrescriptionsController.ListByAttendance))!;
        Assert.Contains(list.GetCustomAttributes(typeof(HttpGetAttribute), true), a => a is HttpGetAttribute);
        Assert.Equal(PrescriptionPolicies.Read, Assert.Single(list.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()).Policy);

        var create = typeof(PrescriptionsController).GetMethod(nameof(PrescriptionsController.Create))!;
        Assert.Contains(create.GetCustomAttributes(typeof(HttpPostAttribute), true), a => a is HttpPostAttribute);
        Assert.Equal(PrescriptionPolicies.Create, Assert.Single(create.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()).Policy);
    }

    [Fact]
    public async Task Post_ShouldReturnOkAndWriteAuditLog_WhenUserCanCreateAndAttendanceIsOpen()
    {
        using var app = CreateApp();
        app.AttendanceRepository.AddAttendance(1, AttendanceStatus.Open);

        var response = await app.AuthorizedClient.PostAsJsonAsync("/api/attendances/1/prescriptions", ValidRequest(1));
        var body = await response.Content.ReadFromJsonAsync<PrescriptionResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body!.Id > 0);
        Assert.Equal(1, body.AttendanceId);
        Assert.Single(body.Items);
        var audit = Assert.Single(app.AuditLogWriter.Events);
        Assert.Equal(PrescriptionAuditActions.Created, audit.Action);
        Assert.Equal(nameof(Prescription), audit.EntityName);
        Assert.Contains("AttendanceId", audit.MetadataJson);
        Assert.DoesNotContain("secret note", audit.MetadataJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("q12h", audit.MetadataJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact] public async Task Post_ShouldReturnBadRequest_WhenAttendanceIdIsInvalid() { using var app = CreateApp(); var r = await app.AuthorizedClient.PostAsJsonAsync("/api/attendances/0/prescriptions", ValidRequest(0)); Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode); }
    [Fact] public async Task Post_ShouldReturnBadRequest_WhenRouteAttendanceIdDiffersFromBody() { using var app = CreateApp(); var r = await app.AuthorizedClient.PostAsJsonAsync("/api/attendances/1/prescriptions", ValidRequest(2)); Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode); }
    [Fact] public async Task Post_ShouldReturnNotFound_WhenAttendanceDoesNotExist() { using var app = CreateApp(); var r = await app.AuthorizedClient.PostAsJsonAsync("/api/attendances/99/prescriptions", ValidRequest(99)); Assert.Equal(HttpStatusCode.NotFound, r.StatusCode); }

    [Theory]
    [InlineData(AttendanceStatus.Closed)]
    [InlineData(AttendanceStatus.Canceled)]
    public async Task Post_ShouldReturnConflict_WhenAttendanceIsNotOpen(AttendanceStatus status)
    {
        using var app = CreateApp();
        app.AttendanceRepository.AddAttendance(2, status);
        var response = await app.AuthorizedClient.PostAsJsonAsync("/api/attendances/2/prescriptions", ValidRequest(2));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_ShouldReturnForbidden_WhenUserCannotCreate()
    {
        using var app = CreateApp();
        app.AttendanceRepository.AddAttendance(3, AttendanceStatus.Open);
        var response = await app.CreateAuthenticatedClient(UserProfiles.Assistant).PostAsJsonAsync("/api/attendances/3/prescriptions", ValidRequest(3));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_ShouldReturnOkWithMinimalList()
    {
        using var app = CreateApp();
        app.AttendanceRepository.AddAttendance(4, AttendanceStatus.Open);
        await app.PrescriptionRepository.AddAsync(Prescription.Create(4, DateTime.UtcNow, "hidden notes"), [new PrescriptionItemDraft(123, 1, "ml", "q12h", null)], CancellationToken.None);

        var response = await app.AuthorizedClient.GetAsync("/api/attendances/4/prescriptions");
        var json = await response.Content.ReadAsStringAsync();
        var body = JsonSerializer.Deserialize<List<PrescriptionListItemResponse>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Single(body!);
        Assert.Equal(1, body![0].ItemCount);
        Assert.DoesNotContain("Notes", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dosage", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Items", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProductId", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hidden notes", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("q12h", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact] public async Task Get_ShouldReturnOkWithEmptyList() { using var app = CreateApp(); app.AttendanceRepository.AddAttendance(5, AttendanceStatus.Open); var r = await app.AuthorizedClient.GetAsync("/api/attendances/5/prescriptions"); Assert.Equal(HttpStatusCode.OK, r.StatusCode); Assert.Equal("[]", await r.Content.ReadAsStringAsync()); }
    [Fact] public async Task Get_ShouldReturnBadRequest_WhenAttendanceIdIsInvalid() { using var app = CreateApp(); var r = await app.AuthorizedClient.GetAsync("/api/attendances/0/prescriptions"); Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode); }
    [Fact] public async Task Get_ShouldReturnNotFound_WhenAttendanceDoesNotExist() { using var app = CreateApp(); var r = await app.AuthorizedClient.GetAsync("/api/attendances/404/prescriptions"); Assert.Equal(HttpStatusCode.NotFound, r.StatusCode); }
    [Fact] public async Task Get_ShouldReturnForbidden_WhenUserCannotRead() { using var app = CreateApp(); app.AttendanceRepository.AddAttendance(6, AttendanceStatus.Open); var r = await app.CreateAuthenticatedClient(UserProfiles.Reception).GetAsync("/api/attendances/6/prescriptions"); Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode); }

    [Fact]
    public void ListItemResponse_ShouldNotExposeSensitiveFields()
    {
        var properties = typeof(PrescriptionListItemResponse).GetProperties().Select(p => p.Name).ToArray();
        Assert.DoesNotContain("Notes", properties);
        Assert.DoesNotContain("Dosage", properties);
        Assert.DoesNotContain("Items", properties);
        Assert.DoesNotContain("ProductId", properties);
    }

    private static CreatePrescriptionRequest ValidRequest(long attendanceId) => new(attendanceId, DateTime.UtcNow, "secret note", [new CreatePrescriptionItemRequest(123, 1, " ml ", " q12h ", 7)]);

    private static ApiTestContext CreateApp()
    {
        var attendanceRepository = new InMemoryAttendanceRepository();
        var prescriptionRepository = new InMemoryPrescriptionRepository();
        var auditLogWriter = new InMemoryClinicalAuditLogWriter();

        var server = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddControllers().AddApplicationPart(typeof(PrescriptionsController).Assembly);
                services.AddHttpContextAccessor();
                services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
                services.AddScoped<IAttendanceRepository>(_ => attendanceRepository);
                services.AddScoped<IPrescriptionRepository>(_ => prescriptionRepository);
                services.AddScoped<IClinicalAuditLogWriter>(_ => auditLogWriter);
                services.AddScoped<CreatePrescriptionUseCase>();
                services.AddScoped<ListPrescriptionsByAttendanceUseCase>();
                services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.AddAuthorization(options => options.AddPrescriptionPolicies());
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            }));

        return new ApiTestContext(server, CreateAuthenticatedClient(server, UserProfiles.Veterinarian), attendanceRepository, prescriptionRepository, auditLogWriter);
    }

    private static HttpClient CreateAuthenticatedClient(TestServer server, string profile)
    {
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", profile);
        return client;
    }

    private sealed record ApiTestContext(TestServer Server, HttpClient AuthorizedClient, InMemoryAttendanceRepository AttendanceRepository, InMemoryPrescriptionRepository PrescriptionRepository, InMemoryClinicalAuditLogWriter AuditLogWriter) : IDisposable
    {
        public HttpClient CreateAuthenticatedClient(string profile) => PrescriptionsControllerTests.CreateAuthenticatedClient(Server, profile);
        public void Dispose() { AuthorizedClient.Dispose(); Server.Dispose(); }
    }

    private sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.Authorization.Count == 0) return Task.FromResult(AuthenticateResult.NoResult());
            var profile = Request.Headers.Authorization.ToString().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1) ?? string.Empty;
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new(TogoClaimTypes.Profile, profile) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }

    private sealed class InMemoryAttendanceRepository : IAttendanceRepository
    {
        private readonly Dictionary<long, Attendance> _items = [];
        public void AddAttendance(long id, AttendanceStatus status)
        {
            var attendance = Attendance.Create(1, $"ATT-{id}", DateTime.UtcNow.AddHours(-1), AttendanceType.Outpatient, Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), DateTime.UtcNow.AddHours(-1));
            typeof(Attendance).GetProperty(nameof(Attendance.Id))!.SetValue(attendance, id);
            if (status == AttendanceStatus.Closed) attendance.Close(DateTime.UtcNow, Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), DateTime.UtcNow);
            if (status == AttendanceStatus.Canceled) attendance.Cancel(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), DateTime.UtcNow);
            _items[id] = attendance;
        }
        public Task<Attendance?> GetByIdAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult(_items.GetValueOrDefault(id));
        public Task<IReadOnlyList<Attendance>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Attendance>>(_items.Values.ToList());
        public Task<IReadOnlyList<Attendance>> ListByPatientIdAsync(long patientId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Attendance>>([]);
        public Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsByAttendanceNumberAsync(string attendanceNumber, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> HasOpenAttendanceForPatientAsync(long patientId, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class InMemoryPrescriptionRepository : IPrescriptionRepository
    {
        private readonly List<PrescriptionListItemProjection> _items = [];
        private long _nextId = 1;
        public Task AddAsync(Prescription prescription, IReadOnlyList<PrescriptionItemDraft> items, CancellationToken cancellationToken = default)
        {
            typeof(Prescription).GetProperty(nameof(Prescription.Id))!.SetValue(prescription, _nextId++);
            _items.Add(new PrescriptionListItemProjection(prescription.Id, prescription.AttendanceId, prescription.IssuedAt, items.Count));
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<PrescriptionListItemProjection>> ListByAttendanceIdAsync(long attendanceId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<PrescriptionListItemProjection>>(_items.Where(i => i.AttendanceId == attendanceId).ToList());
    }

    private sealed class InMemoryClinicalAuditLogWriter : IClinicalAuditLogWriter
    {
        public List<ClinicalAuditEvent> Events { get; } = [];
        public Task WriteAsync(ClinicalAuditEvent auditEvent, CancellationToken cancellationToken) { Events.Add(auditEvent); return Task.CompletedTask; }
    }
}
