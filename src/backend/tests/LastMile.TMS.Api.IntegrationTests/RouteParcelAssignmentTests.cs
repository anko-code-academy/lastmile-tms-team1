using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class RouteParcelAssignmentTests : IAsyncLifetime
{
    private readonly IntegrationFixture _fx;
    private Guid _routeId;
    private Guid _sortedParcelId;
    private Guid _stagedParcelId;
    private Guid _registeredParcelId;
    private Guid _depotId;
    private Guid _zoneId;

    public RouteParcelAssignmentTests(IntegrationFixture fx)
    {
        _fx = fx;
    }

    public async Task InitializeAsync()
    {
        using var scope = _fx.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create depot with address
        var depot = new Depot
        {
            Name = "Route Parcel Test Depot",
            Address = new Address
            {
                Street1 = "1 Test Depot Rd",
                City = "TestCity",
                State = "TS",
                PostalCode = "00000",
                CountryCode = "US",
                GeoLocation = new Point(0, 0) { SRID = 4326 }
            }
        };
        db.Depots.Add(depot);
        await db.SaveChangesAsync();
        _depotId = depot.Id;

        // Create zone
        var zone = new Zone
        {
            Name = "Route Parcel Test Zone",
            DepotId = depot.Id
        };
        db.Zones.Add(zone);
        await db.SaveChangesAsync();
        _zoneId = zone.Id;

        // Create addresses
        var shipperAddress = new Address
        {
            Street1 = "100 Shipper St",
            City = "ShipCity",
            State = "CA",
            PostalCode = "90210",
            CountryCode = "US"
        };

        var recipientAddress = new Address
        {
            Street1 = "200 Recipient Ave",
            City = "RecipCity",
            State = "NY",
            PostalCode = "10001",
            CountryCode = "US",
            GeoLocation = new Point(-73.985, 40.748) { SRID = 4326 }
        };

        db.Addresses.AddRange(shipperAddress, recipientAddress);
        await db.SaveChangesAsync();

        // Create parcels in different statuses
        var sortedParcel = Parcel.Create("Sorted parcel", ServiceType.Standard);
        sortedParcel.ShipperAddressId = shipperAddress.Id;
        sortedParcel.RecipientAddressId = recipientAddress.Id;
        sortedParcel.ZoneId = zone.Id;
        sortedParcel.Status = ParcelStatus.Sorted;
        _sortedParcelId = sortedParcel.Id;

        var stagedParcel = Parcel.Create("Staged parcel", ServiceType.Standard);
        stagedParcel.ShipperAddressId = shipperAddress.Id;
        stagedParcel.RecipientAddressId = recipientAddress.Id;
        stagedParcel.ZoneId = zone.Id;
        stagedParcel.Status = ParcelStatus.Staged;
        _stagedParcelId = stagedParcel.Id;

        var registeredParcel = Parcel.Create("Registered parcel", ServiceType.Standard);
        registeredParcel.ShipperAddressId = shipperAddress.Id;
        registeredParcel.RecipientAddressId = recipientAddress.Id;
        registeredParcel.ZoneId = zone.Id;
        registeredParcel.Status = ParcelStatus.Registered;
        _registeredParcelId = registeredParcel.Id;

        db.Parcels.AddRange(sortedParcel, stagedParcel, registeredParcel);
        await db.SaveChangesAsync();

        // Create a draft route via GraphQL
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Route Parcel Assignment Test"",
                    plannedStartTime: ""{DateTimeOffset.UtcNow.AddDays(7):O}"",
                    zoneId: ""{_zoneId}""
                }}) {{ id }}
            }}";
        var response = await _fx.ExecuteGraphQLAsync(createMutation);
        var json = await IntegrationFixture.ReadJsonAsync(response);
        _routeId = Guid.Parse(json.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString()!);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddParcelsToRoute_WithSortedParcel_KeepsStatusUnchanged()
    {
        var mutation = $@"
            mutation {{
                addParcelsToRoute(input: {{
                    routeId: ""{_routeId}"",
                    parcelIds: [""{_sortedParcelId}""]
                }}) {{
                    id
                    stops {{
                        parcels {{
                            id
                            status
                        }}
                    }}
                }}
            }}";
        var response = await _fx.ExecuteGraphQLAsync(mutation);
        var json = await IntegrationFixture.ReadJsonAsync(response);

        json.RootElement.TryGetProperty("errors", out var errors).Should().BeFalse();
        var stops = json.RootElement.GetProperty("data").GetProperty("addParcelsToRoute").GetProperty("stops");
        var parcels = stops.EnumerateArray().SelectMany(s => s.GetProperty("parcels").EnumerateArray()).ToList();
        parcels.Should().ContainSingle(p => p.GetProperty("id").GetString() == _sortedParcelId.ToString());
        var addedParcel = parcels.First(p => p.GetProperty("id").GetString() == _sortedParcelId.ToString());
        addedParcel.GetProperty("status").GetString().Should().Be("SORTED");

        // Verify in DB — status should remain Sorted
        using var scope = _fx.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var parcel = await db.Parcels.FindAsync(_sortedParcelId);
        parcel!.Status.Should().Be(ParcelStatus.Sorted);
        parcel.RouteStopId.Should().NotBeNull();
    }

    [Fact]
    public async Task AddParcelsToRoute_WithNonSortedParcel_ReturnsError()
    {
        var mutation = $@"
            mutation {{
                addParcelsToRoute(input: {{
                    routeId: ""{_routeId}"",
                    parcelIds: [""{_registeredParcelId}""]
                }}) {{
                    id
                }}
            }}";
        var response = await _fx.ExecuteGraphQLAsync(mutation);
        var json = await IntegrationFixture.ReadJsonAsync(response);

        json.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors[0].GetProperty("message").GetString().Should().Contain("Sorted");
    }

    [Fact]
    public async Task AddParcelsToRoute_WithStagedParcel_ReturnsError()
    {
        var mutation = $@"
            mutation {{
                addParcelsToRoute(input: {{
                    routeId: ""{_routeId}"",
                    parcelIds: [""{_stagedParcelId}""]
                }}) {{
                    id
                }}
            }}";
        var response = await _fx.ExecuteGraphQLAsync(mutation);
        var json = await IntegrationFixture.ReadJsonAsync(response);

        json.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors[0].GetProperty("message").GetString().Should().Contain("Sorted");
    }

    [Fact]
    public async Task AddParcelsToRoute_ParcelAlreadyOnAnotherRoute_ReturnsError()
    {
        // Create a second route
        var createMutation = $@"
            mutation {{
                createRoute(input: {{
                    name: ""Second Route"",
                    plannedStartTime: ""{DateTimeOffset.UtcNow.AddDays(8):O}"",
                    zoneId: ""{_zoneId}""
                }}) {{ id }}
            }}";
        var createResponse = await _fx.ExecuteGraphQLAsync(createMutation);
        var createJson = await IntegrationFixture.ReadJsonAsync(createResponse);
        var route2Id = createJson.RootElement.GetProperty("data").GetProperty("createRoute").GetProperty("id").GetString();

        // Create a Sorted parcel that is already assigned to route 2 via DB (simulates data anomaly)
        using (var scope = _fx.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var shipAddr = db.Parcels.First(p => p.Id == _sortedParcelId).ShipperAddressId;
            var recipAddr = db.Parcels.First(p => p.Id == _sortedParcelId).RecipientAddressId;

            var parcel = Parcel.Create("Already assigned", ServiceType.Standard);
            parcel.ShipperAddressId = shipAddr;
            parcel.RecipientAddressId = recipAddr;
            parcel.ZoneId = _zoneId;
            parcel.Status = ParcelStatus.Sorted;
            db.Parcels.Add(parcel);

            var stop = new RouteStop
            {
                SequenceNumber = 1,
                Street1 = "Test St",
                GeoLocation = new Point(-73.985, 40.748) { SRID = 4326 },
                RouteId = Guid.Parse(route2Id!),
                Parcels = [parcel]
            };
            parcel.RouteStopId = stop.Id;
            db.RouteStops.Add(stop);
            await db.SaveChangesAsync();
        }

        // Try to add same parcel to route 1
        // First get the parcel ID
        using (var scope = _fx.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var assignedParcel = db.Parcels.First(p => p.Status == ParcelStatus.Sorted && p.RouteStopId != null);

            var addMutation = $@"
                mutation {{
                    addParcelsToRoute(input: {{
                        routeId: ""{_routeId}"",
                        parcelIds: [""{assignedParcel.Id}""]
                    }}) {{
                        id
                    }}
                }}";
            var addResponse = await _fx.ExecuteGraphQLAsync(addMutation);
            var addJson = await IntegrationFixture.ReadJsonAsync(addResponse);

            addJson.RootElement.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors[0].GetProperty("message").GetString().Should().Contain("already assigned");
        }
    }

    [Fact]
    public async Task RemoveParcelsFromRoute_TransitionsBackToSorted()
    {
        // First add a sorted parcel to the route via DB (simulate what AddParcelsToRoute would do)
        using (var scope = _fx.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var shipAddr = db.Parcels.First(p => p.Id == _sortedParcelId).ShipperAddressId;
            var recipAddr = db.Parcels.First(p => p.Id == _sortedParcelId).RecipientAddressId;

            var parcel = Parcel.Create("To remove", ServiceType.Standard);
            parcel.ShipperAddressId = shipAddr;
            parcel.RecipientAddressId = recipAddr;
            parcel.ZoneId = _zoneId;
            parcel.Status = ParcelStatus.Staged;
            db.Parcels.Add(parcel);

            var stop = new RouteStop
            {
                SequenceNumber = 1,
                Street1 = "Remove Test St",
                GeoLocation = new Point(-73.985, 40.748) { SRID = 4326 },
                RouteId = _routeId,
                Parcels = [parcel]
            };
            parcel.RouteStopId = stop.Id;
            db.RouteStops.Add(stop);
            await db.SaveChangesAsync();

            // Now remove it
            var removeMutation = $@"
                mutation {{
                    removeParcelsFromRoute(input: {{
                        routeId: ""{_routeId}"",
                        parcelIds: [""{parcel.Id}""]
                    }}) {{
                        id
                        stops {{
                            parcels {{ id }}
                        }}
                    }}
                }}";
            var removeResponse = await _fx.ExecuteGraphQLAsync(removeMutation);
            var removeJson = await IntegrationFixture.ReadJsonAsync(removeResponse);

            removeJson.RootElement.TryGetProperty("errors", out _).Should().BeFalse();

            // Verify parcel is back to Sorted and unassigned
            await db.Entry(parcel).ReloadAsync();
            parcel.Status.Should().Be(ParcelStatus.Sorted);
            parcel.RouteStopId.Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteRoute_TransitionsParcelsBackToSorted()
    {
        // Create route with parcels via DB
        Guid testRouteId;
        Guid testParcelId;

        using (var scope = _fx.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var route = new Route
            {
                Name = "Delete Test Route",
                Status = RouteStatus.Draft,
                PlannedStartTime = DateTime.UtcNow.AddDays(7),
                ZoneId = _zoneId
            };
            db.Routes.Add(route);

            var shipAddr = db.Parcels.First(p => p.Id == _sortedParcelId).ShipperAddressId;
            var recipAddr = db.Parcels.First(p => p.Id == _sortedParcelId).RecipientAddressId;

            var parcel = Parcel.Create("Delete route parcel", ServiceType.Standard);
            parcel.ShipperAddressId = shipAddr;
            parcel.RecipientAddressId = recipAddr;
            parcel.ZoneId = _zoneId;
            parcel.Status = ParcelStatus.Staged;
            db.Parcels.Add(parcel);

            var stop = new RouteStop
            {
                SequenceNumber = 1,
                Street1 = "Delete Test St",
                GeoLocation = new Point(-73.985, 40.748) { SRID = 4326 },
                RouteId = route.Id,
                Parcels = [parcel]
            };
            parcel.RouteStopId = stop.Id;
            db.RouteStops.Add(stop);
            await db.SaveChangesAsync();

            testRouteId = route.Id;
            testParcelId = parcel.Id;
        }

        // Delete the route
        var deleteMutation = $@"
            mutation {{
                deleteRoute(id: ""{testRouteId}"")
            }}";
        var deleteResponse = await _fx.ExecuteGraphQLAsync(deleteMutation);
        var deleteJson = await IntegrationFixture.ReadJsonAsync(deleteResponse);
        deleteJson.RootElement.GetProperty("data").GetProperty("deleteRoute").GetBoolean().Should().BeTrue();

        // Verify parcel is back to Sorted
        using (var scope = _fx.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var parcel = await db.Parcels.FindAsync(testParcelId);
            parcel!.Status.Should().Be(ParcelStatus.Sorted);
            parcel.RouteStopId.Should().BeNull();
        }
    }
}
