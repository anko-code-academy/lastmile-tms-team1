export interface AddressDto {
  street1: string;
  street2?: string;
  city: string;
  state: string;
  postalCode: string;
  countryCode: string;
  isResidential: boolean;
  contactName?: string;
  companyName?: string;
  phone?: string;
  email?: string;
  geoLocation?: {
    coordinates?: [number, number];
  };
}

export interface DailyOperatingHoursDto {
  dayOfWeek: string;
  openTime: string;
  closeTime: string;
}

export enum DayOfWeek {
  Sunday = 0,
  Monday = 1,
  Tuesday = 2,
  Wednesday = 3,
  Thursday = 4,
  Friday = 5,
  Saturday = 6,
}

export interface DepotDto {
  id: string;
  name: string;
  address?: AddressDto;
  shiftSchedules: DailyOperatingHoursDto[];
  isActive: boolean;
  createdAt: string;
  lastModifiedAt?: string;
  zones: { id: string }[];
}

export interface DepotSummaryDto {
  id: string;
  name: string;
  address?: AddressDto;
  isActive: boolean;
  createdAt: string;
}

export interface DepotsResponse {
  depots: {
    nodes: DepotSummaryDto[];
  };
}

export interface DepotResult {
  id: string;
  name: string;
  isActive: boolean;
  createdAt: string;
}

export interface ZoneDto {
  id: string;
  name: string;
  boundaryGeometry: { type: string; coordinates: unknown; crs?: number } | string;
  depotId: string;
  depot?: { name: string };
  isActive: boolean;
  createdAt: string;
  lastModifiedAt?: string;
}

export interface ZoneSummaryDto {
  id: string;
  name: string;
  boundaryGeometry: { type: string; coordinates: unknown; crs?: number } | string;
  depotId: string;
  depot: { name: string };
  isActive: boolean;
  createdAt: string;
}

export interface ZonesResponse {
  zones: {
    nodes: ZoneSummaryDto[];
  };
}

export interface ZoneResult {
  id: string;
  name: string;
  depotId: string;
  isActive: boolean;
  createdAt: string;
}

export interface AddressInput {
  street1: string;
  street2?: string;
  city: string;
  state: string;
  postalCode: string;
  countryCode?: string;
  isResidential?: boolean;
  contactName?: string;
  companyName?: string;
  phone?: string;
  email?: string;
}

export interface DailyOperatingHoursInput {
  dayOfWeek: string;
  openTime: string | null;
  closeTime: string | null;
}

export interface CreateDepotInput {
  name: string;
  address: AddressInput;
  operatingHours?: DailyOperatingHoursInput[];
  isActive?: boolean;
}

export interface UpdateDepotInput {
  id: string;
  name: string;
  address: AddressInput;
  operatingHours?: DailyOperatingHoursInput[];
  isActive: boolean;
}

// ============================================
// Parcel
// ============================================

export interface ParcelAddressDto {
  street1: string;
  street2?: string;
  city: string;
  state: string;
  postalCode: string;
  countryCode: string;
  isResidential: boolean;
  contactName?: string;
  companyName?: string;
  phone?: string;
  email?: string;
  geoLocation?: {
    coordinates?: [number, number];
  };
}

export enum WeightUnit {
  Kg = "Kg",
  Lb = "Lb",
}

export enum DimensionUnit {
  Cm = "Cm",
  In = "In",
}

export enum ParcelStatus {
  REGISTERED = "REGISTERED",
  RECEIVED_AT_DEPOT = "RECEIVED_AT_DEPOT",
  SORTED = "SORTED",
  STAGED = "STAGED",
  LOADED = "LOADED",
  OUT_FOR_DELIVERY = "OUT_FOR_DELIVERY",
  DELIVERED = "DELIVERED",
  FAILED_ATTEMPT = "FAILED_ATTEMPT",
  RETURNED_TO_DEPOT = "RETURNED_TO_DEPOT",
  CANCELLED = "CANCELLED",
  EXCEPTION = "EXCEPTION",
}

export enum ServiceType {
  ECONOMY = "ECONOMY",
  STANDARD = "STANDARD",
  EXPRESS = "EXPRESS",
  OVERNIGHT = "OVERNIGHT",
}

export interface ParcelDto {
  id: string;
  trackingNumber: string;
  description?: string;
  serviceType: ServiceType;
  status: ParcelStatus;
  weight: number;
  weightUnit: WeightUnit;
  length: number;
  width: number;
  height: number;
  dimensionUnit: DimensionUnit;
  declaredValue: number;
  currency: string;
  estimatedDeliveryDate?: string;
  actualDeliveryDate?: string;
  deliveryAttempts: number;
  parcelType?: string;
  notes?: string;
  shipperAddress: ParcelAddressDto;
  recipientAddress: ParcelAddressDto;
  zone?: { id: string; name: string };
  createdAt: string;
  lastModifiedAt?: string;
}

export interface ParcelSummaryDto {
  id: string;
  trackingNumber: string;
  serviceType: ServiceType;
  status: ParcelStatus;
  createdAt: string;
}

export interface ParcelAddressInput {
  street1: string;
  street2?: string;
  city: string;
  state: string;
  postalCode: string;
  countryCode?: string;
  isResidential?: boolean;
  contactName?: string;
  companyName?: string;
  phone?: string;
  email?: string;
}

export interface CreateParcelInput {
  description?: string;
  serviceType: ServiceType;
  shipperAddress: ParcelAddressInput;
  recipientAddress: ParcelAddressInput;
  weight: number;
  weightUnit: WeightUnit;
  length: number;
  width: number;
  height: number;
  dimensionUnit: DimensionUnit;
  declaredValue: number;
  currency?: string;
  parcelType?: string;
  notes?: string;
  estimatedDeliveryDate?: string;
}

// ============================================
// Zone
// ============================================

export interface CreateZoneInput {
  name: string;
  geoJson: string;
  depotId: string;
  isActive?: boolean;
}

export interface UpdateZoneInput {
  id: string;
  name: string;
  geoJson?: string;
  depotId: string;
  isActive: boolean;
}
