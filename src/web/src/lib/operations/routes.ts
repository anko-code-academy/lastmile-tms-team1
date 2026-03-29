export const GET_ROUTES = `
  query GetRoutes($where: RouteFilterInput) {
    routes(where: $where) {
      id
      name
      status
      plannedStartTime
      vehicleId
      vehicle {
        registrationPlate
      }
    }
  }
`;

export const GET_ROUTE = `
  query GetRoute($id: UUID!) {
    route(id: $id) {
      id
      name
      status
      plannedStartTime
      actualStartTime
      actualEndTime
      totalDistanceKm
      totalParcelCount
      vehicleId
      vehicle {
        registrationPlate
      }
      createdAt
    }
  }
`;

export const CREATE_ROUTE = `
  mutation CreateRoute(
    $name: String!
    $plannedStartTime: DateTime!
    $totalDistanceKm: Decimal!
    $totalParcelCount: Int!
    $vehicleId: UUID
  ) {
    createRoute(
      name: $name
      plannedStartTime: $plannedStartTime
      totalDistanceKm: $totalDistanceKm
      totalParcelCount: $totalParcelCount
      vehicleId: $vehicleId
    ) {
      id
      name
      status
      plannedStartTime
      totalDistanceKm
      totalParcelCount
      vehicleId
      vehiclePlate
      createdAt
    }
  }
`;

export const UPDATE_ROUTE = `
  mutation UpdateRoute(
    $id: UUID!
    $name: String!
    $plannedStartTime: DateTime!
    $totalDistanceKm: Decimal!
    $totalParcelCount: Int!
    $vehicleId: UUID
  ) {
    updateRoute(
      id: $id
      name: $name
      plannedStartTime: $plannedStartTime
      totalDistanceKm: $totalDistanceKm
      totalParcelCount: $totalParcelCount
      vehicleId: $vehicleId
    ) {
      id
      name
      status
      plannedStartTime
      totalDistanceKm
      totalParcelCount
      vehicleId
      vehiclePlate
      createdAt
    }
  }
`;

export const DELETE_ROUTE = `
  mutation DeleteRoute($id: UUID!) {
    deleteRoute(id: $id)
  }
`;

export const CHANGE_ROUTE_STATUS = `
  mutation ChangeRouteStatus($id: UUID!, $newStatus: RouteStatus!) {
    changeRouteStatus(id: $id, newStatus: $newStatus) {
      id
      name
      status
      plannedStartTime
      actualStartTime
      actualEndTime
      totalDistanceKm
      totalParcelCount
      vehicleId
      vehiclePlate
      createdAt
    }
  }
`;
