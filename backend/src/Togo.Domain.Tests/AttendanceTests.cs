using Togo.Domain.Entities;
using Togo.Domain.Enums;
using Xunit;

namespace Togo.Domain.Tests;

public class AttendanceTests
{
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
            type: AttendanceType.Consultation);

        // Assert
        Assert.Equal(10, attendance.PatientId);
        Assert.Equal("ATD-0001", attendance.AttendanceNumber);
        Assert.Equal(openedAt, attendance.OpenedAt);
        Assert.Null(attendance.ClosedAt);
        Assert.Equal(AttendanceStatus.Open, attendance.Status);
        Assert.Equal(AttendanceType.Consultation, attendance.Type);
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
                type: AttendanceType.Consultation));
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
                type: AttendanceType.Consultation));
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
                type: AttendanceType.Consultation));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("openedAt", exception.ParamName);
    }

    [Fact]
    public void Close_ShouldCloseAttendance_WhenClosedAtIsValid()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation);
        var closedAt = new DateTime(2026, 5, 11, 11, 30, 0, DateTimeKind.Utc);

        // Act
        attendance.Close(closedAt);

        // Assert
        Assert.Equal(closedAt, attendance.ClosedAt);
        Assert.Equal(AttendanceStatus.Closed, attendance.Status);
    }

    [Fact]
    public void Close_ShouldThrowArgumentException_WhenClosedAtIsDefault()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => attendance.Close(default));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("closedAt", exception.ParamName);
    }

    [Fact]
    public void Close_ShouldThrowArgumentException_WhenClosedAtIsBeforeOpenedAt()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation);
        var invalidClosedAt = new DateTime(2026, 5, 11, 9, 59, 59, DateTimeKind.Utc);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => attendance.Close(invalidClosedAt));
        Assert.StartsWith("ClosedAt cannot be before OpenedAt", exception.Message);
        Assert.Equal("closedAt", exception.ParamName);
    }

    [Fact]
    public void Close_ShouldThrowInvalidOperationException_WhenAttendanceIsAlreadyClosed()
    {
        // Arrange
        var openedAt = new DateTime(2026, 5, 11, 10, 0, 0, DateTimeKind.Utc);
        var attendance = Attendance.Create(10, "ATD-0001", openedAt, AttendanceType.Consultation);
        var firstClosedAt = new DateTime(2026, 5, 11, 11, 30, 0, DateTimeKind.Utc);
        var secondClosedAt = new DateTime(2026, 5, 11, 12, 0, 0, DateTimeKind.Utc);

        attendance.Close(firstClosedAt);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => attendance.Close(secondClosedAt));
    }
}
