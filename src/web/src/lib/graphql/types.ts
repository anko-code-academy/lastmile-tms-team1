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
  dayOfWeek: DayOfWeek;
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
  operatingHours: DailyOperatingHoursDto[];
  isActive: boolean;
  createdAt: string;
  lastModifiedAt?: string;
  zoneIds: string[];
}

export interface DepotSummaryDto {
  id: string;
  name: string;
  addressSummary?: string;
  isActive: boolean;
  createdAt: string;
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
  geoJson: string;
  depotId: string;
  depotName?: string;
  isActive: boolean;
  createdAt: string;
  lastModifiedAt?: string;
}

export interface ZoneSummaryDto {
  id: string;
  name: string;
  depotId: string;
  depotName?: string;
  isActive: boolean;
  createdAt: string;
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
  dayOfWeek: DayOfWeek;
  openTime: string;
  closeTime: string;
}

export interface CreateDepotInput {
  name: string;
  address?: AddressInput;
  operatingHours?: DailyOperatingHoursInput[];
  isActive?: boolean;
}

export interface UpdateDepotInput {
  id: string;
  name: string;
  address?: AddressInput;
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
