export const GET_VEHICLES = `
  query GetVehicles($status: VehicleStatus) {
    vehicles(status: $status) {
      id
      registrationPlate
      type
      status
      depotId
    }
  }
`;

export const GET_VEHICLE = `
  query GetVehicle($id: UUID!) {
    vehicle(id: $id) {
      id
      registrationPlate
      type
      parcelCapacity
      weightCapacityKg
      status
      depotId
      createdAt
    }
  }
`;

export const GET_VEHICLE_HISTORY = `
  query GetVehicleHistory($id: UUID!) {
    vehicleHistory(id: $id) {
      id
      registrationPlate
      type
      totalMileageKm
      totalRoutesCompleted
      routes {
        routeId
        routeName
        completedAt
        distanceKm
        parcelCount
      }
    }
  }
`;

export const CREATE_VEHICLE = `
  mutation CreateVehicle(
    $registrationPlate: String!
    $type: VehicleType!
    $parcelCapacity: Int!
    $weightCapacityKg: Decimal!
    $depotId: UUID
  ) {
    createVehicle(
      registrationPlate: $registrationPlate
      type: $type
      parcelCapacity: $parcelCapacity
      weightCapacityKg: $weightCapacityKg
      depotId: $depotId
    ) {
      id
      registrationPlate
      type
      parcelCapacity
      weightCapacityKg
      status
      depotId
      createdAt
    }
  }
`;

export const UPDATE_VEHICLE = `
  mutation UpdateVehicle(
    $id: UUID!
    $registrationPlate: String!
    $type: VehicleType!
    $parcelCapacity: Int!
    $weightCapacityKg: Decimal!
    $depotId: UUID
  ) {
    updateVehicle(
      id: $id
      registrationPlate: $registrationPlate
      type: $type
      parcelCapacity: $parcelCapacity
      weightCapacityKg: $weightCapacityKg
      depotId: $depotId
    ) {
      id
      registrationPlate
      type
      parcelCapacity
      weightCapacityKg
      status
      depotId
      createdAt
    }
  }
`;

export const DELETE_VEHICLE = `
  mutation DeleteVehicle($id: UUID!) {
    deleteVehicle(id: $id)
  }
`;

export const CHANGE_VEHICLE_STATUS = `
  mutation ChangeVehicleStatus($id: UUID!, $newStatus: VehicleStatus!) {
    changeVehicleStatus(id: $id, newStatus: $newStatus) {
      id
      registrationPlate
      type
      parcelCapacity
      weightCapacityKg
      status
      depotId
      createdAt
    }
  }
`;
