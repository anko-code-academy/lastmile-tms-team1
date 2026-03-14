using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class ZoneTests
{
    [Fact]
    public void SetBoundaryFromGeoJson_ValidPolygon_ShouldSetBoundaryGeometry()
    {
        // Arrange
        Zone zone = new();
        string validGeoJson = """
            {
                "type": "Polygon",
                "coordinates": [[
                    [-122.4194, 37.7749],
                    [-122.4094, 37.7749],
                    [-122.4094, 37.7849],
                    [-122.4194, 37.7849],
                    [-122.4194, 37.7749]
                ]]
            }
            """;

        // Act
        zone.SetBoundaryFromGeoJson(validGeoJson);

        // Assert
        zone.BoundaryGeometry.Should().NotBeNull();
        zone.BoundaryGeometry.GeometryType.Should().Be("Polygon");
    }

    [Fact]
    public void SetBoundaryFromGeoJson_NullOrWhiteSpace_ShouldThrowArgumentException()
    {
        // Arrange
        Zone zone = new();

        // Act
        Action actNull = () => zone.SetBoundaryFromGeoJson(null!);
        Action actWhiteSpace = () => zone.SetBoundaryFromGeoJson("   ");

        // Assert
        actNull.Should().Throw<ArgumentException>();
        actWhiteSpace.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetBoundaryFromGeoJson_NotPolygon_ShouldThrowArgumentException()
    {
        // Arrange
        Zone zone = new();
        string pointGeoJson = """
            {
                "type": "Point",
                "coordinates": [-122.4194, 37.7749]
            }
            """;

        // Act
        Action act = () => zone.SetBoundaryFromGeoJson(pointGeoJson);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Polygon*");
    }

    [Fact]
    public void SetBoundaryFromGeoJson_InvalidGeoJson_ShouldThrow()
    {
        // Arrange
        Zone zone = new();
        string invalidGeoJson = "not valid json";

        // Act
        Action act = () => zone.SetBoundaryFromGeoJson(invalidGeoJson);

        // Assert
        act.Should().Throw<Exception>();
    }
}
