using Microsoft.Extensions.Primitives;
using Togo.Application.Security;

namespace Togo.Api.Security;

public sealed class HttpCurrentClinicalContext : ICurrentClinicalContext
{
    public const string ClinicIdHeaderName = "X-Clinic-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _resolved;
    private long? _clinicId;

    public HttpCurrentClinicalContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? ClinicId
    {
        get
        {
            EnsureResolved();
            return _clinicId;
        }
    }

    public bool HasClinic => ClinicId.HasValue;

    public long GetRequiredClinicId()
    {
        var clinicId = ClinicId;

        return clinicId ?? throw new MissingClinicalContextException();
    }

    private void EnsureResolved()
    {
        if (_resolved)
        {
            return;
        }

        _clinicId = ResolveClinicId();
        _resolved = true;
    }

    private long? ResolveClinicId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null || !httpContext.Request.Headers.TryGetValue(ClinicIdHeaderName, out var values))
        {
            return null;
        }

        var rawValue = GetSingleHeaderValue(values);

        if (!long.TryParse(rawValue, out var clinicId) || clinicId <= 0)
        {
            throw new InvalidClinicalContextException($"Header '{ClinicIdHeaderName}' must be a positive long value.");
        }

        return clinicId;
    }

    private static string? GetSingleHeaderValue(StringValues values)
    {
        if (values.Count != 1)
        {
            throw new InvalidClinicalContextException($"Header '{ClinicIdHeaderName}' must contain a single value.");
        }

        var rawValue = values[0];

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw new InvalidClinicalContextException($"Header '{ClinicIdHeaderName}' must be a positive long value.");
        }

        return rawValue;
    }
}
