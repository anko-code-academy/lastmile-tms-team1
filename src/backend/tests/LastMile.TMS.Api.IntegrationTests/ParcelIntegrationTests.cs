using System.Text;
using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Api.IntegrationTests;

[Collection("Integration")]
public class ParcelIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationFixture _fx;
    private string _searchTrackingNumber = null!;
    private string _otherTrackingNumber = null!;

    public ParcelIntegrationTests(IntegrationFixture fx)
    {
        _fx = fx;
    }

    public async Task InitializeAsync()
    {
        var run = Guid.NewGuid().ToString("N")[..4];
        _searchTrackingNumber = $"S-{run}";
        _otherTrackingNumber = $"O-{run}";

        using var scope = _fx.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedTestParcelsAsync(dbContext);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedTestParcelsAsync(AppDbContext dbContext)
    {
        var shipperAddress = new Address
        {
            Street1 = "100 Shipper St",
            City = "ShipperCity",
            State = "CA",
            PostalCode = "90210",
            CountryCode = "US"
        };

        var recipientAddress1 = new Address
        {
            Street1 = "200 Recipient Ave",
            City = "RecipientCity",
            State = "NY",
            PostalCode = "10001",
            CountryCode = "US",
            ContactName = "John Doe"
        };

        var recipientAddress2 = new Address
        {
            Street1 = "300 Other St",
            City = "OtherCity",
            State = "TX",
            PostalCode = "75001",
            CountryCode = "US",
            ContactName = "Jane Smith"
        };

        dbContext.Addresses.AddRange(shipperAddress, recipientAddress1, recipientAddress2);
        await dbContext.SaveChangesAsync();

        var parcel1 = Parcel.Create("Electronics package", ServiceType.Express);
        SetTrackingNumber(parcel1, _searchTrackingNumber);
        parcel1.ShipperAddressId = shipperAddress.Id;
        parcel1.RecipientAddressId = recipientAddress1.Id;
        parcel1.Status = ParcelStatus.Registered;

        var parcel2 = Parcel.Create("Fragile items", ServiceType.Standard);
        SetTrackingNumber(parcel2, _otherTrackingNumber);
        parcel2.ShipperAddressId = shipperAddress.Id;
        parcel2.RecipientAddressId = recipientAddress2.Id;
        parcel2.Status = ParcelStatus.Registered;

        dbContext.Parcels.AddRange(parcel1, parcel2);
        await dbContext.SaveChangesAsync();
    }

    private static void SetTrackingNumber(Parcel parcel, string trackingNumber)
    {
        var prop = typeof(Parcel).GetProperty("TrackingNumber");
        prop!.SetValue(parcel, trackingNumber);
    }

    private Task<JsonElement> GqlAsync(string query, object? variables = null) =>
        _fx.GraphQLRequestAsync(query, variables);

    [Fact]
    public async Task GetParcels_ReturnsPaginatedResults()
    {
        var query = @"query {
            parcels {
                nodes {
                    id
                    trackingNumber
                    status
                }
                pageInfo {
                    hasNextPage
                    hasPreviousPage
                }
                totalCount
            }
        }";

        var json = await GqlAsync(query);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcels = json.GetProperty("data").GetProperty("parcels");
        parcels.GetProperty("nodes").GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
        parcels.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(2);
        parcels.GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetParcels_FilterByTrackingNumber_ReturnsMatchingParcel()
    {
        var query = @$"query {{
            parcels(where: {{ trackingNumber: {{ contains: ""{_searchTrackingNumber}"" }} }}) {{
                nodes {{
                    trackingNumber
                }}
                totalCount
            }}
        }}";

        var json = await GqlAsync(query);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcels = json.GetProperty("data").GetProperty("parcels");
        parcels.GetProperty("nodes").GetArrayLength().Should().Be(1);
        parcels.GetProperty("nodes")[0].GetProperty("trackingNumber").GetString()
            .Should().Be(_searchTrackingNumber);
    }

    [Fact]
    public async Task GetParcels_RecipientSearch_ReturnsMatchingParcel()
    {
        var query = @"query {
            parcels(recipientSearch: ""John"") {
                nodes {
                    trackingNumber
                }
            }
        }";

        var json = await GqlAsync(query);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcels = json.GetProperty("data").GetProperty("parcels");
        parcels.GetProperty("nodes").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetParcels_AddressSearch_ReturnsMatchingParcel()
    {
        var query = @"query {
            parcels(addressSearch: ""RecipientCity"") {
                nodes {
                    trackingNumber
                }
            }
        }";

        var json = await GqlAsync(query);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcels = json.GetProperty("data").GetProperty("parcels");
        parcels.GetProperty("nodes").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetParcels_RecipientAndAddressSearch_ReturnsCombinedResults()
    {
        var query = @"query {
            parcels(recipientSearch: ""John"", addressSearch: ""RecipientCity"") {
                nodes {
                    trackingNumber
                }
            }
        }";

        var json = await GqlAsync(query);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcels = json.GetProperty("data").GetProperty("parcels");
        parcels.GetProperty("nodes").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetParcels_NoSearchParams_ReturnsAllResults()
    {
        var query = @"query {
            parcels {
                nodes {
                    trackingNumber
                }
                totalCount
            }
        }";

        var json = await GqlAsync(query);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcels = json.GetProperty("data").GetProperty("parcels");
        parcels.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetParcels_FilterByStatus_ReturnsMatchingParcels()
    {
        var query = @"query {
            parcels(where: { status: { eq: REGISTERED } }) {
                nodes {
                    status
                }
            }
        }";

        var json = await GqlAsync(query);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcels = json.GetProperty("data").GetProperty("parcels");
        var nodes = parcels.GetProperty("nodes");
        nodes.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
        foreach (var node in nodes.EnumerateArray())
        {
            node.GetProperty("status").GetString().Should().Be("REGISTERED");
        }
    }

    [Fact]
    public async Task GetParcels_SortByTrackingNumber_ReturnsOrderedResults()
    {
        var query = @"query {
            parcels(order: { trackingNumber: ASC }) {
                nodes {
                    trackingNumber
                }
            }
        }";

        var json = await GqlAsync(query);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcels = json.GetProperty("data").GetProperty("parcels");
        var nodes = parcels.GetProperty("nodes").EnumerateArray().ToList();
        var trackingNumbers = nodes.Select(n => n.GetProperty("trackingNumber").GetString()).ToList();
        trackingNumbers.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetParcels_WithFirst_ReturnsLimitedResults()
    {
        var query = @"query {
            parcels(first: 1) {
                nodes {
                    trackingNumber
                }
                pageInfo {
                    hasNextPage
                }
                totalCount
            }
        }";

        var json = await GqlAsync(query);

        if (json.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcels = json.GetProperty("data").GetProperty("parcels");
        parcels.GetProperty("nodes").GetArrayLength().Should().Be(1);
        parcels.GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean().Should().BeTrue();
        parcels.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task CreateParcel_WithValidInput_ReturnsParcelWithTrackingNumber()
    {
        var mutation = @"mutation CreateParcel($input: CreateParcelCommandInput!) {
            createParcel(input: $input) {
                id
                trackingNumber
                status
                serviceType
                createdAt
            }
        }";

        var variables = new
        {
            input = new
            {
                description = "Test parcel",
                serviceType = "Standard",
                shipperAddress = new
                {
                    street1 = "123 Sender St",
                    city = "New York",
                    state = "NY",
                    postalCode = "10001",
                    countryCode = "US",
                    contactName = "John Sender",
                    phone = "+1-555-0100",
                    email = "sender@example.com"
                },
                recipientAddress = new
                {
                    street1 = "456 Recipient Ave",
                    city = "Los Angeles",
                    state = "CA",
                    postalCode = "90001",
                    countryCode = "US",
                    contactName = "Jane Recipient",
                    phone = "+1-555-0200",
                    email = "recipient@example.com"
                },
                weight = 2.5,
                weightUnit = "Lb",
                length = 10.0,
                width = 8.0,
                height = 5.0,
                dimensionUnit = "In",
                declaredValue = 100.00,
                currency = "USD",
                parcelType = "PACKAGE",
                notes = "Handle with care"
            }
        };

        var jsonResponse = await GqlAsync(mutation, variables);

        if (jsonResponse.TryGetProperty("errors", out var errors))
        {
            throw new Exception($"GraphQL errors: {errors.GetRawText()}");
        }

        var parcel = jsonResponse.GetProperty("data").GetProperty("createParcel");
        parcel.TryGetProperty("id", out _).Should().BeTrue();
        parcel.GetProperty("trackingNumber").GetString().Should().NotBeNullOrEmpty();
        parcel.GetProperty("trackingNumber").GetString().Should().StartWith("LM-");
        parcel.GetProperty("status").GetString().Should().Be("REGISTERED");
        parcel.GetProperty("serviceType").GetString().Should().Be("STANDARD");
    }

    [Fact]
    public async Task CreateParcel_WithMissingRequiredFields_ReturnsValidationError()
    {
        var mutation = @"mutation CreateParcel($input: CreateParcelCommandInput!) {
            createParcel(input: $input) {
                id
                trackingNumber
            }
        }";

        var variables = new
        {
            input = new
            {
                description = "Invalid parcel",
                serviceType = "Standard",
                shipperAddress = new
                {
                    street1 = "",
                    city = "",
                    state = "",
                    postalCode = "",
                    countryCode = "US"
                },
                recipientAddress = new
                {
                    street1 = "",
                    city = "",
                    state = "",
                    postalCode = "",
                    countryCode = "US"
                },
                weight = 0,
                weightUnit = "Lb",
                length = 0,
                width = 0,
                height = 0,
                dimensionUnit = "In"
            }
        };

        var jsonResponse = await GqlAsync(mutation, variables);

        jsonResponse.TryGetProperty("errors", out var valErrors).Should().BeTrue();
        valErrors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateParcel_WithInvalidEmail_ReturnsValidationError()
    {
        var mutation = @"mutation CreateParcel($input: CreateParcelCommandInput!) {
            createParcel(input: $input) {
                id
            }
        }";

        var variables = new
        {
            input = new
            {
                serviceType = "Standard",
                shipperAddress = new
                {
                    street1 = "123 Sender St",
                    city = "New York",
                    state = "NY",
                    postalCode = "10001",
                    countryCode = "US",
                    email = "not-an-email"
                },
                recipientAddress = new
                {
                    street1 = "456 Recipient Ave",
                    city = "Los Angeles",
                    state = "CA",
                    postalCode = "90001",
                    countryCode = "US"
                },
                weight = 2.5,
                weightUnit = "Lb",
                length = 10.0,
                width = 8.0,
                height = 5.0,
                dimensionUnit = "In"
            }
        };

        var jsonResponse = await GqlAsync(mutation, variables);

        jsonResponse.TryGetProperty("errors", out var valErrors).Should().BeTrue();
        valErrors.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateParcel_GeneratesUniqueTrackingNumbers()
    {
        var mutation = @"mutation CreateParcel($input: CreateParcelCommandInput!) {
            createParcel(input: $input) {
                trackingNumber
            }
        }";

        var baseInput = new
        {
            serviceType = "Standard",
            shipperAddress = new
            {
                street1 = "123 Sender St",
                city = "New York",
                state = "NY",
                postalCode = "10001",
                countryCode = "US"
            },
            recipientAddress = new
            {
                street1 = "456 Recipient Ave",
                city = "Los Angeles",
                state = "CA",
                postalCode = "90001",
                countryCode = "US"
            },
            weight = 1.0,
            weightUnit = "Lb",
            length = 5.0,
            width = 5.0,
            height = 5.0,
            dimensionUnit = "In"
        };

        var response1 = await GqlAsync(mutation, new { input = baseInput });
        var response2 = await GqlAsync(mutation, new { input = baseInput });

        var tracking1 = response1.GetProperty("data").GetProperty("createParcel").GetProperty("trackingNumber").GetString();
        var tracking2 = response2.GetProperty("data").GetProperty("createParcel").GetProperty("trackingNumber").GetString();

        tracking1.Should().NotBe(tracking2, "each parcel should get a unique tracking number");
    }
}
