using Togo.Domain.Entities;
using Xunit;

namespace Togo.Domain.Tests;

public class TutorTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateTutor()
    {
        // Arrange
        var name = "John Doe";
        var document = "123456789";
        var email = "john@example.com";
        var phone = "123-456-7890";
        var createdAt = DateTime.Now;

        // Act
        var tutor = Tutor.Create(name, document, email, phone, createdAt);

        // Assert
        Assert.Equal(name, tutor.Name);
        Assert.Equal(document, tutor.Document);
        Assert.Equal(email, tutor.Email);
        Assert.Equal(phone, tutor.Phone);
        Assert.Equal(createdAt, tutor.CreatedAt);
        Assert.Null(tutor.UpdatedAt);
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "";
        var document = "123456789";
        var email = "john@example.com";
        var phone = "123-456-7890";
        var createdAt = DateTime.Now;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Tutor.Create(name, document, email, phone, createdAt));
        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Create_WhitespaceName_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "   ";
        var document = "123456789";
        var email = "john@example.com";
        var phone = "123-456-7890";
        var createdAt = DateTime.Now;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Tutor.Create(name, document, email, phone, createdAt));
        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Create_DefaultDate_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "John Doe";
        var document = "123456789";
        var email = "john@example.com";
        var phone = "123-456-7890";
        var createdAt = default(DateTime);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Tutor.Create(name, document, email, phone, createdAt));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("createdAt", exception.ParamName);
    }

    [Fact]
    public void Create_NameWithWhitespace_ShouldTrimName()
    {
        // Arrange
        var name = "  John Doe  ";
        var document = "123456789";
        var email = "john@example.com";
        var phone = "123-456-7890";
        var createdAt = DateTime.Now;

        // Act
        var tutor = Tutor.Create(name, document, email, phone, createdAt);

        // Assert
        Assert.Equal("John Doe", tutor.Name);
    }

    [Fact]
    public void Create_EmptyDocument_ShouldSetToNull()
    {
        // Arrange
        var name = "John Doe";
        var document = "";
        var email = "john@example.com";
        var phone = "123-456-7890";
        var createdAt = DateTime.Now;

        // Act
        var tutor = Tutor.Create(name, document, email, phone, createdAt);

        // Assert
        Assert.Null(tutor.Document);
    }

    [Fact]
    public void Create_WhitespaceDocument_ShouldSetToNull()
    {
        // Arrange
        var name = "John Doe";
        var document = "   ";
        var email = "john@example.com";
        var phone = "123-456-7890";
        var createdAt = DateTime.Now;

        // Act
        var tutor = Tutor.Create(name, document, email, phone, createdAt);

        // Assert
        Assert.Null(tutor.Document);
    }

    [Fact]
    public void Create_DocumentWithWhitespace_ShouldTrim()
    {
        // Arrange
        var name = "John Doe";
        var document = "  123456789  ";
        var email = "john@example.com";
        var phone = "123-456-7890";
        var createdAt = DateTime.Now;

        // Act
        var tutor = Tutor.Create(name, document, email, phone, createdAt);

        // Assert
        Assert.Equal("123456789", tutor.Document);
    }

    [Fact]
    public void UpdateContact_ValidData_ShouldUpdate()
    {
        // Arrange
        var tutor = Tutor.Create("John Doe", "123", "john@example.com", "123", DateTime.Now);
        var newDocument = "987654321";
        var newEmail = "jane@example.com";
        var newPhone = "987-654-3210";
        var updatedAt = DateTime.Now.AddDays(1);

        // Act
        tutor.UpdateContact(newDocument, newEmail, newPhone, updatedAt);

        // Assert
        Assert.Equal(newDocument, tutor.Document);
        Assert.Equal(newEmail, tutor.Email);
        Assert.Equal(newPhone, tutor.Phone);
        Assert.Equal(updatedAt, tutor.UpdatedAt);
    }

    [Fact]
    public void UpdateContact_DefaultUpdatedAt_ShouldThrowArgumentException()
    {
        // Arrange
        var tutor = Tutor.Create("John Doe", "123", "john@example.com", "123", DateTime.Now);
        var updatedAt = default(DateTime);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => tutor.UpdateContact("new", "new", "new", updatedAt));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("updatedAt", exception.ParamName);
    }

    [Fact]
    public void UpdateName_ValidData_ShouldUpdate()
    {
        // Arrange
        var tutor = Tutor.Create("John Doe", "123", "john@example.com", "123", DateTime.Now);
        var newName = "Jane Doe";
        var updatedAt = DateTime.Now.AddDays(1);

        // Act
        tutor.UpdateName(newName, updatedAt);

        // Assert
        Assert.Equal(newName, tutor.Name);
        Assert.Equal(updatedAt, tutor.UpdatedAt);
    }

    [Fact]
    public void UpdateName_EmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var tutor = Tutor.Create("John Doe", "123", "john@example.com", "123", DateTime.Now);
        var newName = "";
        var updatedAt = DateTime.Now.AddDays(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => tutor.UpdateName(newName, updatedAt));
        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void UpdateName_WhitespaceName_ShouldThrowArgumentException()
    {
        // Arrange
        var tutor = Tutor.Create("John Doe", "123", "john@example.com", "123", DateTime.Now);
        var newName = "   ";
        var updatedAt = DateTime.Now.AddDays(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => tutor.UpdateName(newName, updatedAt));
        Assert.StartsWith("Name is required", exception.Message);
        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void UpdateName_DefaultUpdatedAt_ShouldThrowArgumentException()
    {
        // Arrange
        var tutor = Tutor.Create("John Doe", "123", "john@example.com", "123", DateTime.Now);
        var newName = "Jane Doe";
        var updatedAt = default(DateTime);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => tutor.UpdateName(newName, updatedAt));
        Assert.StartsWith("Date is required", exception.Message);
        Assert.Equal("updatedAt", exception.ParamName);
    }

    [Fact]
    public void UpdateName_NameWithWhitespace_ShouldTrim()
    {
        // Arrange
        var tutor = Tutor.Create("John Doe", "123", "john@example.com", "123", DateTime.Now);
        var newName = "  Jane Doe  ";
        var updatedAt = DateTime.Now.AddDays(1);

        // Act
        tutor.UpdateName(newName, updatedAt);

        // Assert
        Assert.Equal("Jane Doe", tutor.Name);
    }
}