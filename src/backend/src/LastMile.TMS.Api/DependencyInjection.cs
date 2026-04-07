using HotChocolate;
using HotChocolate.Types;
using LastMile.TMS.Api.GraphQL;
using LastMile.TMS.Api.GraphQL.Common;
using LastMile.TMS.Api.GraphQL.Extensions.Depot;
using LastMile.TMS.Api.GraphQL.Extensions.Driver;
using LastMile.TMS.Api.GraphQL.Extensions.Parcel;
using LastMile.TMS.Api.GraphQL.Extensions.Route;
using LastMile.TMS.Api.GraphQL.Extensions.UserManagement;
using LastMile.TMS.Api.GraphQL.Extensions.Vehicle;
using LastMile.TMS.Api.GraphQL.Extensions.Zone;
using LastMile.TMS.Api.GraphQL.Inputs;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddLastMileApi(this IServiceCollection services)
    {
        services.AddGraphQLServer()
            .ModifyCostOptions(o => o.MaxFieldCost = 25000)
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddAuthorization()
            .AddSpatialTypes()
            .AddQueryType<Query>(d => d.Name("Query").Field("sentinel").Type<StringType>().Resolve(_ => "sentinel"))
            .AddMutationType<Mutation>(d => d.Name("Mutation").Field("sentinel").Type<StringType>().Resolve(_ => "sentinel"))
            .AddType<DepotQuery>()
            .AddType<DepotMutation>()
            .AddType<ParcelQuery>()
            .AddType<ParcelMutation>()
            .AddType<ZoneQuery>()
            .AddType<ZoneMutation>()
            .AddType<VehicleQuery>()
            .AddType<VehicleMutation>()
            .AddType<RouteQuery>()
            .AddType<RouteMutation>()
            .AddType<DriverQuery>()
            .AddType<DriverMutation>()
            .AddType<UserManagementQuery>()
            .AddType<UserManagementMutation>()
            .AddType<CreateDriverInput>()
            .AddType<UpdateDriverInput>()
            .AddType<CreateDepotInput>()
            .AddType<CreateParcelInput>()
            .AddType<ParcelAddressInputType>()
            .AddType<AddressInputType>()
            .AddType<UpdateDepotInput>()
            .AddType<UpdateAddressInputType>()
            .AddType<DailyOperatingHoursInputType>()
            .AddType<CreateZoneInput>()
            .AddType<UpdateZoneInput>()
            .AddType<CreateRouteInput>()
            .AddType<UpdateRouteInput>()
            .AddErrorFilter<DomainExceptionErrorFilter>()
            .AddErrorFilter<ErrorFilter>()
            .AddType<ShiftScheduleInputType>()
            .AddType<DayOffInputType>();

        return services;
    }
}
