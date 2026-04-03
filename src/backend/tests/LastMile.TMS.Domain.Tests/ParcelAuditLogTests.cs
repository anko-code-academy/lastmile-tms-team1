using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class ParcelAuditLogTests
{
    [Fact]
    public void Create_ShouldStoreBeforeAndAfterValues()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        const string propertyName = "Description";
        const string oldValue = "Old description";
        const string newValue = "New description";
        const string changedBy = "test-user";

        // Act
        var auditLog = ParcelAuditLog.Create(
            parcelId,
            propertyName,
            oldValue,
            newValue,
            changedBy);

        // Assert
        auditLog.ParcelId.Should().Be(parcelId);
        auditLog.PropertyName.Should().Be(propertyName);
        auditLog.OldValue.Should().Be(oldValue);
        auditLog.NewValue.Should().Be(newValue);
        auditLog.ChangedBy.Should().Be(changedBy);
    }

    [Fact]
    public void Create_ShouldAllowNullOldValue()
    {
        // Act
        var auditLog = ParcelAuditLog.Create(
            Guid.NewGuid(),
            "Description",
            null,
            "New description",
            "test-user");

        // Assert
        auditLog.OldValue.Should().BeNull();
        auditLog.NewValue.Should().Be("New description");
    }

    [Fact]
    public void Create_ShouldAllowNullNewValue()
    {
        // Act
        var auditLog = ParcelAuditLog.Create(
            Guid.NewGuid(),
            "Description",
            "Old description",
            null,
            "test-user");

        // Assert
        auditLog.OldValue.Should().Be("Old description");
        auditLog.NewValue.Should().BeNull();
    }
}
