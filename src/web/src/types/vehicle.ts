export enum VehicleType {
  VAN = "VAN",
  CAR = "CAR",
  BIKE = "BIKE",
}

export enum VehicleStatus {
  AVAILABLE = "AVAILABLE",
  IN_USE = "IN_USE",
  MAINTENANCE = "MAINTENANCE",
  RETIRED = "RETIRED",
}

export interface VehicleSummary {
  id: string;
  registrationPlate: string;
  type: VehicleType;
  status: VehicleStatus;
  depotId: string | null;
}

export interface Vehicle {
  id: string;
  registrationPlate: string;
  type: VehicleType;
  parcelCapacity: number;
  weightCapacityKg: number;
  status: VehicleStatus;
  depotId: string | null;
  depot: { name: string } | null;
  createdAt: string;
}

export interface RouteHistory {
  routeId: string;
  routeName: string;
  completedAt: string;
  distanceKm: number;
  parcelCount: number;
}

export interface VehicleHistory {
  id: string;
  registrationPlate: string;
  type: VehicleType;
  totalMileageKm: number;
  totalRoutesCompleted: number;
  routes: RouteHistory[];
}

export interface CreateVehicleInput {
  registrationPlate: string;
  type: VehicleType;
  parcelCapacity: number;
  weightCapacityKg: number;
  depotId?: string | null;
}

export interface UpdateVehicleInput extends CreateVehicleInput {
  id: string;
}
