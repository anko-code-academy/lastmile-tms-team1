using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Manifests.Commands;
using LastMile.TMS.Application.Features.Manifests.Commands.CreateManifest;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Features.Manifests.Commands.CreateManifest;

public class CreateManifestCommandHandlerTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly CreateManifestCommandHandler _sut;

    public CreateManifestCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentUserService.UserId.Returns("test-user");

        _context = new TestDbContext(options, _currentUserService);
        _sut = new CreateManifestCommandHandler(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_ValidRequest_CreatesManifestWithItems()
    {
        // Arrange
        var depot = CreateDepot();
        _context.Depots.Add(depot);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var command = new CreateManifestCommand(
            "Truck Arrival #1", depot.Id,
            ["LM-260411-ABC123", "LM-260411-DEF456"]);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Truck Arrival #1");
        result.Status.Should().Be(ManifestStatus.Open);
        result.DepotId.Should().Be(depot.Id);
        result.ExpectedCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(i => i.Status == ManifestItemStatus.Expected);
    }

    [Fact]
    public async Task Handle_DepotNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = new CreateManifestCommand(
            "Test", Guid.NewGuid(), ["LM-260411-ABC123"]);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Depot*not found*");
    }

    private static Depot CreateDepot()
    {
        return new Depot
        {
            Name = "Test Depot",
            IsActive = true,
            Address = new Address
            {
                Street1 = "123 Main St",
                City = "Almaty",
                State = "Almaty",
                PostalCode = "050000",
                CountryCode = "KZ"
            }
        };
    }
}
