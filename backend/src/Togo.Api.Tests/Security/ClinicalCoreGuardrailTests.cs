using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Togo.Api.Controllers;
using Togo.Api.Models;
using Togo.Api.Security;
using Togo.Application.ClinicalEvolutions.Contracts;
using Togo.Application.MedicalRecords.Contracts;
using Togo.Application.Prescriptions.Contracts;

namespace Togo.Api.Tests.Security;

public sealed class ClinicalCoreGuardrailTests
{
    private static readonly Type[] ClinicalControllers =
    [
        typeof(AttendancesController),
        typeof(MedicalRecordsController),
        typeof(ClinicalEvolutionsController),
        typeof(PrescriptionsController)
    ];

    [Fact]
    public void ClinicalControllers_ShouldRequireAuthentication()
    {
        Assert.All(ClinicalControllers, controller =>
        {
            var authorize = Assert.Single(controller.GetCustomAttributes<AuthorizeAttribute>(inherit: true));
            Assert.Null(authorize.Policy);
        });
    }

    [Theory]
    [InlineData(typeof(AttendancesController), nameof(AttendancesController.List), AttendancePolicies.Read)]
    [InlineData(typeof(AttendancesController), nameof(AttendancesController.GetById), AttendancePolicies.Read)]
    [InlineData(typeof(AttendancesController), nameof(AttendancesController.Create), AttendancePolicies.Create)]
    [InlineData(typeof(AttendancesController), nameof(AttendancesController.Close), AttendancePolicies.Close)]
    [InlineData(typeof(AttendancesController), nameof(AttendancesController.Cancel), AttendancePolicies.Cancel)]
    [InlineData(typeof(MedicalRecordsController), nameof(MedicalRecordsController.GetByPatientIdAsync), MedicalRecordPolicies.Read)]
    [InlineData(typeof(MedicalRecordsController), nameof(MedicalRecordsController.CreateAsync), MedicalRecordPolicies.Create)]
    [InlineData(typeof(MedicalRecordsController), nameof(MedicalRecordsController.UpdateAsync), MedicalRecordPolicies.Update)]
    [InlineData(typeof(ClinicalEvolutionsController), nameof(ClinicalEvolutionsController.ListByAttendance), ClinicalEvolutionPolicies.Read)]
    [InlineData(typeof(ClinicalEvolutionsController), nameof(ClinicalEvolutionsController.Create), ClinicalEvolutionPolicies.Create)]
    [InlineData(typeof(PrescriptionsController), nameof(PrescriptionsController.ListByAttendance), PrescriptionPolicies.Read)]
    [InlineData(typeof(PrescriptionsController), nameof(PrescriptionsController.Create), PrescriptionPolicies.Create)]
    public void SensitiveClinicalEndpoints_ShouldRequireExpectedPolicy(Type controllerType, string actionName, string expectedPolicy)
    {
        var method = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(method => method.Name == actionName);

        var authorize = Assert.Single(method.GetCustomAttributes<AuthorizeAttribute>(inherit: true));

        Assert.Equal(expectedPolicy, authorize.Policy);
    }

    [Fact]
    public void PrescriptionController_ShouldRemainAttendanceScopedWithoutGlobalRoute()
    {
        var route = Assert.Single(typeof(PrescriptionsController).GetCustomAttributes<RouteAttribute>(inherit: true));
        var actions = GetHttpActions(typeof(PrescriptionsController));

        var routeTemplate = Assert.IsType<string>(route.Template);

        Assert.Equal("api/attendances/{attendanceId:long}/prescriptions", routeTemplate);
        Assert.DoesNotContain("api/prescriptions", routeTemplate, StringComparison.OrdinalIgnoreCase);
        Assert.Equal([nameof(PrescriptionsController.Create), nameof(PrescriptionsController.ListByAttendance)], actions.Select(action => action.Name).Order().ToArray());
        Assert.All(actions, action => Assert.Contains(action.GetParameters(), parameter => parameter.Name == "attendanceId" && parameter.ParameterType == typeof(long)));
    }

    [Fact]
    public void MedicalRecordResponse_ShouldExposeOnlyMinimalPublicPayload()
    {
        var properties = typeof(MedicalRecordResponse).GetProperties().Select(property => property.Name).ToArray();

        Assert.Equal(["Id", "ClinicId", "PatientId", "GeneralNotes", "FlagsJson", "UpdatedAt"], properties);
        Assert.DoesNotContain("CreatedByUserId", properties);
        Assert.DoesNotContain("UpdatedByUserId", properties);
        Assert.DoesNotContain("IsDeleted", properties);
    }

    [Fact]
    public void ClinicalEvolutionController_ShouldRemainAttendanceScopedWithoutMutationEndpointsBeyondCreate()
    {
        var route = Assert.Single(typeof(ClinicalEvolutionsController).GetCustomAttributes<RouteAttribute>(inherit: true));
        var actions = GetHttpActions(typeof(ClinicalEvolutionsController));

        var routeTemplate = Assert.IsType<string>(route.Template);

        Assert.Equal("api/attendances/{attendanceId:long}/clinical-evolutions", routeTemplate);
        Assert.Equal([nameof(ClinicalEvolutionsController.Create), nameof(ClinicalEvolutionsController.ListByAttendance)], actions.Select(action => action.Name).Order().ToArray());
        Assert.All(actions, action => Assert.Contains(action.GetParameters(), parameter => parameter.Name == "attendanceId" && parameter.ParameterType == typeof(long)));
        Assert.DoesNotContain(actions, action => action.GetCustomAttributes<HttpMethodAttribute>().Any(attribute => attribute.HttpMethods.Any(method => method is "PUT" or "PATCH" or "DELETE")));
    }

    [Fact]
    public void MinimalPublicContracts_ShouldNotExposeInternalSensitiveDtosDirectly()
    {
        AssertPublicShape<PrescriptionCreatedResponse>(["Id", "AttendanceId", "IssuedAt", "ItemCount"]);
        AssertPublicShape<PrescriptionListItemResponse>(["Id", "AttendanceId", "IssuedAt", "ItemCount"]);
        AssertPublicShape<ClinicalEvolutionListItemResponse>(["Id", "AttendanceId", "RegisteredAt", "Type"]);
        AssertPublicShape<MedicalRecordListItemResponse>(["Id", "PatientId", "UpdatedAt", "HasGeneralNotes", "HasFlags"]);

        Assert.DoesNotContain("Items", GetPropertyNames<PrescriptionCreatedResponse>());
        Assert.DoesNotContain("Notes", GetPropertyNames<PrescriptionListItemResponse>());
        Assert.DoesNotContain("Text", GetPropertyNames<ClinicalEvolutionListItemResponse>());
        Assert.DoesNotContain("GeneralNotes", GetPropertyNames<MedicalRecordListItemResponse>());
        Assert.DoesNotContain("FlagsJson", GetPropertyNames<MedicalRecordListItemResponse>());
    }

    private static MethodInfo[] GetHttpActions(Type controllerType) => controllerType
        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
        .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(inherit: true).Any())
        .ToArray();

    private static void AssertPublicShape<TContract>(string[] expectedProperties) => Assert.Equal(expectedProperties, GetPropertyNames<TContract>());

    private static string[] GetPropertyNames<TContract>() => typeof(TContract).GetProperties().Select(property => property.Name).ToArray();
}
