using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Routes.Commands;
using LastMile.TMS.Application.Features.Routes.Services;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NSubstitute;

namespace LastMile.TMS.Application.Tests;

public class OptimizeRouteStopOrderHandlerTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly IRouteStopOptimizer _optimizer;

    public OptimizeRouteStopOrderHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns("test-user");

        _context = new TestDbContext(options, currentUserService);
        _optimizer = new RouteStopOptimizer();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Handle_ValidDraftRouteWithStops_ReturnsOptimizedRoute()
    {
        // Arrange
        var (route, depot, zone) = await SeedRouteWithDepotAndStops();

        var handler = new OptimizeRouteStopOrderCommandHandler(_context, _optimizer);

        // Act
        var result = await handler.Handle(
            new OptimizeRouteStopOrderCommand(route.Id),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(route.Id);
        result.Stops.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ValidDraftRoute_UpdatesSequenceNumbers()
    {
        // Arrange
        var (route, _, _) = await SeedRouteWithDepotAndStops();

        var handler = new OptimizeRouteStopOrderCommandHandler(_context, _optimizer);

        // Act
        var result = await handler.Handle(
            new OptimizeRouteStopOrderCommand(route.Id),
            CancellationToken.None);

        // Assert
        var sequences = result.Stops.Select(s => s.SequenceNumber).ToList();
        sequences.Should().Equal([1, 2, 3]);
    }

    [Fact]
    public async Task Handle_RouteNotFound_ThrowsInvalidOperationException()
    {
        var handler = new OptimizeRouteStopOrderCommandHandler(_context, _optimizer);

        var act = () => handler.Handle(
            new OptimizeRouteStopOrderCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_RouteNotDraft_ThrowsInvalidOperationException()
    {
        // Arrange
        var route = new Route
        {
            Name = "Active Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.InProgress,
            ZoneId = Guid.NewGuid()
        };
        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        var handler = new OptimizeRouteStopOrderCommandHandler(_context, _optimizer);

        // Act
        var act = () => handler.Handle(
            new OptimizeRouteStopOrderCommand(route.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Draft*");
    }

    [Fact]
    public async Task Handle_RouteWithLessThanTwoStops_ReturnsUnchanged()
    {
        // Arrange — route with a single stop
        var depot = new Depot { Name = "D1", Address = new Address { Street1 = "1 Main", City = "City", GeoLocation = new Point(0, 0) { SRID = 4326 } } };
        var zone = new Zone { Name = "Z1", Depot = depot, DepotId = depot.Id };
        var route = new Route { Name = "R1", PlannedStartTime = DateTime.UtcNow, Status = RouteStatus.Draft, Zone = zone, ZoneId = zone.Id };
        var stop = new RouteStop { SequenceNumber = 1, Street1 = "Stop 1", GeoLocation = new Point(0.01, 0.01) { SRID = 4326 }, Route = route, RouteId = route.Id };
        route.RouteStops.Add(stop);

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        var handler = new OptimizeRouteStopOrderCommandHandler(_context, _optimizer);

        // Act
        var result = await handler.Handle(
            new OptimizeRouteStopOrderCommand(route.Id),
            CancellationToken.None);

        result.Stops.Should().HaveCount(1);
        result.Stops.First().SequenceNumber.Should().Be(1);
    }

    [Fact]
    public async Task Handle_StopsWithNullGeoLocation_Succeeds_NullsAtEnd()
    {
        // Arrange
        var depot = new Depot { Name = "D1", Address = new Address { Street1 = "1 Main", City = "City", GeoLocation = new Point(0, 0) { SRID = 4326 } } };
        var zone = new Zone { Name = "Z1", Depot = depot, DepotId = depot.Id };
        var route = new Route { Name = "R1", PlannedStartTime = DateTime.UtcNow, Status = RouteStatus.Draft, Zone = zone, ZoneId = zone.Id };

        var validStop = new RouteStop { SequenceNumber = 1, Street1 = "Valid", GeoLocation = new Point(0.01, 0.01) { SRID = 4326 }, Route = route, RouteId = route.Id };
        var nullStop = new RouteStop { SequenceNumber = 2, Street1 = "No Geo", GeoLocation = null, Route = route, RouteId = route.Id };
        route.RouteStops.Add(validStop);
        route.RouteStops.Add(nullStop);

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        var handler = new OptimizeRouteStopOrderCommandHandler(_context, _optimizer);

        // Act
        var result = await handler.Handle(
            new OptimizeRouteStopOrderCommand(route.Id),
            CancellationToken.None);

        result.Stops.Should().HaveCount(2);
        // Valid stop should come first
        result.Stops.First().Street1.Should().Be("Valid");
        result.Stops.Last().Street1.Should().Be("No Geo");
    }

    private async Task<(Route route, Depot depot, Zone zone)> SeedRouteWithDepotAndStops()
    {
        var address = new Address
        {
            Street1 = "Depot St",
            City = "Metro",
            GeoLocation = new Point(0, 0) { SRID = 4326 }
        };
        var depot = new Depot { Name = "Main Depot", Address = address };
        var zone = new Zone { Name = "Zone A", Depot = depot, DepotId = depot.Id };

        var route = new Route
        {
            Name = "Test Route",
            PlannedStartTime = DateTime.UtcNow,
            Status = RouteStatus.Draft,
            Zone = zone,
            ZoneId = zone.Id
        };

        // 3 stops at different distances from depot
        route.RouteStops.Add(new RouteStop
        {
            SequenceNumber = 1,
            Street1 = "Far Stop",
            GeoLocation = new Point(0.04, 0.04) { SRID = 4326 },
            Route = route,
            RouteId = route.Id
        });
        route.RouteStops.Add(new RouteStop
        {
            SequenceNumber = 2,
            Street1 = "Near Stop",
            GeoLocation = new Point(0.01, 0.01) { SRID = 4326 },
            Route = route,
            RouteId = route.Id
        });
        route.RouteStops.Add(new RouteStop
        {
            SequenceNumber = 3,
            Street1 = "Mid Stop",
            GeoLocation = new Point(0.02, 0.02) { SRID = 4326 },
            Route = route,
            RouteId = route.Id
        });

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        return (route, depot, zone);
    }
}
