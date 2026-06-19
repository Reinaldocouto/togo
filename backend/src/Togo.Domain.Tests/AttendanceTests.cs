using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Xunit;

namespace Togo.Domain.Tests;

public class AttendanceTests
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime TestCreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    [Fact]
    public void Create_ShouldCreateAttendance_WhenDataIsValid()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var attendance = Attendance.Create(
            patientId: 10,
            attendanceNumber: "  ATD-0001  ",
            openedAt: openedAt,
            type: AttendanceType.Consultation,
            createdByUserId: TestUserId,
            createdAtUtc: TestCreatedAt);

        // Assert
        Assert.Equal(10, attendance.PatientId);
        Assert.Equal("ATD-0001", attendance.AttendanceNumber);
        Assert.Equal(openedAt, attendance.OpenedAt);
        Assert.Null(attendance.ClosedAt);
        Assert.Equal(AttendanceStatus.Open, attendance.Status);
        Assert.Equal(AttendanceType.Consultation, attendance.Type);
        Assert.Equal(TestUserId, attendance.CreatedByUserId);
        Assert.Equal(TestCreatedAt, attendance.CreatedAt);
        Assert.Equal(TestUserId, attendance.UpdatedByUserId);
        Assert.Equal(TestCreatedAt, attendance.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowArgumentOutOfRangeException_WhenPatientIdIsInvalid(long patientId)
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Attendance.Create(
                patientId: patientId,
                attendanceNumber: "ATD-0001",
                openedAt: openedAt,
                type: AttendanceType.Consultation,
                createdByUserId: TestUserId,
                createdAtUtc: TestCreatedAt));
        Assert.StartsWith("Id must be greater than zero", exception.Message);
        Assert.Equal("patientId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenAttendanceNumberIsEmpty(string attendanceNumber)
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Attendance.Create(
                patientId: 10,
                attendanceNumber: attendanceNumber,
                openedAt: openedAt,
                type: AttendanceType.Consultation,
                createdByUserId: TestUserId,
                createdAtUtc: TestCreatedAt));
        Assert.StartsWith("Value is required", exception.Message);
        Assert.Equal("attendanceNumber", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenOpenedAtIsDefault()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Attendance.Create(
                patientId: 10,
                attendanceNumber: "ATD-0001",
                openedAt: default,
                type: AttendanceType.Consultation,
                createdByUserId: TestUserId,
                createdAtUtc: TestCreatedAt));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("openedAt", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenCreatedByUserIdIsEmpty()
    {
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);

        var exception = Assert.Throws<ArgumentException>(() =>
            Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation, Guid.Empty, TestCreatedAt));

        Assert.Equal("createdByUserId", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenCreatedAtIsDefault()
    {
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);

        var exception = Assert.Throws<ArgumentException>(() =>
            Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation, TestUserId, default));

        Assert.Equal("createdAtUtc", exception.ParamName);
    }

    [Fact]
    public void Close_ShouldCloseAttendance_WhenClosedAtIsValid()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation, TestUserId, TestCreatedAt);
        var closedAt = new DateTime(2026, 5, 11, 11, 30, 0, DateTimeKind.Utc);

        // Act
        attendance.Close(closedAt, TestUserId, TestCreatedAt.AddHours(1));

        // Assert
        Assert.Equal(closedAt, attendance.ClosedAt);
        Assert.Equal(AttendanceStatus.Closed, attendance.Status);
        Assert.Equal(TestUserId, attendance.ClosedByUserId);
        Assert.Equal(TestUserId, attendance.UpdatedByUserId);
        Assert.Equal(TestCreatedAt.AddHours(1), attendance.UpdatedAt);
        Assert.Equal(TestCreatedAt, attendance.CreatedAt);
    }

    [Fact]
    public void Close_ShouldThrowArgumentException_WhenClosedAtIsDefault()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation, TestUserId, TestCreatedAt);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => attendance.Close(default, TestUserId, TestCreatedAt.AddHours(1)));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("closedAt", exception.ParamName);
    }

    [Fact]
    public void Close_ShouldThrowArgumentException_WhenClosedAtIsBeforeOpenedAt()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation, TestUserId, TestCreatedAt);
        var invalidClosedAt = new DateTime(2026, 5, 11, 9, 59, 59, DateTimeKind.Utc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => attendance.Close(invalidClosedAt, TestUserId, TestCreatedAt.AddHours(1)));
        Assert.StartsWith("ClosedAt cannot be before OpenedAt", exception.Message);
        Assert.Equal("closedAt", exception.ParamName);
    }

    [Fact]
    public void Close_ShouldThrowInvalidOperationException_WhenAttendanceIsAlreadyClosed()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation, TestUserId, TestCreatedAt);
        var firstClosedAt = new DateTime(2026, 5, 11, 11, 30, 0, DateTimeKind.Utc);
        var secondClosedAt = new DateTime(2026, 5, 11, 12, 0, 0, DateTimeKind.Utc);

        attendance.Close(firstClosedAt, TestUserId, TestCreatedAt.AddHours(1));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => attendance.Close(secondClosedAt, TestUserId, TestCreatedAt.AddHours(2)));
    }

    [Fact]
    public void Cancel_ShouldCancelAttendance_WhenAttendanceIsOpen()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation, TestUserId, TestCreatedAt);

        // Act
        attendance.Cancel(TestUserId, TestCreatedAt.AddHours(1));

        // Assert
        Assert.Equal(AttendanceStatus.Canceled, attendance.Status);
        Assert.Null(attendance.ClosedAt);
        Assert.Equal(TestUserId, attendance.CanceledByUserId);
        Assert.Equal(TestCreatedAt.AddHours(1), attendance.CanceledAt);
        Assert.Equal(TestUserId, attendance.UpdatedByUserId);
        Assert.Equal(TestCreatedAt.AddHours(1), attendance.UpdatedAt);
        Assert.Equal(TestCreatedAt, attendance.CreatedAt);
    }

    [Fact]
    public void Cancel_ShouldThrowInvalidOperationException_WhenAttendanceIsClosed()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation, TestUserId, TestCreatedAt);
        var closedAt = new DateTime(2026, 5, 11, 11, 30, 0, DateTimeKind.Utc);
        attendance.Close(closedAt, TestUserId, TestCreatedAt.AddHours(1));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => attendance.Cancel(TestUserId, TestCreatedAt.AddHours(1)));
    }

    [Fact]
    public void Cancel_ShouldThrowInvalidOperationException_WhenAttendanceIsAlreadyCanceled()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation, TestUserId, TestCreatedAt);
        attendance.Cancel(TestUserId, TestCreatedAt.AddHours(1));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => attendance.Cancel(TestUserId, TestCreatedAt.AddHours(1)));
    }

    [Fact]
    public void Close_ShouldThrowInvalidOperationException_WhenAttendanceIsCanceled()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation, TestUserId, TestCreatedAt);
        var closedAt = new DateTime(2026, 5, 11, 11, 30, 0, DateTimeKind.Utc);
        attendance.Cancel(TestUserId, TestCreatedAt.AddHours(1));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => attendance.Close(closedAt, TestUserId, TestCreatedAt.AddHours(1)));
    }
}
