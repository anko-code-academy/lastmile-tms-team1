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
