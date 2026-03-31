import { apiFetch } from "@/lib/api";
import type { Vehicle, VehicleSummary, VehicleHistory, VehicleStatus } from "@/types/vehicle";

const GET_VEHICLES_QUERY = `
  query GetVehicles($where: VehicleFilterInput) {
    vehicles(where: $where) {
      id
      registrationPlate
      type
      status
      depotId
    }
  }
`;

const GET_VEHICLE_QUERY = `
  query GetVehicle($id: UUID!) {
    vehicle(id: $id) {
      id
      registrationPlate
      type
      parcelCapacity
      weightCapacityKg
      status
      depotId
      depot {
        name
      }
      createdAt
    }
  }
`;

const GET_VEHICLE_HISTORY_QUERY = `
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

const CREATE_VEHICLE_MUTATION = `
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

const UPDATE_VEHICLE_MUTATION = `
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

const DELETE_VEHICLE_MUTATION = `
  mutation DeleteVehicle($id: UUID!) {
    deleteVehicle(id: $id)
  }
`;

const CHANGE_VEHICLE_STATUS_MUTATION = `
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

interface VehiclesResponse {
  vehicles: VehicleSummary[];
}

interface VehicleResponse {
  vehicle: Vehicle | null;
}

interface VehicleHistoryResponse {
  vehicleHistory: VehicleHistory | null;
}

export async function fetchVehicles(
  token: string,
  status?: VehicleStatus
): Promise<VehicleSummary[]> {
  const response = await apiFetch<{ data: VehiclesResponse }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: GET_VEHICLES_QUERY,
      variables: status ? { where: { status } } : {},
    }),
  });
  return response.data.vehicles;
}

export async function fetchVehicle(
  token: string,
  id: string
): Promise<Vehicle | null> {
  const response = await apiFetch<{ data: VehicleResponse }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: GET_VEHICLE_QUERY,
      variables: { id },
    }),
  });
  return response.data.vehicle;
}

export async function fetchVehicleHistory(
  token: string,
  id: string
): Promise<VehicleHistory | null> {
  const response = await apiFetch<{
    data: VehicleHistoryResponse;
  }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: GET_VEHICLE_HISTORY_QUERY,
      variables: { id },
    }),
  });
  return response.data.vehicleHistory;
}

export async function createVehicle(
  token: string,
  input: {
    registrationPlate: string;
    type: string;
    parcelCapacity: number;
    weightCapacityKg: number;
    depotId?: string | null;
  }
): Promise<Vehicle> {
  const response = await apiFetch<{ data: { createVehicle: Vehicle } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: CREATE_VEHICLE_MUTATION,
        variables: {
          registrationPlate: input.registrationPlate,
          type: input.type,
          parcelCapacity: input.parcelCapacity,
          weightCapacityKg: input.weightCapacityKg,
          depotId: input.depotId || null,
        },
      }),
    }
  );
  return response.data.createVehicle;
}

export async function updateVehicle(
  token: string,
  input: {
    id: string;
    registrationPlate: string;
    type: string;
    parcelCapacity: number;
    weightCapacityKg: number;
    depotId?: string | null;
  }
): Promise<Vehicle> {
  const response = await apiFetch<{ data: { updateVehicle: Vehicle } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: UPDATE_VEHICLE_MUTATION,
        variables: {
          id: input.id,
          registrationPlate: input.registrationPlate,
          type: input.type,
          parcelCapacity: input.parcelCapacity,
          weightCapacityKg: input.weightCapacityKg,
          depotId: input.depotId || null,
        },
      }),
    }
  );
  return response.data.updateVehicle;
}

export async function deleteVehicle(
  token: string,
  id: string
): Promise<boolean> {
  const response = await apiFetch<{ data: { deleteVehicle: boolean } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: DELETE_VEHICLE_MUTATION,
        variables: { id },
      }),
    }
  );
  return response.data.deleteVehicle;
}

export async function changeVehicleStatus(
  token: string,
  id: string,
  newStatus: VehicleStatus
): Promise<Vehicle> {
  const response = await apiFetch<{
    data: { changeVehicleStatus: Vehicle };
  }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: CHANGE_VEHICLE_STATUS_MUTATION,
      variables: { id, newStatus },
    }),
  });
  return response.data.changeVehicleStatus;
}
