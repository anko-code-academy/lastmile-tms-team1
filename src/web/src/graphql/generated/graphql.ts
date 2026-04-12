/* eslint-disable */
import { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';
export type Maybe<T> = T | null;
export type InputMaybe<T> = T | null | undefined;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
export type MakeEmpty<T extends { [key: string]: unknown }, K extends keyof T> = { [_ in K]?: never };
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
  /** A coordinate is an array of positions. */
  Coordinates: { input: any; output: any; }
  /** The `DateTime` scalar type represents a date and time with time zone offset information. */
  DateTime: { input: any; output: any; }
  /** The `Decimal` scalar type represents a decimal floating-point number with high precision. */
  Decimal: { input: any; output: any; }
  Geometry: { input: any; output: any; }
  /** The `LocalTime` scalar type represents a time of day without date or time zone information. */
  LocalTime: { input: any; output: any; }
  /** A position is an array of numbers. There MUST be two or more elements. The first two elements are longitude and latitude, or easting and northing, precisely in that order and using decimal numbers. Altitude or elevation MAY be included as an optional third element. */
  Position: { input: any; output: any; }
  /** The `UUID` scalar type represents a Universally Unique Identifier (UUID) as defined by RFC 9562. */
  UUID: { input: any; output: any; }
};

export type AddParcelsToRouteCommandInput = {
  parcelIds: Array<Scalars['UUID']['input']>;
  routeId: Scalars['UUID']['input'];
};

export type Address = {
  __typename?: 'Address';
  city: Scalars['String']['output'];
  companyName?: Maybe<Scalars['String']['output']>;
  contactName?: Maybe<Scalars['String']['output']>;
  countryCode: Scalars['String']['output'];
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  email?: Maybe<Scalars['String']['output']>;
  geoLocation?: Maybe<GeoJsonPointType>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  isResidential: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  phone?: Maybe<Scalars['String']['output']>;
  postalCode: Scalars['String']['output'];
  recipientParcels: Array<Parcel>;
  shipperParcels: Array<Parcel>;
  state: Scalars['String']['output'];
  street1: Scalars['String']['output'];
  street2?: Maybe<Scalars['String']['output']>;
};

export type AddressFilterInput = {
  and?: InputMaybe<Array<AddressFilterInput>>;
  city?: InputMaybe<StringOperationFilterInput>;
  companyName?: InputMaybe<StringOperationFilterInput>;
  contactName?: InputMaybe<StringOperationFilterInput>;
  countryCode?: InputMaybe<StringOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  email?: InputMaybe<StringOperationFilterInput>;
  geoLocation?: InputMaybe<PointFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  isResidential?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<AddressFilterInput>>;
  phone?: InputMaybe<StringOperationFilterInput>;
  postalCode?: InputMaybe<StringOperationFilterInput>;
  recipientParcels?: InputMaybe<ListFilterInputTypeOfParcelFilterInput>;
  shipperParcels?: InputMaybe<ListFilterInputTypeOfParcelFilterInput>;
  state?: InputMaybe<StringOperationFilterInput>;
  street1?: InputMaybe<StringOperationFilterInput>;
  street2?: InputMaybe<StringOperationFilterInput>;
};

export type AddressInput = {
  city: Scalars['String']['input'];
  companyName?: InputMaybe<Scalars['String']['input']>;
  contactName?: InputMaybe<Scalars['String']['input']>;
  countryCode?: Scalars['String']['input'];
  email?: InputMaybe<Scalars['String']['input']>;
  isResidential?: Scalars['Boolean']['input'];
  phone?: InputMaybe<Scalars['String']['input']>;
  postalCode: Scalars['String']['input'];
  state: Scalars['String']['input'];
  street1: Scalars['String']['input'];
  street2?: InputMaybe<Scalars['String']['input']>;
};

export type AddressResult = {
  __typename?: 'AddressResult';
  city: Scalars['String']['output'];
  companyName?: Maybe<Scalars['String']['output']>;
  contactName?: Maybe<Scalars['String']['output']>;
  countryCode: Scalars['String']['output'];
  email?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  phone?: Maybe<Scalars['String']['output']>;
  postalCode: Scalars['String']['output'];
  state: Scalars['String']['output'];
  street1: Scalars['String']['output'];
  street2?: Maybe<Scalars['String']['output']>;
};

export type AddressSortInput = {
  city?: InputMaybe<SortEnumType>;
  companyName?: InputMaybe<SortEnumType>;
  contactName?: InputMaybe<SortEnumType>;
  countryCode?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  email?: InputMaybe<SortEnumType>;
  geoLocation?: InputMaybe<PointSortInput>;
  id?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  isResidential?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  phone?: InputMaybe<SortEnumType>;
  postalCode?: InputMaybe<SortEnumType>;
  state?: InputMaybe<SortEnumType>;
  street1?: InputMaybe<SortEnumType>;
  street2?: InputMaybe<SortEnumType>;
};

export type Aisle = {
  __typename?: 'Aisle';
  bins: Array<Bin>;
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  isActive: Scalars['Boolean']['output'];
  isDeleted: Scalars['Boolean']['output'];
  label: Scalars['String']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  name: Scalars['String']['output'];
  order: Scalars['Int']['output'];
  zone: Zone;
  zoneId: Scalars['UUID']['output'];
};

export type AisleFilterInput = {
  and?: InputMaybe<Array<AisleFilterInput>>;
  bins?: InputMaybe<ListFilterInputTypeOfBinFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isActive?: InputMaybe<BooleanOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  label?: InputMaybe<StringOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<AisleFilterInput>>;
  order?: InputMaybe<IntOperationFilterInput>;
  zone?: InputMaybe<ZoneFilterInput>;
  zoneId?: InputMaybe<UuidOperationFilterInput>;
};

export type AisleResult = {
  __typename?: 'AisleResult';
  createdAt: Scalars['DateTime']['output'];
  id: Scalars['UUID']['output'];
  isActive: Scalars['Boolean']['output'];
  label: Scalars['String']['output'];
  name: Scalars['String']['output'];
  order: Scalars['Int']['output'];
  zoneId: Scalars['UUID']['output'];
  zoneName: Scalars['String']['output'];
};

export type AisleSortInput = {
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isActive?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  label?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
  order?: InputMaybe<SortEnumType>;
  zone?: InputMaybe<ZoneSortInput>;
  zoneId?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type AislesConnection = {
  __typename?: 'AislesConnection';
  /** A list of edges. */
  edges?: Maybe<Array<AislesEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Aisle>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type AislesEdge = {
  __typename?: 'AislesEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Aisle;
};

/** Defines when a policy shall be executed. */
export enum ApplyPolicy {
  /** After the resolver was executed. */
  AfterResolver = 'AFTER_RESOLVER',
  /** Before the resolver was executed. */
  BeforeResolver = 'BEFORE_RESOLVER',
  /** The policy is applied in the validation step before the execution. */
  Validation = 'VALIDATION'
}

export type AutoAssignParcelsByZoneCommandInput = {
  routeId: Scalars['UUID']['input'];
};

export type AvailableDriverDto = {
  __typename?: 'AvailableDriverDto';
  assignedRoutes: Array<DriverRouteSummaryDto>;
  id: Scalars['UUID']['output'];
  name: Scalars['String']['output'];
  shift?: Maybe<ShiftInfoDto>;
};

export type Bin = {
  __typename?: 'Bin';
  aisle: Aisle;
  aisleId: Scalars['UUID']['output'];
  capacity: Scalars['Int']['output'];
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  description?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  isActive: Scalars['Boolean']['output'];
  isDeleted: Scalars['Boolean']['output'];
  label: Scalars['String']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  slot: Scalars['Int']['output'];
  zone: Zone;
  zoneId: Scalars['UUID']['output'];
};

export type BinDto = {
  __typename?: 'BinDto';
  aisleLabel: Scalars['String']['output'];
  capacity: Scalars['Int']['output'];
  createdAt: Scalars['DateTime']['output'];
  currentParcelCount: Scalars['Int']['output'];
  depotName: Scalars['String']['output'];
  description?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  isActive: Scalars['Boolean']['output'];
  label: Scalars['String']['output'];
  slot: Scalars['Int']['output'];
  utilizationPercent: Scalars['Float']['output'];
  zoneId: Scalars['UUID']['output'];
  zoneName: Scalars['String']['output'];
};

export type BinFilterInput = {
  aisle?: InputMaybe<AisleFilterInput>;
  aisleId?: InputMaybe<UuidOperationFilterInput>;
  and?: InputMaybe<Array<BinFilterInput>>;
  capacity?: InputMaybe<IntOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  description?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isActive?: InputMaybe<BooleanOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  label?: InputMaybe<StringOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<BinFilterInput>>;
  slot?: InputMaybe<IntOperationFilterInput>;
  zone?: InputMaybe<ZoneFilterInput>;
  zoneId?: InputMaybe<UuidOperationFilterInput>;
};

export type BinResult = {
  __typename?: 'BinResult';
  aisleLabel: Scalars['String']['output'];
  capacity: Scalars['Int']['output'];
  createdAt: Scalars['DateTime']['output'];
  description?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  isActive: Scalars['Boolean']['output'];
  label: Scalars['String']['output'];
  slot: Scalars['Int']['output'];
  zoneId: Scalars['UUID']['output'];
  zoneName: Scalars['String']['output'];
};

export type BinSortInput = {
  aisle?: InputMaybe<AisleSortInput>;
  aisleId?: InputMaybe<SortEnumType>;
  capacity?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  description?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isActive?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  label?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  slot?: InputMaybe<SortEnumType>;
  zone?: InputMaybe<ZoneSortInput>;
  zoneId?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type BinsConnection = {
  __typename?: 'BinsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<BinsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Bin>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type BinsEdge = {
  __typename?: 'BinsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Bin;
};

export type BooleanOperationFilterInput = {
  eq?: InputMaybe<Scalars['Boolean']['input']>;
  neq?: InputMaybe<Scalars['Boolean']['input']>;
};

export type CancelParcelCommandInput = {
  id: Scalars['UUID']['input'];
  reason: Scalars['String']['input'];
};

export type CancelParcelResult = {
  __typename?: 'CancelParcelResult';
  id: Scalars['UUID']['output'];
  status: ParcelStatus;
  trackingNumber: Scalars['String']['output'];
};

export type ChangeParcelStatusCommandInput = {
  description?: InputMaybe<Scalars['String']['input']>;
  exceptionReason?: InputMaybe<ExceptionReason>;
  id: Scalars['UUID']['input'];
  locationCity?: InputMaybe<Scalars['String']['input']>;
  locationCountry?: InputMaybe<Scalars['String']['input']>;
  locationState?: InputMaybe<Scalars['String']['input']>;
  newStatus: ParcelStatus;
};

export type ChangeParcelStatusResult = {
  __typename?: 'ChangeParcelStatusResult';
  deliveryAttempts: Scalars['Int']['output'];
  id: Scalars['UUID']['output'];
  status: ParcelStatus;
  trackingNumber: Scalars['String']['output'];
};

export type CoordinateEqualityComparerFilterInput = {
  and?: InputMaybe<Array<CoordinateEqualityComparerFilterInput>>;
  or?: InputMaybe<Array<CoordinateEqualityComparerFilterInput>>;
};

export type CoordinateFilterInput = {
  and?: InputMaybe<Array<CoordinateFilterInput>>;
  coordinateValue?: InputMaybe<CoordinateFilterInput>;
  isValid?: InputMaybe<BooleanOperationFilterInput>;
  m?: InputMaybe<FloatOperationFilterInput>;
  or?: InputMaybe<Array<CoordinateFilterInput>>;
  x?: InputMaybe<FloatOperationFilterInput>;
  y?: InputMaybe<FloatOperationFilterInput>;
  z?: InputMaybe<FloatOperationFilterInput>;
};

export type CoordinateSequenceFactoryFilterInput = {
  and?: InputMaybe<Array<CoordinateSequenceFactoryFilterInput>>;
  or?: InputMaybe<Array<CoordinateSequenceFactoryFilterInput>>;
  ordinates?: InputMaybe<OrdinatesOperationFilterInput>;
};

export type CoordinateSequenceFactorySortInput = {
  ordinates?: InputMaybe<SortEnumType>;
};

export type CoordinateSequenceFilterInput = {
  and?: InputMaybe<Array<CoordinateSequenceFilterInput>>;
  count?: InputMaybe<IntOperationFilterInput>;
  dimension?: InputMaybe<IntOperationFilterInput>;
  first?: InputMaybe<CoordinateFilterInput>;
  hasM?: InputMaybe<BooleanOperationFilterInput>;
  hasZ?: InputMaybe<BooleanOperationFilterInput>;
  last?: InputMaybe<CoordinateFilterInput>;
  mOrdinateIndex?: InputMaybe<IntOperationFilterInput>;
  measures?: InputMaybe<IntOperationFilterInput>;
  or?: InputMaybe<Array<CoordinateSequenceFilterInput>>;
  ordinates?: InputMaybe<OrdinatesOperationFilterInput>;
  spatial?: InputMaybe<IntOperationFilterInput>;
  zOrdinateIndex?: InputMaybe<IntOperationFilterInput>;
};

export type CoordinateSequenceSortInput = {
  count?: InputMaybe<SortEnumType>;
  dimension?: InputMaybe<SortEnumType>;
  first?: InputMaybe<CoordinateSortInput>;
  hasM?: InputMaybe<SortEnumType>;
  hasZ?: InputMaybe<SortEnumType>;
  last?: InputMaybe<CoordinateSortInput>;
  mOrdinateIndex?: InputMaybe<SortEnumType>;
  measures?: InputMaybe<SortEnumType>;
  ordinates?: InputMaybe<SortEnumType>;
  spatial?: InputMaybe<SortEnumType>;
  zOrdinateIndex?: InputMaybe<SortEnumType>;
};

export type CoordinateSortInput = {
  coordinateValue?: InputMaybe<CoordinateSortInput>;
  isValid?: InputMaybe<SortEnumType>;
  m?: InputMaybe<SortEnumType>;
  x?: InputMaybe<SortEnumType>;
  y?: InputMaybe<SortEnumType>;
  z?: InputMaybe<SortEnumType>;
};

export type CreateAisleCommandInput = {
  isActive?: Scalars['Boolean']['input'];
  name: Scalars['String']['input'];
  order?: InputMaybe<Scalars['Int']['input']>;
  zoneId: Scalars['UUID']['input'];
};

export type CreateBinCommandInput = {
  aisleId: Scalars['UUID']['input'];
  capacity: Scalars['Int']['input'];
  description?: InputMaybe<Scalars['String']['input']>;
  isActive?: Scalars['Boolean']['input'];
  slot: Scalars['Int']['input'];
  zoneId: Scalars['UUID']['input'];
};

export type CreateDepotCommandInput = {
  address: AddressInput;
  isActive?: Scalars['Boolean']['input'];
  name: Scalars['String']['input'];
  operatingHours?: InputMaybe<Array<InputMaybe<DailyOperatingHoursInput>>>;
};

export type CreateDriverCommandInput = {
  daysOff?: InputMaybe<Array<InputMaybe<DayOffInput>>>;
  email: Scalars['String']['input'];
  licenseExpiryDate: Scalars['DateTime']['input'];
  licenseNumber: Scalars['String']['input'];
  photo?: InputMaybe<Scalars['String']['input']>;
  shiftSchedules?: InputMaybe<Array<InputMaybe<ShiftScheduleInput>>>;
};

export type CreateParcelCommandInput = {
  currency?: InputMaybe<Scalars['String']['input']>;
  declaredValue?: InputMaybe<Scalars['Decimal']['input']>;
  description?: InputMaybe<Scalars['String']['input']>;
  dimensionUnit: Scalars['String']['input'];
  height: Scalars['Decimal']['input'];
  length: Scalars['Decimal']['input'];
  notes?: InputMaybe<Scalars['String']['input']>;
  parcelType?: InputMaybe<ParcelType>;
  recipientAddress: ParcelAddressInput;
  serviceType: Scalars['String']['input'];
  shipperAddress: ParcelAddressInput;
  weight: Scalars['Decimal']['input'];
  weightUnit: Scalars['String']['input'];
  width: Scalars['Decimal']['input'];
};

export type CreateParcelResult = {
  __typename?: 'CreateParcelResult';
  createdAt: Scalars['DateTime']['output'];
  estimatedDeliveryDate: Scalars['DateTime']['output'];
  id: Scalars['UUID']['output'];
  serviceType: ServiceType;
  status: ParcelStatus;
  trackingNumber: Scalars['String']['output'];
};

export type CreateRouteCommandInput = {
  driverId?: InputMaybe<Scalars['UUID']['input']>;
  name: Scalars['String']['input'];
  plannedStartTime: Scalars['DateTime']['input'];
  vehicleId?: InputMaybe<Scalars['UUID']['input']>;
  zoneId?: InputMaybe<Scalars['UUID']['input']>;
};

export type CreateZoneCommandInput = {
  depotId: Scalars['UUID']['input'];
  geoJson?: InputMaybe<Scalars['String']['input']>;
  isActive?: Scalars['Boolean']['input'];
  name: Scalars['String']['input'];
};

export type DailyOperatingHoursInput = {
  closeTime?: InputMaybe<Scalars['LocalTime']['input']>;
  dayOfWeek: DayOfWeek;
  openTime?: InputMaybe<Scalars['LocalTime']['input']>;
};

export enum DataScope {
  All = 'ALL',
  Department = 'DEPARTMENT',
  None = 'NONE',
  Own = 'OWN'
}

export type DataScopeOperationFilterInput = {
  eq?: InputMaybe<DataScope>;
  in?: InputMaybe<Array<DataScope>>;
  neq?: InputMaybe<DataScope>;
  nin?: InputMaybe<Array<DataScope>>;
};

export type DateTimeOperationFilterInput = {
  eq?: InputMaybe<Scalars['DateTime']['input']>;
  gt?: InputMaybe<Scalars['DateTime']['input']>;
  gte?: InputMaybe<Scalars['DateTime']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['DateTime']['input']>>>;
  lt?: InputMaybe<Scalars['DateTime']['input']>;
  lte?: InputMaybe<Scalars['DateTime']['input']>;
  neq?: InputMaybe<Scalars['DateTime']['input']>;
  ngt?: InputMaybe<Scalars['DateTime']['input']>;
  ngte?: InputMaybe<Scalars['DateTime']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['DateTime']['input']>>>;
  nlt?: InputMaybe<Scalars['DateTime']['input']>;
  nlte?: InputMaybe<Scalars['DateTime']['input']>;
};

export enum DayOfWeek {
  Friday = 'FRIDAY',
  Monday = 'MONDAY',
  Saturday = 'SATURDAY',
  Sunday = 'SUNDAY',
  Thursday = 'THURSDAY',
  Tuesday = 'TUESDAY',
  Wednesday = 'WEDNESDAY'
}

export type DayOfWeekOperationFilterInput = {
  eq?: InputMaybe<DayOfWeek>;
  in?: InputMaybe<Array<DayOfWeek>>;
  neq?: InputMaybe<DayOfWeek>;
  nin?: InputMaybe<Array<DayOfWeek>>;
};

export type DayOff = {
  __typename?: 'DayOff';
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  date: Scalars['DateTime']['output'];
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  driver: Driver;
  driverId: Scalars['UUID']['output'];
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
};

export type DayOffFilterInput = {
  and?: InputMaybe<Array<DayOffFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  date?: InputMaybe<DateTimeOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  driver?: InputMaybe<DriverFilterInput>;
  driverId?: InputMaybe<UuidOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<DayOffFilterInput>>;
};

export type DayOffInput = {
  date: Scalars['DateTime']['input'];
};

export type DayOffResult = {
  __typename?: 'DayOffResult';
  date: Scalars['DateTime']['output'];
};

export type DecimalOperationFilterInput = {
  eq?: InputMaybe<Scalars['Decimal']['input']>;
  gt?: InputMaybe<Scalars['Decimal']['input']>;
  gte?: InputMaybe<Scalars['Decimal']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['Decimal']['input']>>>;
  lt?: InputMaybe<Scalars['Decimal']['input']>;
  lte?: InputMaybe<Scalars['Decimal']['input']>;
  neq?: InputMaybe<Scalars['Decimal']['input']>;
  ngt?: InputMaybe<Scalars['Decimal']['input']>;
  ngte?: InputMaybe<Scalars['Decimal']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['Decimal']['input']>>>;
  nlt?: InputMaybe<Scalars['Decimal']['input']>;
  nlte?: InputMaybe<Scalars['Decimal']['input']>;
};

export type DeliveryConfirmation = {
  __typename?: 'DeliveryConfirmation';
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  deliveredAt: Scalars['DateTime']['output'];
  deliveryLocation?: Maybe<Scalars['String']['output']>;
  deliveryLocationCoords?: Maybe<GeoJsonPointType>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  parcel: Parcel;
  parcelId: Scalars['UUID']['output'];
  photo?: Maybe<Scalars['String']['output']>;
  receivedBy?: Maybe<Scalars['String']['output']>;
  signatureImage?: Maybe<Scalars['String']['output']>;
};

export type DeliveryConfirmationFilterInput = {
  and?: InputMaybe<Array<DeliveryConfirmationFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  deliveredAt?: InputMaybe<DateTimeOperationFilterInput>;
  deliveryLocation?: InputMaybe<StringOperationFilterInput>;
  deliveryLocationCoords?: InputMaybe<PointFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<DeliveryConfirmationFilterInput>>;
  parcel?: InputMaybe<ParcelFilterInput>;
  parcelId?: InputMaybe<UuidOperationFilterInput>;
  photo?: InputMaybe<StringOperationFilterInput>;
  receivedBy?: InputMaybe<StringOperationFilterInput>;
  signatureImage?: InputMaybe<StringOperationFilterInput>;
};

export type DeliveryConfirmationSortInput = {
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  deliveredAt?: InputMaybe<SortEnumType>;
  deliveryLocation?: InputMaybe<SortEnumType>;
  deliveryLocationCoords?: InputMaybe<PointSortInput>;
  id?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  parcel?: InputMaybe<ParcelSortInput>;
  parcelId?: InputMaybe<SortEnumType>;
  photo?: InputMaybe<SortEnumType>;
  receivedBy?: InputMaybe<SortEnumType>;
  signatureImage?: InputMaybe<SortEnumType>;
};

export type Depot = {
  __typename?: 'Depot';
  address: Address;
  addressId: Scalars['UUID']['output'];
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  isActive: Scalars['Boolean']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  name: Scalars['String']['output'];
  shiftSchedules: Array<ShiftSchedule>;
  zones: Array<Zone>;
};

export type DepotFilterInput = {
  address?: InputMaybe<AddressFilterInput>;
  addressId?: InputMaybe<UuidOperationFilterInput>;
  and?: InputMaybe<Array<DepotFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isActive?: InputMaybe<BooleanOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<DepotFilterInput>>;
  shiftSchedules?: InputMaybe<ListFilterInputTypeOfShiftScheduleFilterInput>;
  zones?: InputMaybe<ListFilterInputTypeOfZoneFilterInput>;
};

export type DepotLookupDto = {
  __typename?: 'DepotLookupDto';
  id: Scalars['UUID']['output'];
  name: Scalars['String']['output'];
};

export type DepotResult = {
  __typename?: 'DepotResult';
  createdAt: Scalars['DateTime']['output'];
  id: Scalars['UUID']['output'];
  isActive: Scalars['Boolean']['output'];
  name: Scalars['String']['output'];
};

export type DepotSortInput = {
  address?: InputMaybe<AddressSortInput>;
  addressId?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isActive?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type DepotsConnection = {
  __typename?: 'DepotsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<DepotsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Depot>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type DepotsEdge = {
  __typename?: 'DepotsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Depot;
};

export enum Dimension {
  Collapse = 'COLLAPSE',
  Curve = 'CURVE',
  Dontcare = 'DONTCARE',
  False = 'FALSE',
  Point = 'POINT',
  Surface = 'SURFACE',
  True = 'TRUE'
}

export type DimensionOperationFilterInput = {
  eq?: InputMaybe<Dimension>;
  in?: InputMaybe<Array<Dimension>>;
  neq?: InputMaybe<Dimension>;
  nin?: InputMaybe<Array<Dimension>>;
};

export enum DimensionUnit {
  Cm = 'CM',
  In = 'IN'
}

export type DimensionUnitOperationFilterInput = {
  eq?: InputMaybe<DimensionUnit>;
  in?: InputMaybe<Array<DimensionUnit>>;
  neq?: InputMaybe<DimensionUnit>;
  nin?: InputMaybe<Array<DimensionUnit>>;
};

export type DispatchRouteCommandInput = {
  routeId: Scalars['UUID']['input'];
};

export type Driver = {
  __typename?: 'Driver';
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  daysOff: Array<DayOff>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  licenseExpiryDate: Scalars['DateTime']['output'];
  licenseNumber: Scalars['String']['output'];
  photo?: Maybe<Scalars['String']['output']>;
  routes: Array<Route>;
  shiftSchedules: Array<ShiftSchedule>;
  user: User;
  userId: Scalars['UUID']['output'];
};

export type DriverFilterInput = {
  and?: InputMaybe<Array<DriverFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  daysOff?: InputMaybe<ListFilterInputTypeOfDayOffFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  licenseExpiryDate?: InputMaybe<DateTimeOperationFilterInput>;
  licenseNumber?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<DriverFilterInput>>;
  photo?: InputMaybe<StringOperationFilterInput>;
  routes?: InputMaybe<ListFilterInputTypeOfRouteFilterInput>;
  shiftSchedules?: InputMaybe<ListFilterInputTypeOfShiftScheduleFilterInput>;
  user?: InputMaybe<UserFilterInput>;
  userId?: InputMaybe<UuidOperationFilterInput>;
};

export type DriverResult = {
  __typename?: 'DriverResult';
  createdAt: Scalars['DateTime']['output'];
  daysOff?: Maybe<Array<DayOffResult>>;
  id: Scalars['UUID']['output'];
  licenseExpiryDate: Scalars['DateTime']['output'];
  licenseNumber: Scalars['String']['output'];
  photo?: Maybe<Scalars['String']['output']>;
  shiftSchedules?: Maybe<Array<ShiftScheduleResult>>;
  userId: Scalars['UUID']['output'];
};

export type DriverRouteSummaryDto = {
  __typename?: 'DriverRouteSummaryDto';
  id: Scalars['UUID']['output'];
  name: Scalars['String']['output'];
  status: RouteStatus;
};

export type DriverSortInput = {
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  licenseExpiryDate?: InputMaybe<SortEnumType>;
  licenseNumber?: InputMaybe<SortEnumType>;
  photo?: InputMaybe<SortEnumType>;
  user?: InputMaybe<UserSortInput>;
  userId?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type DriversConnection = {
  __typename?: 'DriversConnection';
  /** A list of edges. */
  edges?: Maybe<Array<DriversEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Driver>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type DriversEdge = {
  __typename?: 'DriversEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Driver;
};

export type ElevationModelFilterInput = {
  and?: InputMaybe<Array<ElevationModelFilterInput>>;
  extent?: InputMaybe<EnvelopeFilterInput>;
  or?: InputMaybe<Array<ElevationModelFilterInput>>;
};

export type ElevationModelSortInput = {
  extent?: InputMaybe<EnvelopeSortInput>;
};

export type EnvelopeFilterInput = {
  and?: InputMaybe<Array<EnvelopeFilterInput>>;
  area?: InputMaybe<FloatOperationFilterInput>;
  centre?: InputMaybe<CoordinateFilterInput>;
  diameter?: InputMaybe<FloatOperationFilterInput>;
  height?: InputMaybe<FloatOperationFilterInput>;
  isNull?: InputMaybe<BooleanOperationFilterInput>;
  maxExtent?: InputMaybe<FloatOperationFilterInput>;
  maxX?: InputMaybe<FloatOperationFilterInput>;
  maxY?: InputMaybe<FloatOperationFilterInput>;
  minExtent?: InputMaybe<FloatOperationFilterInput>;
  minX?: InputMaybe<FloatOperationFilterInput>;
  minY?: InputMaybe<FloatOperationFilterInput>;
  or?: InputMaybe<Array<EnvelopeFilterInput>>;
  width?: InputMaybe<FloatOperationFilterInput>;
};

export type EnvelopeSortInput = {
  area?: InputMaybe<SortEnumType>;
  centre?: InputMaybe<CoordinateSortInput>;
  diameter?: InputMaybe<SortEnumType>;
  height?: InputMaybe<SortEnumType>;
  isNull?: InputMaybe<SortEnumType>;
  maxExtent?: InputMaybe<SortEnumType>;
  maxX?: InputMaybe<SortEnumType>;
  maxY?: InputMaybe<SortEnumType>;
  minExtent?: InputMaybe<SortEnumType>;
  minX?: InputMaybe<SortEnumType>;
  minY?: InputMaybe<SortEnumType>;
  width?: InputMaybe<SortEnumType>;
};

export enum EventType {
  AddressCorrection = 'ADDRESS_CORRECTION',
  ArrivedAtFacility = 'ARRIVED_AT_FACILITY',
  CustomsClearance = 'CUSTOMS_CLEARANCE',
  Delivered = 'DELIVERED',
  DeliveryAttempted = 'DELIVERY_ATTEMPTED',
  DepartedFacility = 'DEPARTED_FACILITY',
  Exception = 'EXCEPTION',
  HeldAtFacility = 'HELD_AT_FACILITY',
  InTransit = 'IN_TRANSIT',
  LabelCreated = 'LABEL_CREATED',
  OutForDelivery = 'OUT_FOR_DELIVERY',
  PickedUp = 'PICKED_UP',
  Returned = 'RETURNED'
}

export type EventTypeOperationFilterInput = {
  eq?: InputMaybe<EventType>;
  in?: InputMaybe<Array<EventType>>;
  neq?: InputMaybe<EventType>;
  nin?: InputMaybe<Array<EventType>>;
};

export enum ExceptionReason {
  AddressNotFound = 'ADDRESS_NOT_FOUND',
  BadLabel = 'BAD_LABEL',
  CustomerHold = 'CUSTOMER_HOLD',
  CustomsHold = 'CUSTOMS_HOLD',
  DamagedInTransit = 'DAMAGED_IN_TRANSIT',
  RecipientUnavailable = 'RECIPIENT_UNAVAILABLE',
  RefusedByRecipient = 'REFUSED_BY_RECIPIENT',
  Unidentified = 'UNIDENTIFIED',
  WeatherDelay = 'WEATHER_DELAY'
}

export type FloatOperationFilterInput = {
  eq?: InputMaybe<Scalars['Float']['input']>;
  gt?: InputMaybe<Scalars['Float']['input']>;
  gte?: InputMaybe<Scalars['Float']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['Float']['input']>>>;
  lt?: InputMaybe<Scalars['Float']['input']>;
  lte?: InputMaybe<Scalars['Float']['input']>;
  neq?: InputMaybe<Scalars['Float']['input']>;
  ngt?: InputMaybe<Scalars['Float']['input']>;
  ngte?: InputMaybe<Scalars['Float']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['Float']['input']>>>;
  nlt?: InputMaybe<Scalars['Float']['input']>;
  nlte?: InputMaybe<Scalars['Float']['input']>;
};

export enum GeoJsonGeometryType {
  GeometryCollection = 'GeometryCollection',
  LineString = 'LineString',
  MultiLineString = 'MultiLineString',
  MultiPoint = 'MultiPoint',
  MultiPolygon = 'MultiPolygon',
  Point = 'Point',
  Polygon = 'Polygon'
}

export type GeoJsonInterface = {
  /** The minimum bounding box around the geometry object */
  bbox?: Maybe<Array<Maybe<Scalars['Float']['output']>>>;
  /** The coordinate reference system integer identifier */
  crs?: Maybe<Scalars['Int']['output']>;
  /** The geometry type of the GeoJson object */
  type: GeoJsonGeometryType;
};

export type GeoJsonLineStringInput = {
  /** The "coordinates" field is an array of two or more positions. */
  coordinates?: InputMaybe<Array<InputMaybe<Scalars['Position']['input']>>>;
  /** The coordinate reference system integer identifier */
  crs?: InputMaybe<Scalars['Int']['input']>;
  /** The geometry type of the GeoJson object */
  type?: InputMaybe<GeoJsonGeometryType>;
};

export type GeoJsonLineStringType = GeoJsonInterface & {
  __typename?: 'GeoJSONLineStringType';
  /** The minimum bounding box around the geometry object */
  bbox: Array<Scalars['Float']['output']>;
  /** The "coordinates" field is an array of two or more positions. */
  coordinates?: Maybe<Array<Maybe<Scalars['Position']['output']>>>;
  /** The coordinate reference system integer identifier */
  crs: Scalars['Int']['output'];
  /** The geometry type of the GeoJson object */
  type: GeoJsonGeometryType;
};

export type GeoJsonMultiLineStringInput = {
  /** The "coordinates" field is an array of LineString coordinate arrays. */
  coordinates?: InputMaybe<Array<InputMaybe<Array<InputMaybe<Scalars['Position']['input']>>>>>;
  /** The coordinate reference system integer identifier */
  crs?: InputMaybe<Scalars['Int']['input']>;
  /** The geometry type of the GeoJson object */
  type?: InputMaybe<GeoJsonGeometryType>;
};

export type GeoJsonMultiLineStringType = GeoJsonInterface & {
  __typename?: 'GeoJSONMultiLineStringType';
  /** The minimum bounding box around the geometry object */
  bbox: Array<Scalars['Float']['output']>;
  /** The "coordinates" field is an array of LineString coordinate arrays. */
  coordinates?: Maybe<Array<Maybe<Array<Maybe<Scalars['Position']['output']>>>>>;
  /** The coordinate reference system integer identifier */
  crs: Scalars['Int']['output'];
  /** The geometry type of the GeoJson object */
  type: GeoJsonGeometryType;
};

export type GeoJsonMultiPointInput = {
  /** The "coordinates" field is an array of positions. */
  coordinates?: InputMaybe<Array<InputMaybe<Scalars['Position']['input']>>>;
  /** The coordinate reference system integer identifier */
  crs?: InputMaybe<Scalars['Int']['input']>;
  /** The geometry type of the GeoJson object */
  type?: InputMaybe<GeoJsonGeometryType>;
};

export type GeoJsonMultiPointType = GeoJsonInterface & {
  __typename?: 'GeoJSONMultiPointType';
  /** The minimum bounding box around the geometry object */
  bbox: Array<Scalars['Float']['output']>;
  /** The "coordinates" field is an array of positions. */
  coordinates?: Maybe<Array<Maybe<Scalars['Position']['output']>>>;
  /** The coordinate reference system integer identifier */
  crs: Scalars['Int']['output'];
  /** The geometry type of the GeoJson object */
  type: GeoJsonGeometryType;
};

export type GeoJsonMultiPolygonInput = {
  /** The "coordinates" field is an array of Polygon coordinate arrays. */
  coordinates?: InputMaybe<Scalars['Coordinates']['input']>;
  /** The coordinate reference system integer identifier */
  crs?: InputMaybe<Scalars['Int']['input']>;
  /** The geometry type of the GeoJson object */
  type?: InputMaybe<GeoJsonGeometryType>;
};

export type GeoJsonMultiPolygonType = GeoJsonInterface & {
  __typename?: 'GeoJSONMultiPolygonType';
  /** The minimum bounding box around the geometry object */
  bbox: Array<Scalars['Float']['output']>;
  /** The "coordinates" field is an array of Polygon coordinate arrays. */
  coordinates?: Maybe<Scalars['Coordinates']['output']>;
  /** The coordinate reference system integer identifier */
  crs: Scalars['Int']['output'];
  /** The geometry type of the GeoJson object */
  type: GeoJsonGeometryType;
};

export type GeoJsonPointInput = {
  /** The "coordinates" field is a single position. */
  coordinates?: InputMaybe<Scalars['Position']['input']>;
  /** The coordinate reference system integer identifier */
  crs?: InputMaybe<Scalars['Int']['input']>;
  /** The geometry type of the GeoJson object */
  type?: InputMaybe<GeoJsonGeometryType>;
};

export type GeoJsonPointType = GeoJsonInterface & {
  __typename?: 'GeoJSONPointType';
  /** The minimum bounding box around the geometry object */
  bbox: Array<Scalars['Float']['output']>;
  /** The "coordinates" field is a single position. */
  coordinates?: Maybe<Scalars['Position']['output']>;
  /** The coordinate reference system integer identifier */
  crs: Scalars['Int']['output'];
  /** The geometry type of the GeoJson object */
  type: GeoJsonGeometryType;
};

export type GeoJsonPolygonInput = {
  /** The "coordinates" field MUST be an array of linear ring coordinate arrays. For Polygons with more than one of these rings, the first MUST be the exterior ring, and any others MUST be interior rings. The exterior ring bounds the surface, and the interior rings (if present) bound holes within the surface. */
  coordinates?: InputMaybe<Array<InputMaybe<Array<InputMaybe<Scalars['Position']['input']>>>>>;
  /** The coordinate reference system integer identifier */
  crs?: InputMaybe<Scalars['Int']['input']>;
  /** The geometry type of the GeoJson object */
  type?: InputMaybe<GeoJsonGeometryType>;
};

export type GeoJsonPolygonType = GeoJsonInterface & {
  __typename?: 'GeoJSONPolygonType';
  /** The minimum bounding box around the geometry object */
  bbox: Array<Scalars['Float']['output']>;
  /** The "coordinates" field MUST be an array of linear ring coordinate arrays. For Polygons with more than one of these rings, the first MUST be the exterior ring, and any others MUST be interior rings. The exterior ring bounds the surface, and the interior rings (if present) bound holes within the surface. */
  coordinates?: Maybe<Array<Maybe<Array<Maybe<Scalars['Position']['output']>>>>>;
  /** The coordinate reference system integer identifier */
  crs: Scalars['Int']['output'];
  /** The geometry type of the GeoJson object */
  type: GeoJsonGeometryType;
};

export type GeometryFactoryFilterInput = {
  and?: InputMaybe<Array<GeometryFactoryFilterInput>>;
  coordinateSequenceFactory?: InputMaybe<CoordinateSequenceFactoryFilterInput>;
  elevationModel?: InputMaybe<ElevationModelFilterInput>;
  geometryServices?: InputMaybe<NtsGeometryServicesFilterInput>;
  or?: InputMaybe<Array<GeometryFactoryFilterInput>>;
  precisionModel?: InputMaybe<PrecisionModelFilterInput>;
  srid?: InputMaybe<IntOperationFilterInput>;
};

export type GeometryFactorySortInput = {
  coordinateSequenceFactory?: InputMaybe<CoordinateSequenceFactorySortInput>;
  elevationModel?: InputMaybe<ElevationModelSortInput>;
  geometryServices?: InputMaybe<NtsGeometryServicesSortInput>;
  precisionModel?: InputMaybe<PrecisionModelSortInput>;
  srid?: InputMaybe<SortEnumType>;
};

export type GeometryFilterInput = {
  and?: InputMaybe<Array<GeometryFilterInput>>;
  area?: InputMaybe<FloatOperationFilterInput>;
  boundary?: InputMaybe<GeometryFilterInput>;
  boundaryDimension?: InputMaybe<DimensionOperationFilterInput>;
  centroid?: InputMaybe<PointFilterInput>;
  coordinate?: InputMaybe<CoordinateFilterInput>;
  coordinates?: InputMaybe<ListFilterInputTypeOfCoordinateFilterInput>;
  dimension?: InputMaybe<DimensionOperationFilterInput>;
  envelope?: InputMaybe<GeometryFilterInput>;
  envelopeInternal?: InputMaybe<EnvelopeFilterInput>;
  factory?: InputMaybe<GeometryFactoryFilterInput>;
  geometryType?: InputMaybe<StringOperationFilterInput>;
  interiorPoint?: InputMaybe<PointFilterInput>;
  isEmpty?: InputMaybe<BooleanOperationFilterInput>;
  isRectangle?: InputMaybe<BooleanOperationFilterInput>;
  isSimple?: InputMaybe<BooleanOperationFilterInput>;
  isValid?: InputMaybe<BooleanOperationFilterInput>;
  length?: InputMaybe<FloatOperationFilterInput>;
  numGeometries?: InputMaybe<IntOperationFilterInput>;
  numPoints?: InputMaybe<IntOperationFilterInput>;
  ogcGeometryType?: InputMaybe<OgcGeometryTypeOperationFilterInput>;
  or?: InputMaybe<Array<GeometryFilterInput>>;
  pointOnSurface?: InputMaybe<PointFilterInput>;
  precisionModel?: InputMaybe<PrecisionModelFilterInput>;
  srid?: InputMaybe<IntOperationFilterInput>;
};

export type GeometryOverlayFilterInput = {
  and?: InputMaybe<Array<GeometryOverlayFilterInput>>;
  or?: InputMaybe<Array<GeometryOverlayFilterInput>>;
};

export type GeometryRelateFilterInput = {
  and?: InputMaybe<Array<GeometryRelateFilterInput>>;
  or?: InputMaybe<Array<GeometryRelateFilterInput>>;
};

export type GeometrySortInput = {
  area?: InputMaybe<SortEnumType>;
  boundary?: InputMaybe<GeometrySortInput>;
  boundaryDimension?: InputMaybe<SortEnumType>;
  centroid?: InputMaybe<PointSortInput>;
  coordinate?: InputMaybe<CoordinateSortInput>;
  dimension?: InputMaybe<SortEnumType>;
  envelope?: InputMaybe<GeometrySortInput>;
  envelopeInternal?: InputMaybe<EnvelopeSortInput>;
  factory?: InputMaybe<GeometryFactorySortInput>;
  geometryType?: InputMaybe<SortEnumType>;
  interiorPoint?: InputMaybe<PointSortInput>;
  isEmpty?: InputMaybe<SortEnumType>;
  isRectangle?: InputMaybe<SortEnumType>;
  isSimple?: InputMaybe<SortEnumType>;
  isValid?: InputMaybe<SortEnumType>;
  length?: InputMaybe<SortEnumType>;
  numGeometries?: InputMaybe<SortEnumType>;
  numPoints?: InputMaybe<SortEnumType>;
  ogcGeometryType?: InputMaybe<SortEnumType>;
  pointOnSurface?: InputMaybe<PointSortInput>;
  precisionModel?: InputMaybe<PrecisionModelSortInput>;
  srid?: InputMaybe<SortEnumType>;
};

export type IntOperationFilterInput = {
  eq?: InputMaybe<Scalars['Int']['input']>;
  gt?: InputMaybe<Scalars['Int']['input']>;
  gte?: InputMaybe<Scalars['Int']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['Int']['input']>>>;
  lt?: InputMaybe<Scalars['Int']['input']>;
  lte?: InputMaybe<Scalars['Int']['input']>;
  neq?: InputMaybe<Scalars['Int']['input']>;
  ngt?: InputMaybe<Scalars['Int']['input']>;
  ngte?: InputMaybe<Scalars['Int']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['Int']['input']>>>;
  nlt?: InputMaybe<Scalars['Int']['input']>;
  nlte?: InputMaybe<Scalars['Int']['input']>;
};

export type ListFilterInputTypeOfAisleFilterInput = {
  all?: InputMaybe<AisleFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<AisleFilterInput>;
  some?: InputMaybe<AisleFilterInput>;
};

export type ListFilterInputTypeOfBinFilterInput = {
  all?: InputMaybe<BinFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<BinFilterInput>;
  some?: InputMaybe<BinFilterInput>;
};

export type ListFilterInputTypeOfCoordinateFilterInput = {
  all?: InputMaybe<CoordinateFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<CoordinateFilterInput>;
  some?: InputMaybe<CoordinateFilterInput>;
};

export type ListFilterInputTypeOfDayOffFilterInput = {
  all?: InputMaybe<DayOffFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<DayOffFilterInput>;
  some?: InputMaybe<DayOffFilterInput>;
};

export type ListFilterInputTypeOfParcelAuditLogFilterInput = {
  all?: InputMaybe<ParcelAuditLogFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<ParcelAuditLogFilterInput>;
  some?: InputMaybe<ParcelAuditLogFilterInput>;
};

export type ListFilterInputTypeOfParcelContentItemFilterInput = {
  all?: InputMaybe<ParcelContentItemFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<ParcelContentItemFilterInput>;
  some?: InputMaybe<ParcelContentItemFilterInput>;
};

export type ListFilterInputTypeOfParcelFilterInput = {
  all?: InputMaybe<ParcelFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<ParcelFilterInput>;
  some?: InputMaybe<ParcelFilterInput>;
};

export type ListFilterInputTypeOfParcelWatcherFilterInput = {
  all?: InputMaybe<ParcelWatcherFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<ParcelWatcherFilterInput>;
  some?: InputMaybe<ParcelWatcherFilterInput>;
};

export type ListFilterInputTypeOfRolePermissionFilterInput = {
  all?: InputMaybe<RolePermissionFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<RolePermissionFilterInput>;
  some?: InputMaybe<RolePermissionFilterInput>;
};

export type ListFilterInputTypeOfRouteFilterInput = {
  all?: InputMaybe<RouteFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<RouteFilterInput>;
  some?: InputMaybe<RouteFilterInput>;
};

export type ListFilterInputTypeOfRouteStopFilterInput = {
  all?: InputMaybe<RouteStopFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<RouteStopFilterInput>;
  some?: InputMaybe<RouteStopFilterInput>;
};

export type ListFilterInputTypeOfShiftScheduleFilterInput = {
  all?: InputMaybe<ShiftScheduleFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<ShiftScheduleFilterInput>;
  some?: InputMaybe<ShiftScheduleFilterInput>;
};

export type ListFilterInputTypeOfTrackingEventFilterInput = {
  all?: InputMaybe<TrackingEventFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<TrackingEventFilterInput>;
  some?: InputMaybe<TrackingEventFilterInput>;
};

export type ListFilterInputTypeOfUserFilterInput = {
  all?: InputMaybe<UserFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<UserFilterInput>;
  some?: InputMaybe<UserFilterInput>;
};

export type ListFilterInputTypeOfVehicleJourneyFilterInput = {
  all?: InputMaybe<VehicleJourneyFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<VehicleJourneyFilterInput>;
  some?: InputMaybe<VehicleJourneyFilterInput>;
};

export type ListFilterInputTypeOfZoneFilterInput = {
  all?: InputMaybe<ZoneFilterInput>;
  any?: InputMaybe<Scalars['Boolean']['input']>;
  none?: InputMaybe<ZoneFilterInput>;
  some?: InputMaybe<ZoneFilterInput>;
};

export type LocalTimeOperationFilterInput = {
  eq?: InputMaybe<Scalars['LocalTime']['input']>;
  gt?: InputMaybe<Scalars['LocalTime']['input']>;
  gte?: InputMaybe<Scalars['LocalTime']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['LocalTime']['input']>>>;
  lt?: InputMaybe<Scalars['LocalTime']['input']>;
  lte?: InputMaybe<Scalars['LocalTime']['input']>;
  neq?: InputMaybe<Scalars['LocalTime']['input']>;
  ngt?: InputMaybe<Scalars['LocalTime']['input']>;
  ngte?: InputMaybe<Scalars['LocalTime']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['LocalTime']['input']>>>;
  nlt?: InputMaybe<Scalars['LocalTime']['input']>;
  nlte?: InputMaybe<Scalars['LocalTime']['input']>;
};

export type Mutation = {
  __typename?: 'Mutation';
  activateUser: UserDto;
  addParcelsToRoute: RouteDto;
  assignDriverToRoute: RouteDto;
  autoAssignParcelsByZone: RouteDto;
  cancelParcel: CancelParcelResult;
  changeParcelStatus: ChangeParcelStatusResult;
  changeRouteStatus: RouteDto;
  changeVehicleStatus: VehicleDto;
  completePasswordReset: Scalars['Boolean']['output'];
  createAisle: AisleResult;
  createBin: BinResult;
  createDepot: DepotResult;
  createDriver: DriverResult;
  createParcel: CreateParcelResult;
  createRoute: RouteDto;
  createUser: UserDto;
  createVehicle: VehicleDto;
  createZone: ZoneResult;
  deactivateUser: UserDto;
  deleteAisle: Scalars['Boolean']['output'];
  deleteBin: Scalars['Boolean']['output'];
  deleteDepot: Scalars['Boolean']['output'];
  deleteDriver: Scalars['Boolean']['output'];
  deleteRoute: Scalars['Boolean']['output'];
  deleteVehicle: Scalars['Boolean']['output'];
  deleteZone: Scalars['Boolean']['output'];
  dispatchRoute: RouteDto;
  optimizeRouteStopOrder: RouteDto;
  removeParcelsFromRoute: RouteDto;
  reorderRouteStops: RouteDto;
  resetPassword: Scalars['Boolean']['output'];
  sentinel?: Maybe<Scalars['String']['output']>;
  updateAisle: AisleResult;
  updateBin: BinResult;
  updateDepot: DepotResult;
  updateDriver: DriverResult;
  updateParcel: UpdateParcelResult;
  updateRoute: RouteDto;
  updateUser: UserDto;
  updateVehicle: VehicleDto;
  updateZone: ZoneResult;
};


export type MutationActivateUserArgs = {
  userId: Scalars['UUID']['input'];
};


export type MutationAddParcelsToRouteArgs = {
  input: AddParcelsToRouteCommandInput;
};


export type MutationAssignDriverToRouteArgs = {
  driverId?: InputMaybe<Scalars['UUID']['input']>;
  routeId: Scalars['UUID']['input'];
};


export type MutationAutoAssignParcelsByZoneArgs = {
  input: AutoAssignParcelsByZoneCommandInput;
};


export type MutationCancelParcelArgs = {
  input: CancelParcelCommandInput;
};


export type MutationChangeParcelStatusArgs = {
  input: ChangeParcelStatusCommandInput;
};


export type MutationChangeRouteStatusArgs = {
  id: Scalars['UUID']['input'];
  newStatus: RouteStatus;
};


export type MutationChangeVehicleStatusArgs = {
  id: Scalars['UUID']['input'];
  newStatus: VehicleStatus;
};


export type MutationCompletePasswordResetArgs = {
  email: Scalars['String']['input'];
  newPassword: Scalars['String']['input'];
  token: Scalars['String']['input'];
};


export type MutationCreateAisleArgs = {
  input: CreateAisleCommandInput;
};


export type MutationCreateBinArgs = {
  input: CreateBinCommandInput;
};


export type MutationCreateDepotArgs = {
  input: CreateDepotCommandInput;
};


export type MutationCreateDriverArgs = {
  input: CreateDriverCommandInput;
};


export type MutationCreateParcelArgs = {
  input: CreateParcelCommandInput;
};


export type MutationCreateRouteArgs = {
  input: CreateRouteCommandInput;
};


export type MutationCreateUserArgs = {
  depotId?: InputMaybe<Scalars['UUID']['input']>;
  email: Scalars['String']['input'];
  firstName: Scalars['String']['input'];
  lastName: Scalars['String']['input'];
  password: Scalars['String']['input'];
  phone?: InputMaybe<Scalars['String']['input']>;
  roleId: Scalars['UUID']['input'];
  zoneId?: InputMaybe<Scalars['UUID']['input']>;
};


export type MutationCreateVehicleArgs = {
  depotId?: InputMaybe<Scalars['UUID']['input']>;
  parcelCapacity: Scalars['Int']['input'];
  registrationPlate: Scalars['String']['input'];
  type: VehicleType;
  weightCapacityKg: Scalars['Decimal']['input'];
};


export type MutationCreateZoneArgs = {
  input: CreateZoneCommandInput;
};


export type MutationDeactivateUserArgs = {
  userId: Scalars['UUID']['input'];
};


export type MutationDeleteAisleArgs = {
  id: Scalars['UUID']['input'];
};


export type MutationDeleteBinArgs = {
  id: Scalars['UUID']['input'];
};


export type MutationDeleteDepotArgs = {
  id: Scalars['UUID']['input'];
};


export type MutationDeleteDriverArgs = {
  id: Scalars['UUID']['input'];
};


export type MutationDeleteRouteArgs = {
  id: Scalars['UUID']['input'];
};


export type MutationDeleteVehicleArgs = {
  id: Scalars['UUID']['input'];
};


export type MutationDeleteZoneArgs = {
  id: Scalars['UUID']['input'];
};


export type MutationDispatchRouteArgs = {
  input: DispatchRouteCommandInput;
};


export type MutationOptimizeRouteStopOrderArgs = {
  input: OptimizeRouteStopOrderCommandInput;
};


export type MutationRemoveParcelsFromRouteArgs = {
  input: RemoveParcelsFromRouteCommandInput;
};


export type MutationReorderRouteStopsArgs = {
  input: ReorderRouteStopsCommandInput;
};


export type MutationResetPasswordArgs = {
  email: Scalars['String']['input'];
};


export type MutationUpdateAisleArgs = {
  input: UpdateAisleCommandInput;
};


export type MutationUpdateBinArgs = {
  input: UpdateBinCommandInput;
};


export type MutationUpdateDepotArgs = {
  input: UpdateDepotCommandInput;
};


export type MutationUpdateDriverArgs = {
  input: UpdateDriverCommandInput;
};


export type MutationUpdateParcelArgs = {
  input: UpdateParcelCommandInput;
};


export type MutationUpdateRouteArgs = {
  input: UpdateRouteCommandInput;
};


export type MutationUpdateUserArgs = {
  depotId?: InputMaybe<Scalars['UUID']['input']>;
  email: Scalars['String']['input'];
  firstName: Scalars['String']['input'];
  lastName: Scalars['String']['input'];
  phone?: InputMaybe<Scalars['String']['input']>;
  roleId?: InputMaybe<Scalars['UUID']['input']>;
  userId: Scalars['UUID']['input'];
  zoneId?: InputMaybe<Scalars['UUID']['input']>;
};


export type MutationUpdateVehicleArgs = {
  depotId?: InputMaybe<Scalars['UUID']['input']>;
  id: Scalars['UUID']['input'];
  parcelCapacity: Scalars['Int']['input'];
  registrationPlate: Scalars['String']['input'];
  type: VehicleType;
  weightCapacityKg: Scalars['Decimal']['input'];
};


export type MutationUpdateZoneArgs = {
  input: UpdateZoneCommandInput;
};

export type NtsGeometryServicesFilterInput = {
  and?: InputMaybe<Array<NtsGeometryServicesFilterInput>>;
  coordinateEqualityComparer?: InputMaybe<CoordinateEqualityComparerFilterInput>;
  defaultCoordinateSequenceFactory?: InputMaybe<CoordinateSequenceFactoryFilterInput>;
  defaultElevationModel?: InputMaybe<ElevationModelFilterInput>;
  defaultPrecisionModel?: InputMaybe<PrecisionModelFilterInput>;
  defaultSRID?: InputMaybe<IntOperationFilterInput>;
  geometryOverlay?: InputMaybe<GeometryOverlayFilterInput>;
  geometryRelate?: InputMaybe<GeometryRelateFilterInput>;
  or?: InputMaybe<Array<NtsGeometryServicesFilterInput>>;
};

export type NtsGeometryServicesSortInput = {
  defaultCoordinateSequenceFactory?: InputMaybe<CoordinateSequenceFactorySortInput>;
  defaultElevationModel?: InputMaybe<ElevationModelSortInput>;
  defaultPrecisionModel?: InputMaybe<PrecisionModelSortInput>;
  defaultSRID?: InputMaybe<SortEnumType>;
};

export type NullableOfExceptionReasonOperationFilterInput = {
  eq?: InputMaybe<ExceptionReason>;
  in?: InputMaybe<Array<InputMaybe<ExceptionReason>>>;
  neq?: InputMaybe<ExceptionReason>;
  nin?: InputMaybe<Array<InputMaybe<ExceptionReason>>>;
};

export type NullableOfParcelTypeOperationFilterInput = {
  eq?: InputMaybe<ParcelType>;
  in?: InputMaybe<Array<InputMaybe<ParcelType>>>;
  neq?: InputMaybe<ParcelType>;
  nin?: InputMaybe<Array<InputMaybe<ParcelType>>>;
};

export enum OgcGeometryType {
  CircularString = 'CIRCULAR_STRING',
  CompoundCurve = 'COMPOUND_CURVE',
  Curve = 'CURVE',
  CurvePolygon = 'CURVE_POLYGON',
  GeometryCollection = 'GEOMETRY_COLLECTION',
  LineString = 'LINE_STRING',
  MultiCurve = 'MULTI_CURVE',
  MultiLineString = 'MULTI_LINE_STRING',
  MultiPoint = 'MULTI_POINT',
  MultiPolygon = 'MULTI_POLYGON',
  MultiSurface = 'MULTI_SURFACE',
  Point = 'POINT',
  Polygon = 'POLYGON',
  PolyhedralSurface = 'POLYHEDRAL_SURFACE',
  Surface = 'SURFACE',
  Tin = 'TIN'
}

export type OgcGeometryTypeOperationFilterInput = {
  eq?: InputMaybe<OgcGeometryType>;
  in?: InputMaybe<Array<OgcGeometryType>>;
  neq?: InputMaybe<OgcGeometryType>;
  nin?: InputMaybe<Array<OgcGeometryType>>;
};

export type OptimizeRouteStopOrderCommandInput = {
  routeId: Scalars['UUID']['input'];
};

export enum Ordinates {
  AllMeasureOrdinates = 'ALL_MEASURE_ORDINATES',
  AllOrdinates = 'ALL_ORDINATES',
  AllSpatialOrdinates = 'ALL_SPATIAL_ORDINATES',
  Measure1 = 'MEASURE1',
  Measure2 = 'MEASURE2',
  Measure3 = 'MEASURE3',
  Measure4 = 'MEASURE4',
  Measure5 = 'MEASURE5',
  Measure6 = 'MEASURE6',
  Measure7 = 'MEASURE7',
  Measure8 = 'MEASURE8',
  Measure9 = 'MEASURE9',
  Measure10 = 'MEASURE10',
  Measure11 = 'MEASURE11',
  Measure12 = 'MEASURE12',
  Measure13 = 'MEASURE13',
  Measure14 = 'MEASURE14',
  Measure15 = 'MEASURE15',
  Measure16 = 'MEASURE16',
  None = 'NONE',
  Spatial1 = 'SPATIAL1',
  Spatial2 = 'SPATIAL2',
  Spatial3 = 'SPATIAL3',
  Spatial4 = 'SPATIAL4',
  Spatial5 = 'SPATIAL5',
  Spatial6 = 'SPATIAL6',
  Spatial7 = 'SPATIAL7',
  Spatial8 = 'SPATIAL8',
  Spatial9 = 'SPATIAL9',
  Spatial10 = 'SPATIAL10',
  Spatial11 = 'SPATIAL11',
  Spatial12 = 'SPATIAL12',
  Spatial13 = 'SPATIAL13',
  Spatial14 = 'SPATIAL14',
  Spatial15 = 'SPATIAL15',
  Spatial16 = 'SPATIAL16',
  Xy = 'XY',
  Xym = 'XYM',
  Xyz = 'XYZ',
  Xyzm = 'XYZM'
}

export type OrdinatesOperationFilterInput = {
  eq?: InputMaybe<Ordinates>;
  in?: InputMaybe<Array<Ordinates>>;
  neq?: InputMaybe<Ordinates>;
  nin?: InputMaybe<Array<Ordinates>>;
};

/** Information about pagination in a connection. */
export type PageInfo = {
  __typename?: 'PageInfo';
  /** When paginating forwards, the cursor to continue. */
  endCursor?: Maybe<Scalars['String']['output']>;
  /** Indicates whether more edges exist following the set defined by the clients arguments. */
  hasNextPage: Scalars['Boolean']['output'];
  /** Indicates whether more edges exist prior the set defined by the clients arguments. */
  hasPreviousPage: Scalars['Boolean']['output'];
  /** When paginating backwards, the cursor to continue. */
  startCursor?: Maybe<Scalars['String']['output']>;
};

export type Parcel = {
  __typename?: 'Parcel';
  actualDeliveryDate?: Maybe<Scalars['DateTime']['output']>;
  auditLogs: Array<ParcelAuditLog>;
  bin?: Maybe<Bin>;
  binId?: Maybe<Scalars['UUID']['output']>;
  canTransitionTo: Scalars['Boolean']['output'];
  contentItems: Array<ParcelContentItem>;
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  currency: Scalars['String']['output'];
  declaredValue: Scalars['Decimal']['output'];
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  deliveryAttempts: Scalars['Int']['output'];
  deliveryConfirmation?: Maybe<DeliveryConfirmation>;
  description?: Maybe<Scalars['String']['output']>;
  dimensionUnit: DimensionUnit;
  estimatedDeliveryDate?: Maybe<Scalars['DateTime']['output']>;
  height: Scalars['Decimal']['output'];
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  length: Scalars['Decimal']['output'];
  notes?: Maybe<Scalars['String']['output']>;
  parcelType?: Maybe<ParcelType>;
  parcelWatchers: Array<ParcelWatcher>;
  recipientAddress: Address;
  recipientAddressId: Scalars['UUID']['output'];
  routeStop?: Maybe<RouteStop>;
  routeStopId?: Maybe<Scalars['UUID']['output']>;
  serviceType: ServiceType;
  shipperAddress: Address;
  shipperAddressId: Scalars['UUID']['output'];
  status: ParcelStatus;
  trackingEvents: Array<TrackingEvent>;
  trackingNumber: Scalars['String']['output'];
  weight: Scalars['Decimal']['output'];
  weightUnit: WeightUnit;
  width: Scalars['Decimal']['output'];
  zone?: Maybe<Zone>;
  zoneId?: Maybe<Scalars['UUID']['output']>;
};


export type ParcelCanTransitionToArgs = {
  newStatus: ParcelStatus;
};

export type ParcelAddressInput = {
  city: Scalars['String']['input'];
  companyName?: InputMaybe<Scalars['String']['input']>;
  contactName?: InputMaybe<Scalars['String']['input']>;
  countryCode?: InputMaybe<Scalars['String']['input']>;
  email?: InputMaybe<Scalars['String']['input']>;
  isResidential?: Scalars['Boolean']['input'];
  phone?: InputMaybe<Scalars['String']['input']>;
  postalCode: Scalars['String']['input'];
  state: Scalars['String']['input'];
  street1: Scalars['String']['input'];
  street2?: InputMaybe<Scalars['String']['input']>;
};

export type ParcelAuditLog = {
  __typename?: 'ParcelAuditLog';
  changedBy: Scalars['String']['output'];
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  newValue?: Maybe<Scalars['String']['output']>;
  oldValue?: Maybe<Scalars['String']['output']>;
  parcel: Parcel;
  parcelId: Scalars['UUID']['output'];
  propertyName: Scalars['String']['output'];
};

export type ParcelAuditLogFilterInput = {
  and?: InputMaybe<Array<ParcelAuditLogFilterInput>>;
  changedBy?: InputMaybe<StringOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  newValue?: InputMaybe<StringOperationFilterInput>;
  oldValue?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<ParcelAuditLogFilterInput>>;
  parcel?: InputMaybe<ParcelFilterInput>;
  parcelId?: InputMaybe<UuidOperationFilterInput>;
  propertyName?: InputMaybe<StringOperationFilterInput>;
};

export type ParcelAuditLogSortInput = {
  changedBy?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  newValue?: InputMaybe<SortEnumType>;
  oldValue?: InputMaybe<SortEnumType>;
  parcel?: InputMaybe<ParcelSortInput>;
  parcelId?: InputMaybe<SortEnumType>;
  propertyName?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type ParcelAuditLogsConnection = {
  __typename?: 'ParcelAuditLogsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<ParcelAuditLogsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<ParcelAuditLog>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
};

/** An edge in a connection. */
export type ParcelAuditLogsEdge = {
  __typename?: 'ParcelAuditLogsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: ParcelAuditLog;
};

export type ParcelContentItem = {
  __typename?: 'ParcelContentItem';
  countryOfOrigin: Scalars['String']['output'];
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  currency: Scalars['String']['output'];
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  description?: Maybe<Scalars['String']['output']>;
  hsCode?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  parcel: Parcel;
  parcelId: Scalars['UUID']['output'];
  quantity: Scalars['Int']['output'];
  unitValue: Scalars['Decimal']['output'];
  weight: Scalars['Decimal']['output'];
  weightUnit: WeightUnit;
};

export type ParcelContentItemFilterInput = {
  and?: InputMaybe<Array<ParcelContentItemFilterInput>>;
  countryOfOrigin?: InputMaybe<StringOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  currency?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  description?: InputMaybe<StringOperationFilterInput>;
  hsCode?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<ParcelContentItemFilterInput>>;
  parcel?: InputMaybe<ParcelFilterInput>;
  parcelId?: InputMaybe<UuidOperationFilterInput>;
  quantity?: InputMaybe<IntOperationFilterInput>;
  unitValue?: InputMaybe<DecimalOperationFilterInput>;
  weight?: InputMaybe<DecimalOperationFilterInput>;
  weightUnit?: InputMaybe<WeightUnitOperationFilterInput>;
};

export type ParcelFilterInput = {
  actualDeliveryDate?: InputMaybe<DateTimeOperationFilterInput>;
  and?: InputMaybe<Array<ParcelFilterInput>>;
  auditLogs?: InputMaybe<ListFilterInputTypeOfParcelAuditLogFilterInput>;
  bin?: InputMaybe<BinFilterInput>;
  binId?: InputMaybe<UuidOperationFilterInput>;
  contentItems?: InputMaybe<ListFilterInputTypeOfParcelContentItemFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  currency?: InputMaybe<StringOperationFilterInput>;
  declaredValue?: InputMaybe<DecimalOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  deliveryAttempts?: InputMaybe<IntOperationFilterInput>;
  deliveryConfirmation?: InputMaybe<DeliveryConfirmationFilterInput>;
  description?: InputMaybe<StringOperationFilterInput>;
  dimensionUnit?: InputMaybe<DimensionUnitOperationFilterInput>;
  estimatedDeliveryDate?: InputMaybe<DateTimeOperationFilterInput>;
  height?: InputMaybe<DecimalOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  length?: InputMaybe<DecimalOperationFilterInput>;
  notes?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<ParcelFilterInput>>;
  parcelType?: InputMaybe<NullableOfParcelTypeOperationFilterInput>;
  parcelWatchers?: InputMaybe<ListFilterInputTypeOfParcelWatcherFilterInput>;
  recipientAddress?: InputMaybe<AddressFilterInput>;
  recipientAddressId?: InputMaybe<UuidOperationFilterInput>;
  routeStop?: InputMaybe<RouteStopFilterInput>;
  routeStopId?: InputMaybe<UuidOperationFilterInput>;
  serviceType?: InputMaybe<ServiceTypeOperationFilterInput>;
  shipperAddress?: InputMaybe<AddressFilterInput>;
  shipperAddressId?: InputMaybe<UuidOperationFilterInput>;
  status?: InputMaybe<ParcelStatusOperationFilterInput>;
  trackingEvents?: InputMaybe<ListFilterInputTypeOfTrackingEventFilterInput>;
  trackingNumber?: InputMaybe<StringOperationFilterInput>;
  weight?: InputMaybe<DecimalOperationFilterInput>;
  weightUnit?: InputMaybe<WeightUnitOperationFilterInput>;
  width?: InputMaybe<DecimalOperationFilterInput>;
  zone?: InputMaybe<ZoneFilterInput>;
  zoneId?: InputMaybe<UuidOperationFilterInput>;
};

export type ParcelSortInput = {
  actualDeliveryDate?: InputMaybe<SortEnumType>;
  bin?: InputMaybe<BinSortInput>;
  binId?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  currency?: InputMaybe<SortEnumType>;
  declaredValue?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  deliveryAttempts?: InputMaybe<SortEnumType>;
  deliveryConfirmation?: InputMaybe<DeliveryConfirmationSortInput>;
  description?: InputMaybe<SortEnumType>;
  dimensionUnit?: InputMaybe<SortEnumType>;
  estimatedDeliveryDate?: InputMaybe<SortEnumType>;
  height?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  length?: InputMaybe<SortEnumType>;
  notes?: InputMaybe<SortEnumType>;
  parcelType?: InputMaybe<SortEnumType>;
  recipientAddress?: InputMaybe<AddressSortInput>;
  recipientAddressId?: InputMaybe<SortEnumType>;
  routeStop?: InputMaybe<RouteStopSortInput>;
  routeStopId?: InputMaybe<SortEnumType>;
  serviceType?: InputMaybe<SortEnumType>;
  shipperAddress?: InputMaybe<AddressSortInput>;
  shipperAddressId?: InputMaybe<SortEnumType>;
  status?: InputMaybe<SortEnumType>;
  trackingNumber?: InputMaybe<SortEnumType>;
  weight?: InputMaybe<SortEnumType>;
  weightUnit?: InputMaybe<SortEnumType>;
  width?: InputMaybe<SortEnumType>;
  zone?: InputMaybe<ZoneSortInput>;
  zoneId?: InputMaybe<SortEnumType>;
};

export enum ParcelStatus {
  Cancelled = 'CANCELLED',
  Delivered = 'DELIVERED',
  Exception = 'EXCEPTION',
  FailedAttempt = 'FAILED_ATTEMPT',
  Loaded = 'LOADED',
  OutForDelivery = 'OUT_FOR_DELIVERY',
  ReceivedAtDepot = 'RECEIVED_AT_DEPOT',
  Registered = 'REGISTERED',
  ReturnedToDepot = 'RETURNED_TO_DEPOT',
  Sorted = 'SORTED',
  Staged = 'STAGED'
}

export type ParcelStatusOperationFilterInput = {
  eq?: InputMaybe<ParcelStatus>;
  in?: InputMaybe<Array<ParcelStatus>>;
  neq?: InputMaybe<ParcelStatus>;
  nin?: InputMaybe<Array<ParcelStatus>>;
};

export enum ParcelType {
  Bulk = 'BULK',
  Envelope = 'ENVELOPE',
  Package = 'PACKAGE',
  Pallet = 'PALLET'
}

export type ParcelWatcher = {
  __typename?: 'ParcelWatcher';
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  email: Scalars['String']['output'];
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  name?: Maybe<Scalars['String']['output']>;
  parcels: Array<Parcel>;
};

export type ParcelWatcherFilterInput = {
  and?: InputMaybe<Array<ParcelWatcherFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  email?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<ParcelWatcherFilterInput>>;
  parcels?: InputMaybe<ListFilterInputTypeOfParcelFilterInput>;
};

/** A connection to a list of items. */
export type ParcelsConnection = {
  __typename?: 'ParcelsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<ParcelsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Parcel>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type ParcelsEdge = {
  __typename?: 'ParcelsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Parcel;
};

export type Permission = {
  __typename?: 'Permission';
  code: Scalars['String']['output'];
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  description?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  module: PermissionModule;
  name: Scalars['String']['output'];
  rolePermissions: Array<RolePermission>;
  scope: PermissionScope;
};

export type PermissionFilterInput = {
  and?: InputMaybe<Array<PermissionFilterInput>>;
  code?: InputMaybe<StringOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  description?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  module?: InputMaybe<PermissionModuleOperationFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<PermissionFilterInput>>;
  rolePermissions?: InputMaybe<ListFilterInputTypeOfRolePermissionFilterInput>;
  scope?: InputMaybe<PermissionScopeOperationFilterInput>;
};

export enum PermissionModule {
  Depots = 'DEPOTS',
  Parcels = 'PARCELS',
  Reports = 'REPORTS',
  Roles = 'ROLES',
  Settings = 'SETTINGS',
  Users = 'USERS',
  Vehicles = 'VEHICLES',
  Zones = 'ZONES'
}

export type PermissionModuleOperationFilterInput = {
  eq?: InputMaybe<PermissionModule>;
  in?: InputMaybe<Array<PermissionModule>>;
  neq?: InputMaybe<PermissionModule>;
  nin?: InputMaybe<Array<PermissionModule>>;
};

export enum PermissionScope {
  All = 'ALL',
  Delete = 'DELETE',
  Read = 'READ',
  Write = 'WRITE'
}

export type PermissionScopeOperationFilterInput = {
  eq?: InputMaybe<PermissionScope>;
  in?: InputMaybe<Array<PermissionScope>>;
  neq?: InputMaybe<PermissionScope>;
  nin?: InputMaybe<Array<PermissionScope>>;
};

export type PointFilterInput = {
  and?: InputMaybe<Array<PointFilterInput>>;
  area?: InputMaybe<FloatOperationFilterInput>;
  boundary?: InputMaybe<GeometryFilterInput>;
  boundaryDimension?: InputMaybe<DimensionOperationFilterInput>;
  centroid?: InputMaybe<PointFilterInput>;
  coordinate?: InputMaybe<CoordinateFilterInput>;
  coordinateSequence?: InputMaybe<CoordinateSequenceFilterInput>;
  coordinates?: InputMaybe<ListFilterInputTypeOfCoordinateFilterInput>;
  dimension?: InputMaybe<DimensionOperationFilterInput>;
  envelope?: InputMaybe<GeometryFilterInput>;
  envelopeInternal?: InputMaybe<EnvelopeFilterInput>;
  factory?: InputMaybe<GeometryFactoryFilterInput>;
  geometryType?: InputMaybe<StringOperationFilterInput>;
  interiorPoint?: InputMaybe<PointFilterInput>;
  isEmpty?: InputMaybe<BooleanOperationFilterInput>;
  isRectangle?: InputMaybe<BooleanOperationFilterInput>;
  isSimple?: InputMaybe<BooleanOperationFilterInput>;
  isValid?: InputMaybe<BooleanOperationFilterInput>;
  length?: InputMaybe<FloatOperationFilterInput>;
  m?: InputMaybe<FloatOperationFilterInput>;
  numGeometries?: InputMaybe<IntOperationFilterInput>;
  numPoints?: InputMaybe<IntOperationFilterInput>;
  ogcGeometryType?: InputMaybe<OgcGeometryTypeOperationFilterInput>;
  or?: InputMaybe<Array<PointFilterInput>>;
  pointOnSurface?: InputMaybe<PointFilterInput>;
  precisionModel?: InputMaybe<PrecisionModelFilterInput>;
  srid?: InputMaybe<IntOperationFilterInput>;
  x?: InputMaybe<FloatOperationFilterInput>;
  y?: InputMaybe<FloatOperationFilterInput>;
  z?: InputMaybe<FloatOperationFilterInput>;
};

export type PointSortInput = {
  area?: InputMaybe<SortEnumType>;
  boundary?: InputMaybe<GeometrySortInput>;
  boundaryDimension?: InputMaybe<SortEnumType>;
  centroid?: InputMaybe<PointSortInput>;
  coordinate?: InputMaybe<CoordinateSortInput>;
  coordinateSequence?: InputMaybe<CoordinateSequenceSortInput>;
  dimension?: InputMaybe<SortEnumType>;
  envelope?: InputMaybe<GeometrySortInput>;
  envelopeInternal?: InputMaybe<EnvelopeSortInput>;
  factory?: InputMaybe<GeometryFactorySortInput>;
  geometryType?: InputMaybe<SortEnumType>;
  interiorPoint?: InputMaybe<PointSortInput>;
  isEmpty?: InputMaybe<SortEnumType>;
  isRectangle?: InputMaybe<SortEnumType>;
  isSimple?: InputMaybe<SortEnumType>;
  isValid?: InputMaybe<SortEnumType>;
  length?: InputMaybe<SortEnumType>;
  m?: InputMaybe<SortEnumType>;
  numGeometries?: InputMaybe<SortEnumType>;
  numPoints?: InputMaybe<SortEnumType>;
  ogcGeometryType?: InputMaybe<SortEnumType>;
  pointOnSurface?: InputMaybe<PointSortInput>;
  precisionModel?: InputMaybe<PrecisionModelSortInput>;
  srid?: InputMaybe<SortEnumType>;
  x?: InputMaybe<SortEnumType>;
  y?: InputMaybe<SortEnumType>;
  z?: InputMaybe<SortEnumType>;
};

export type PrecisionModelFilterInput = {
  and?: InputMaybe<Array<PrecisionModelFilterInput>>;
  gridSize?: InputMaybe<FloatOperationFilterInput>;
  isFloating?: InputMaybe<BooleanOperationFilterInput>;
  maximumSignificantDigits?: InputMaybe<IntOperationFilterInput>;
  or?: InputMaybe<Array<PrecisionModelFilterInput>>;
  precisionModelType?: InputMaybe<PrecisionModelsOperationFilterInput>;
  scale?: InputMaybe<FloatOperationFilterInput>;
};

export type PrecisionModelSortInput = {
  gridSize?: InputMaybe<SortEnumType>;
  isFloating?: InputMaybe<SortEnumType>;
  maximumSignificantDigits?: InputMaybe<SortEnumType>;
  precisionModelType?: InputMaybe<SortEnumType>;
  scale?: InputMaybe<SortEnumType>;
};

export enum PrecisionModels {
  Fixed = 'FIXED',
  Floating = 'FLOATING',
  FloatingSingle = 'FLOATING_SINGLE'
}

export type PrecisionModelsOperationFilterInput = {
  eq?: InputMaybe<PrecisionModels>;
  in?: InputMaybe<Array<PrecisionModels>>;
  neq?: InputMaybe<PrecisionModels>;
  nin?: InputMaybe<Array<PrecisionModels>>;
};

export type Query = {
  __typename?: 'Query';
  aisle?: Maybe<Aisle>;
  aisles?: Maybe<AislesConnection>;
  aislesByZone: Array<Aisle>;
  availableDrivers: Array<AvailableDriverDto>;
  bin?: Maybe<Bin>;
  binUtilizations: Array<BinDto>;
  bins?: Maybe<BinsConnection>;
  depot?: Maybe<Depot>;
  depots?: Maybe<DepotsConnection>;
  driver?: Maybe<Driver>;
  drivers?: Maybe<DriversConnection>;
  parcel?: Maybe<Parcel>;
  parcelAuditLogs?: Maybe<ParcelAuditLogsConnection>;
  parcelByTrackingNumber?: Maybe<Parcel>;
  parcels?: Maybe<ParcelsConnection>;
  route?: Maybe<Route>;
  routes?: Maybe<RoutesConnection>;
  sentinel?: Maybe<Scalars['String']['output']>;
  trackingEvents?: Maybe<TrackingEventsConnection>;
  user?: Maybe<User>;
  userManagementLookups: UserManagementLookupsDto;
  users?: Maybe<UsersConnection>;
  vehicle?: Maybe<Vehicle>;
  vehicleHistory?: Maybe<VehicleHistoryDto>;
  vehicles: Array<Vehicle>;
  zone?: Maybe<Zone>;
  zones?: Maybe<ZonesConnection>;
};


export type QueryAisleArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryAislesArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<AisleSortInput>>;
  where?: InputMaybe<AisleFilterInput>;
};


export type QueryAislesByZoneArgs = {
  order?: InputMaybe<Array<AisleSortInput>>;
  zoneId: Scalars['UUID']['input'];
};


export type QueryAvailableDriversArgs = {
  date: Scalars['DateTime']['input'];
};


export type QueryBinArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryBinUtilizationsArgs = {
  zoneId: Scalars['UUID']['input'];
};


export type QueryBinsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<BinSortInput>>;
  where?: InputMaybe<BinFilterInput>;
};


export type QueryDepotArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryDepotsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<DepotSortInput>>;
  where?: InputMaybe<DepotFilterInput>;
};


export type QueryDriverArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryDriversArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<DriverSortInput>>;
  where?: InputMaybe<DriverFilterInput>;
};


export type QueryParcelArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryParcelAuditLogsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<ParcelAuditLogSortInput>>;
  parcelId: Scalars['UUID']['input'];
  where?: InputMaybe<ParcelAuditLogFilterInput>;
};


export type QueryParcelByTrackingNumberArgs = {
  trackingNumber: Scalars['String']['input'];
};


export type QueryParcelsArgs = {
  addressSearch?: InputMaybe<Scalars['String']['input']>;
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<ParcelSortInput>>;
  recipientSearch?: InputMaybe<Scalars['String']['input']>;
  where?: InputMaybe<ParcelFilterInput>;
};


export type QueryRouteArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryRoutesArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<RouteSortInput>>;
  where?: InputMaybe<RouteFilterInput>;
};


export type QueryTrackingEventsArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<TrackingEventSortInput>>;
  parcelId: Scalars['UUID']['input'];
  where?: InputMaybe<TrackingEventFilterInput>;
};


export type QueryUserArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryUsersArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<UserSortInput>>;
  where?: InputMaybe<UserFilterInput>;
};


export type QueryVehicleArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryVehicleHistoryArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryVehiclesArgs = {
  order?: InputMaybe<Array<VehicleSortInput>>;
  where?: InputMaybe<VehicleFilterInput>;
};


export type QueryZoneArgs = {
  id: Scalars['UUID']['input'];
};


export type QueryZonesArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
  order?: InputMaybe<Array<ZoneSortInput>>;
  where?: InputMaybe<ZoneFilterInput>;
};

export type RemoveParcelsFromRouteCommandInput = {
  parcelIds: Array<Scalars['UUID']['input']>;
  routeId: Scalars['UUID']['input'];
};

export type ReorderRouteStopsCommandInput = {
  routeId: Scalars['UUID']['input'];
  stopIdsInOrder: Array<Scalars['UUID']['input']>;
};

export type Role = {
  __typename?: 'Role';
  concurrencyStamp?: Maybe<Scalars['String']['output']>;
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  description?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  name?: Maybe<Scalars['String']['output']>;
  normalizedName?: Maybe<Scalars['String']['output']>;
  rolePermissions: Array<RolePermission>;
  users: Array<User>;
};

export type RoleFilterInput = {
  and?: InputMaybe<Array<RoleFilterInput>>;
  concurrencyStamp?: InputMaybe<StringOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  description?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  normalizedName?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<RoleFilterInput>>;
  rolePermissions?: InputMaybe<ListFilterInputTypeOfRolePermissionFilterInput>;
  users?: InputMaybe<ListFilterInputTypeOfUserFilterInput>;
};

export type RoleLookupDto = {
  __typename?: 'RoleLookupDto';
  description?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  name: Scalars['String']['output'];
};

export type RolePermission = {
  __typename?: 'RolePermission';
  dataScope: DataScope;
  permission: Permission;
  permissionId: Scalars['UUID']['output'];
  role: Role;
  roleId: Scalars['UUID']['output'];
};

export type RolePermissionFilterInput = {
  and?: InputMaybe<Array<RolePermissionFilterInput>>;
  dataScope?: InputMaybe<DataScopeOperationFilterInput>;
  or?: InputMaybe<Array<RolePermissionFilterInput>>;
  permission?: InputMaybe<PermissionFilterInput>;
  permissionId?: InputMaybe<UuidOperationFilterInput>;
  role?: InputMaybe<RoleFilterInput>;
  roleId?: InputMaybe<UuidOperationFilterInput>;
};

export type RoleSortInput = {
  concurrencyStamp?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  description?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
  normalizedName?: InputMaybe<SortEnumType>;
};

export type Route = {
  __typename?: 'Route';
  actualEndTime?: Maybe<Scalars['DateTime']['output']>;
  actualStartTime?: Maybe<Scalars['DateTime']['output']>;
  canTransitionTo: Scalars['Boolean']['output'];
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  driver?: Maybe<Driver>;
  driverId?: Maybe<Scalars['UUID']['output']>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  name: Scalars['String']['output'];
  plannedStartTime: Scalars['DateTime']['output'];
  routeStops: Array<RouteStop>;
  status: RouteStatus;
  totalDistanceKm: Scalars['Decimal']['output'];
  totalParcelCount: Scalars['Int']['output'];
  vehicle?: Maybe<Vehicle>;
  vehicleId?: Maybe<Scalars['UUID']['output']>;
  vehicleJourneys: Array<VehicleJourney>;
  zone?: Maybe<Zone>;
  zoneId?: Maybe<Scalars['UUID']['output']>;
};


export type RouteCanTransitionToArgs = {
  newStatus: RouteStatus;
};

export type RouteDto = {
  __typename?: 'RouteDto';
  actualEndTime?: Maybe<Scalars['DateTime']['output']>;
  actualStartTime?: Maybe<Scalars['DateTime']['output']>;
  createdAt: Scalars['DateTime']['output'];
  driverId?: Maybe<Scalars['UUID']['output']>;
  driverName?: Maybe<Scalars['String']['output']>;
  estimatedStopCount: Scalars['Int']['output'];
  id: Scalars['UUID']['output'];
  name: Scalars['String']['output'];
  plannedStartTime: Scalars['DateTime']['output'];
  status: RouteStatus;
  stops: Array<RouteStopDto>;
  totalDistanceKm: Scalars['Decimal']['output'];
  totalParcelCount: Scalars['Int']['output'];
  vehicleId?: Maybe<Scalars['UUID']['output']>;
  vehiclePlate?: Maybe<Scalars['String']['output']>;
  zoneId?: Maybe<Scalars['UUID']['output']>;
  zoneName?: Maybe<Scalars['String']['output']>;
};

export type RouteFilterInput = {
  actualEndTime?: InputMaybe<DateTimeOperationFilterInput>;
  actualStartTime?: InputMaybe<DateTimeOperationFilterInput>;
  and?: InputMaybe<Array<RouteFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  driver?: InputMaybe<DriverFilterInput>;
  driverId?: InputMaybe<UuidOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<RouteFilterInput>>;
  plannedStartTime?: InputMaybe<DateTimeOperationFilterInput>;
  routeStops?: InputMaybe<ListFilterInputTypeOfRouteStopFilterInput>;
  status?: InputMaybe<RouteStatusOperationFilterInput>;
  totalDistanceKm?: InputMaybe<DecimalOperationFilterInput>;
  totalParcelCount?: InputMaybe<IntOperationFilterInput>;
  vehicle?: InputMaybe<VehicleFilterInput>;
  vehicleId?: InputMaybe<UuidOperationFilterInput>;
  vehicleJourneys?: InputMaybe<ListFilterInputTypeOfVehicleJourneyFilterInput>;
  zone?: InputMaybe<ZoneFilterInput>;
  zoneId?: InputMaybe<UuidOperationFilterInput>;
};

export type RouteHistoryDto = {
  __typename?: 'RouteHistoryDto';
  completedAt: Scalars['DateTime']['output'];
  distanceKm: Scalars['Decimal']['output'];
  parcelCount: Scalars['Int']['output'];
  routeId: Scalars['UUID']['output'];
  routeName: Scalars['String']['output'];
};

export type RouteSortInput = {
  actualEndTime?: InputMaybe<SortEnumType>;
  actualStartTime?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  driver?: InputMaybe<DriverSortInput>;
  driverId?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
  plannedStartTime?: InputMaybe<SortEnumType>;
  status?: InputMaybe<SortEnumType>;
  totalDistanceKm?: InputMaybe<SortEnumType>;
  totalParcelCount?: InputMaybe<SortEnumType>;
  vehicle?: InputMaybe<VehicleSortInput>;
  vehicleId?: InputMaybe<SortEnumType>;
  zone?: InputMaybe<ZoneSortInput>;
  zoneId?: InputMaybe<SortEnumType>;
};

export enum RouteStatus {
  Completed = 'COMPLETED',
  Draft = 'DRAFT',
  InProgress = 'IN_PROGRESS'
}

export type RouteStatusOperationFilterInput = {
  eq?: InputMaybe<RouteStatus>;
  in?: InputMaybe<Array<RouteStatus>>;
  neq?: InputMaybe<RouteStatus>;
  nin?: InputMaybe<Array<RouteStatus>>;
};

export type RouteStop = {
  __typename?: 'RouteStop';
  accessInstructions?: Maybe<Scalars['String']['output']>;
  arrivalTime?: Maybe<Scalars['DateTime']['output']>;
  canTransitionTo: Scalars['Boolean']['output'];
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  departureTime?: Maybe<Scalars['DateTime']['output']>;
  estimatedServiceMinutes: Scalars['Int']['output'];
  geoLocation?: Maybe<GeoJsonPointType>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  parcels: Array<Parcel>;
  route?: Maybe<Route>;
  routeId: Scalars['UUID']['output'];
  sequenceNumber: Scalars['Int']['output'];
  status: RouteStopStatus;
  street1: Scalars['String']['output'];
};


export type RouteStopCanTransitionToArgs = {
  newStatus: RouteStopStatus;
};

export type RouteStopDto = {
  __typename?: 'RouteStopDto';
  accessInstructions?: Maybe<Scalars['String']['output']>;
  arrivalTime?: Maybe<Scalars['DateTime']['output']>;
  departureTime?: Maybe<Scalars['DateTime']['output']>;
  estimatedServiceMinutes: Scalars['Int']['output'];
  id: Scalars['UUID']['output'];
  parcelCount: Scalars['Int']['output'];
  parcels: Array<RouteStopParcelDto>;
  sequenceNumber: Scalars['Int']['output'];
  status: RouteStopStatus;
  street1: Scalars['String']['output'];
};

export type RouteStopFilterInput = {
  accessInstructions?: InputMaybe<StringOperationFilterInput>;
  and?: InputMaybe<Array<RouteStopFilterInput>>;
  arrivalTime?: InputMaybe<DateTimeOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  departureTime?: InputMaybe<DateTimeOperationFilterInput>;
  estimatedServiceMinutes?: InputMaybe<IntOperationFilterInput>;
  geoLocation?: InputMaybe<PointFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<RouteStopFilterInput>>;
  parcels?: InputMaybe<ListFilterInputTypeOfParcelFilterInput>;
  route?: InputMaybe<RouteFilterInput>;
  routeId?: InputMaybe<UuidOperationFilterInput>;
  sequenceNumber?: InputMaybe<IntOperationFilterInput>;
  status?: InputMaybe<RouteStopStatusOperationFilterInput>;
  street1?: InputMaybe<StringOperationFilterInput>;
};

export type RouteStopParcelDto = {
  __typename?: 'RouteStopParcelDto';
  id: Scalars['UUID']['output'];
  status: ParcelStatus;
  trackingNumber: Scalars['String']['output'];
};

export type RouteStopSortInput = {
  accessInstructions?: InputMaybe<SortEnumType>;
  arrivalTime?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  departureTime?: InputMaybe<SortEnumType>;
  estimatedServiceMinutes?: InputMaybe<SortEnumType>;
  geoLocation?: InputMaybe<PointSortInput>;
  id?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  route?: InputMaybe<RouteSortInput>;
  routeId?: InputMaybe<SortEnumType>;
  sequenceNumber?: InputMaybe<SortEnumType>;
  status?: InputMaybe<SortEnumType>;
  street1?: InputMaybe<SortEnumType>;
};

export enum RouteStopStatus {
  Arrived = 'ARRIVED',
  Completed = 'COMPLETED',
  Pending = 'PENDING',
  Skipped = 'SKIPPED'
}

export type RouteStopStatusOperationFilterInput = {
  eq?: InputMaybe<RouteStopStatus>;
  in?: InputMaybe<Array<RouteStopStatus>>;
  neq?: InputMaybe<RouteStopStatus>;
  nin?: InputMaybe<Array<RouteStopStatus>>;
};

/** A connection to a list of items. */
export type RoutesConnection = {
  __typename?: 'RoutesConnection';
  /** A list of edges. */
  edges?: Maybe<Array<RoutesEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Route>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type RoutesEdge = {
  __typename?: 'RoutesEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Route;
};

export enum ServiceType {
  Economy = 'ECONOMY',
  Express = 'EXPRESS',
  Overnight = 'OVERNIGHT',
  Standard = 'STANDARD'
}

export type ServiceTypeOperationFilterInput = {
  eq?: InputMaybe<ServiceType>;
  in?: InputMaybe<Array<ServiceType>>;
  neq?: InputMaybe<ServiceType>;
  nin?: InputMaybe<Array<ServiceType>>;
};

export type ShiftInfoDto = {
  __typename?: 'ShiftInfoDto';
  closeTime: Scalars['LocalTime']['output'];
  openTime: Scalars['LocalTime']['output'];
};

export type ShiftSchedule = {
  __typename?: 'ShiftSchedule';
  closeTime: Scalars['LocalTime']['output'];
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  dayOfWeek: DayOfWeek;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  depot: Depot;
  depotId?: Maybe<Scalars['UUID']['output']>;
  driver: Driver;
  driverId?: Maybe<Scalars['UUID']['output']>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  openTime: Scalars['LocalTime']['output'];
};

export type ShiftScheduleFilterInput = {
  and?: InputMaybe<Array<ShiftScheduleFilterInput>>;
  closeTime?: InputMaybe<LocalTimeOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  dayOfWeek?: InputMaybe<DayOfWeekOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  depot?: InputMaybe<DepotFilterInput>;
  depotId?: InputMaybe<UuidOperationFilterInput>;
  driver?: InputMaybe<DriverFilterInput>;
  driverId?: InputMaybe<UuidOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  openTime?: InputMaybe<LocalTimeOperationFilterInput>;
  or?: InputMaybe<Array<ShiftScheduleFilterInput>>;
};

export type ShiftScheduleInput = {
  closeTime?: InputMaybe<Scalars['LocalTime']['input']>;
  dayOfWeek?: InputMaybe<DayOfWeek>;
  openTime?: InputMaybe<Scalars['LocalTime']['input']>;
};

export type ShiftScheduleResult = {
  __typename?: 'ShiftScheduleResult';
  closeTime: Scalars['LocalTime']['output'];
  dayOfWeek: DayOfWeek;
  openTime: Scalars['LocalTime']['output'];
};

export enum SortEnumType {
  Asc = 'ASC',
  Desc = 'DESC'
}

export type StringOperationFilterInput = {
  and?: InputMaybe<Array<StringOperationFilterInput>>;
  contains?: InputMaybe<Scalars['String']['input']>;
  endsWith?: InputMaybe<Scalars['String']['input']>;
  eq?: InputMaybe<Scalars['String']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['String']['input']>>>;
  ncontains?: InputMaybe<Scalars['String']['input']>;
  nendsWith?: InputMaybe<Scalars['String']['input']>;
  neq?: InputMaybe<Scalars['String']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['String']['input']>>>;
  nstartsWith?: InputMaybe<Scalars['String']['input']>;
  or?: InputMaybe<Array<StringOperationFilterInput>>;
  startsWith?: InputMaybe<Scalars['String']['input']>;
};

export type TrackingEvent = {
  __typename?: 'TrackingEvent';
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  delayReason?: Maybe<ExceptionReason>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  description?: Maybe<Scalars['String']['output']>;
  eventType: EventType;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  locationCity?: Maybe<Scalars['String']['output']>;
  locationCountry?: Maybe<Scalars['String']['output']>;
  locationState?: Maybe<Scalars['String']['output']>;
  operator?: Maybe<Scalars['String']['output']>;
  parcel: Parcel;
  parcelId: Scalars['UUID']['output'];
  timestamp: Scalars['DateTime']['output'];
};

export type TrackingEventFilterInput = {
  and?: InputMaybe<Array<TrackingEventFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  delayReason?: InputMaybe<NullableOfExceptionReasonOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  description?: InputMaybe<StringOperationFilterInput>;
  eventType?: InputMaybe<EventTypeOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  locationCity?: InputMaybe<StringOperationFilterInput>;
  locationCountry?: InputMaybe<StringOperationFilterInput>;
  locationState?: InputMaybe<StringOperationFilterInput>;
  operator?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<TrackingEventFilterInput>>;
  parcel?: InputMaybe<ParcelFilterInput>;
  parcelId?: InputMaybe<UuidOperationFilterInput>;
  timestamp?: InputMaybe<DateTimeOperationFilterInput>;
};

export type TrackingEventSortInput = {
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  delayReason?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  description?: InputMaybe<SortEnumType>;
  eventType?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  locationCity?: InputMaybe<SortEnumType>;
  locationCountry?: InputMaybe<SortEnumType>;
  locationState?: InputMaybe<SortEnumType>;
  operator?: InputMaybe<SortEnumType>;
  parcel?: InputMaybe<ParcelSortInput>;
  parcelId?: InputMaybe<SortEnumType>;
  timestamp?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type TrackingEventsConnection = {
  __typename?: 'TrackingEventsConnection';
  /** A list of edges. */
  edges?: Maybe<Array<TrackingEventsEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<TrackingEvent>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type TrackingEventsEdge = {
  __typename?: 'TrackingEventsEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: TrackingEvent;
};

export type UpdateAddressInput = {
  city: Scalars['String']['input'];
  companyName?: InputMaybe<Scalars['String']['input']>;
  contactName?: InputMaybe<Scalars['String']['input']>;
  countryCode?: Scalars['String']['input'];
  email?: InputMaybe<Scalars['String']['input']>;
  isResidential?: Scalars['Boolean']['input'];
  phone?: InputMaybe<Scalars['String']['input']>;
  postalCode: Scalars['String']['input'];
  state: Scalars['String']['input'];
  street1: Scalars['String']['input'];
  street2?: InputMaybe<Scalars['String']['input']>;
};

export type UpdateAisleCommandInput = {
  id: Scalars['UUID']['input'];
  isActive: Scalars['Boolean']['input'];
  name: Scalars['String']['input'];
  order: Scalars['Int']['input'];
  zoneId: Scalars['UUID']['input'];
};

export type UpdateBinCommandInput = {
  aisleId: Scalars['UUID']['input'];
  capacity: Scalars['Int']['input'];
  description?: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  isActive: Scalars['Boolean']['input'];
  slot: Scalars['Int']['input'];
  zoneId: Scalars['UUID']['input'];
};

export type UpdateDepotCommandInput = {
  address: UpdateAddressInput;
  id: Scalars['UUID']['input'];
  isActive: Scalars['Boolean']['input'];
  name: Scalars['String']['input'];
  operatingHours?: InputMaybe<Array<InputMaybe<DailyOperatingHoursInput>>>;
};

export type UpdateDriverCommandInput = {
  daysOff?: InputMaybe<Array<InputMaybe<DayOffInput>>>;
  id: Scalars['UUID']['input'];
  licenseExpiryDate: Scalars['DateTime']['input'];
  licenseNumber: Scalars['String']['input'];
  photo?: InputMaybe<Scalars['String']['input']>;
  shiftSchedules?: InputMaybe<Array<InputMaybe<ShiftScheduleInput>>>;
};

export type UpdateParcelCommandInput = {
  description?: InputMaybe<Scalars['String']['input']>;
  height?: InputMaybe<Scalars['Decimal']['input']>;
  id: Scalars['UUID']['input'];
  length?: InputMaybe<Scalars['Decimal']['input']>;
  parcelType?: InputMaybe<ParcelType>;
  recipientAddress?: InputMaybe<AddressInput>;
  serviceType?: InputMaybe<ServiceType>;
  shipperAddress?: InputMaybe<AddressInput>;
  weight?: InputMaybe<Scalars['Decimal']['input']>;
  width?: InputMaybe<Scalars['Decimal']['input']>;
};

export type UpdateParcelResult = {
  __typename?: 'UpdateParcelResult';
  description?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  lastModifiedAt: Scalars['DateTime']['output'];
  recipientAddress?: Maybe<AddressResult>;
  shipperAddress?: Maybe<AddressResult>;
  status: ParcelStatus;
  trackingNumber: Scalars['String']['output'];
  weight: Scalars['Decimal']['output'];
};

export type UpdateRouteCommandInput = {
  driverId?: InputMaybe<Scalars['UUID']['input']>;
  id: Scalars['UUID']['input'];
  name: Scalars['String']['input'];
  plannedStartTime: Scalars['DateTime']['input'];
  vehicleId?: InputMaybe<Scalars['UUID']['input']>;
  zoneId?: InputMaybe<Scalars['UUID']['input']>;
};

export type UpdateZoneCommandInput = {
  depotId: Scalars['UUID']['input'];
  geoJson?: InputMaybe<Scalars['String']['input']>;
  id: Scalars['UUID']['input'];
  isActive: Scalars['Boolean']['input'];
  name: Scalars['String']['input'];
};

export type User = {
  __typename?: 'User';
  accessFailedCount: Scalars['Int']['output'];
  concurrencyStamp?: Maybe<Scalars['String']['output']>;
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  depot?: Maybe<Depot>;
  depotId?: Maybe<Scalars['UUID']['output']>;
  email?: Maybe<Scalars['String']['output']>;
  emailConfirmed: Scalars['Boolean']['output'];
  firstName: Scalars['String']['output'];
  id: Scalars['UUID']['output'];
  isActive: Scalars['Boolean']['output'];
  isDeleted: Scalars['Boolean']['output'];
  isSystemAdmin: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  lastName: Scalars['String']['output'];
  lockoutEnabled: Scalars['Boolean']['output'];
  lockoutEnd?: Maybe<Scalars['DateTime']['output']>;
  normalizedEmail?: Maybe<Scalars['String']['output']>;
  normalizedUserName?: Maybe<Scalars['String']['output']>;
  passwordHash?: Maybe<Scalars['String']['output']>;
  phone?: Maybe<Scalars['String']['output']>;
  phoneNumber?: Maybe<Scalars['String']['output']>;
  phoneNumberConfirmed: Scalars['Boolean']['output'];
  role?: Maybe<Role>;
  roleId?: Maybe<Scalars['UUID']['output']>;
  securityStamp?: Maybe<Scalars['String']['output']>;
  status: UserStatus;
  twoFactorEnabled: Scalars['Boolean']['output'];
  userName?: Maybe<Scalars['String']['output']>;
  zone?: Maybe<Zone>;
  zoneId?: Maybe<Scalars['UUID']['output']>;
};

export type UserDto = {
  __typename?: 'UserDto';
  createdAt: Scalars['DateTime']['output'];
  depotId?: Maybe<Scalars['UUID']['output']>;
  depotName?: Maybe<Scalars['String']['output']>;
  email: Scalars['String']['output'];
  firstName: Scalars['String']['output'];
  id: Scalars['UUID']['output'];
  lastName: Scalars['String']['output'];
  phoneNumber?: Maybe<Scalars['String']['output']>;
  roleId?: Maybe<Scalars['UUID']['output']>;
  roleName?: Maybe<Scalars['String']['output']>;
  status: UserStatus;
  zoneId?: Maybe<Scalars['UUID']['output']>;
  zoneName?: Maybe<Scalars['String']['output']>;
};

export type UserFilterInput = {
  accessFailedCount?: InputMaybe<IntOperationFilterInput>;
  and?: InputMaybe<Array<UserFilterInput>>;
  concurrencyStamp?: InputMaybe<StringOperationFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  depot?: InputMaybe<DepotFilterInput>;
  depotId?: InputMaybe<UuidOperationFilterInput>;
  email?: InputMaybe<StringOperationFilterInput>;
  emailConfirmed?: InputMaybe<BooleanOperationFilterInput>;
  firstName?: InputMaybe<StringOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isActive?: InputMaybe<BooleanOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  isSystemAdmin?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  lastName?: InputMaybe<StringOperationFilterInput>;
  lockoutEnabled?: InputMaybe<BooleanOperationFilterInput>;
  lockoutEnd?: InputMaybe<DateTimeOperationFilterInput>;
  normalizedEmail?: InputMaybe<StringOperationFilterInput>;
  normalizedUserName?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<UserFilterInput>>;
  passwordHash?: InputMaybe<StringOperationFilterInput>;
  phone?: InputMaybe<StringOperationFilterInput>;
  phoneNumber?: InputMaybe<StringOperationFilterInput>;
  phoneNumberConfirmed?: InputMaybe<BooleanOperationFilterInput>;
  role?: InputMaybe<RoleFilterInput>;
  roleId?: InputMaybe<UuidOperationFilterInput>;
  securityStamp?: InputMaybe<StringOperationFilterInput>;
  status?: InputMaybe<UserStatusOperationFilterInput>;
  twoFactorEnabled?: InputMaybe<BooleanOperationFilterInput>;
  userName?: InputMaybe<StringOperationFilterInput>;
  zone?: InputMaybe<ZoneFilterInput>;
  zoneId?: InputMaybe<UuidOperationFilterInput>;
};

export type UserManagementLookupsDto = {
  __typename?: 'UserManagementLookupsDto';
  depots: Array<DepotLookupDto>;
  roles: Array<RoleLookupDto>;
  zones: Array<ZoneLookupDto>;
};

export type UserSortInput = {
  accessFailedCount?: InputMaybe<SortEnumType>;
  concurrencyStamp?: InputMaybe<SortEnumType>;
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  depot?: InputMaybe<DepotSortInput>;
  depotId?: InputMaybe<SortEnumType>;
  email?: InputMaybe<SortEnumType>;
  emailConfirmed?: InputMaybe<SortEnumType>;
  firstName?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isActive?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  isSystemAdmin?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  lastName?: InputMaybe<SortEnumType>;
  lockoutEnabled?: InputMaybe<SortEnumType>;
  lockoutEnd?: InputMaybe<SortEnumType>;
  normalizedEmail?: InputMaybe<SortEnumType>;
  normalizedUserName?: InputMaybe<SortEnumType>;
  passwordHash?: InputMaybe<SortEnumType>;
  phone?: InputMaybe<SortEnumType>;
  phoneNumber?: InputMaybe<SortEnumType>;
  phoneNumberConfirmed?: InputMaybe<SortEnumType>;
  role?: InputMaybe<RoleSortInput>;
  roleId?: InputMaybe<SortEnumType>;
  securityStamp?: InputMaybe<SortEnumType>;
  status?: InputMaybe<SortEnumType>;
  twoFactorEnabled?: InputMaybe<SortEnumType>;
  userName?: InputMaybe<SortEnumType>;
  zone?: InputMaybe<ZoneSortInput>;
  zoneId?: InputMaybe<SortEnumType>;
};

export enum UserStatus {
  Active = 'ACTIVE',
  Inactive = 'INACTIVE',
  Suspended = 'SUSPENDED'
}

export type UserStatusOperationFilterInput = {
  eq?: InputMaybe<UserStatus>;
  in?: InputMaybe<Array<UserStatus>>;
  neq?: InputMaybe<UserStatus>;
  nin?: InputMaybe<Array<UserStatus>>;
};

/** A connection to a list of items. */
export type UsersConnection = {
  __typename?: 'UsersConnection';
  /** A list of edges. */
  edges?: Maybe<Array<UsersEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<User>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type UsersEdge = {
  __typename?: 'UsersEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: User;
};

export type UuidOperationFilterInput = {
  eq?: InputMaybe<Scalars['UUID']['input']>;
  gt?: InputMaybe<Scalars['UUID']['input']>;
  gte?: InputMaybe<Scalars['UUID']['input']>;
  in?: InputMaybe<Array<InputMaybe<Scalars['UUID']['input']>>>;
  lt?: InputMaybe<Scalars['UUID']['input']>;
  lte?: InputMaybe<Scalars['UUID']['input']>;
  neq?: InputMaybe<Scalars['UUID']['input']>;
  ngt?: InputMaybe<Scalars['UUID']['input']>;
  ngte?: InputMaybe<Scalars['UUID']['input']>;
  nin?: InputMaybe<Array<InputMaybe<Scalars['UUID']['input']>>>;
  nlt?: InputMaybe<Scalars['UUID']['input']>;
  nlte?: InputMaybe<Scalars['UUID']['input']>;
};

export type Vehicle = {
  __typename?: 'Vehicle';
  canTransitionTo: Scalars['Boolean']['output'];
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  depot?: Maybe<Depot>;
  depotId?: Maybe<Scalars['UUID']['output']>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  parcelCapacity: Scalars['Int']['output'];
  registrationPlate: Scalars['String']['output'];
  status: VehicleStatus;
  type: VehicleType;
  weightCapacityKg: Scalars['Decimal']['output'];
};


export type VehicleCanTransitionToArgs = {
  newStatus: VehicleStatus;
};

export type VehicleDto = {
  __typename?: 'VehicleDto';
  createdAt: Scalars['DateTime']['output'];
  depotId?: Maybe<Scalars['UUID']['output']>;
  depotName?: Maybe<Scalars['String']['output']>;
  id: Scalars['UUID']['output'];
  parcelCapacity: Scalars['Int']['output'];
  registrationPlate: Scalars['String']['output'];
  status: VehicleStatus;
  type: VehicleType;
  weightCapacityKg: Scalars['Decimal']['output'];
};

export type VehicleFilterInput = {
  and?: InputMaybe<Array<VehicleFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  depot?: InputMaybe<DepotFilterInput>;
  depotId?: InputMaybe<UuidOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<VehicleFilterInput>>;
  parcelCapacity?: InputMaybe<IntOperationFilterInput>;
  registrationPlate?: InputMaybe<StringOperationFilterInput>;
  status?: InputMaybe<VehicleStatusOperationFilterInput>;
  type?: InputMaybe<VehicleTypeOperationFilterInput>;
  weightCapacityKg?: InputMaybe<DecimalOperationFilterInput>;
};

export type VehicleHistoryDto = {
  __typename?: 'VehicleHistoryDto';
  id: Scalars['UUID']['output'];
  registrationPlate: Scalars['String']['output'];
  routes: Array<RouteHistoryDto>;
  totalMileageKm: Scalars['Decimal']['output'];
  totalRoutesCompleted: Scalars['Int']['output'];
  type: VehicleType;
};

export type VehicleJourney = {
  __typename?: 'VehicleJourney';
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  distanceKm: Scalars['Decimal']['output'];
  endMileageKm: Scalars['Decimal']['output'];
  endTime?: Maybe<Scalars['DateTime']['output']>;
  id: Scalars['UUID']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  route?: Maybe<Route>;
  routeId: Scalars['UUID']['output'];
  startMileageKm: Scalars['Decimal']['output'];
  startTime: Scalars['DateTime']['output'];
  vehicle?: Maybe<Vehicle>;
  vehicleId: Scalars['UUID']['output'];
};

export type VehicleJourneyFilterInput = {
  and?: InputMaybe<Array<VehicleJourneyFilterInput>>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  distanceKm?: InputMaybe<DecimalOperationFilterInput>;
  endMileageKm?: InputMaybe<DecimalOperationFilterInput>;
  endTime?: InputMaybe<DateTimeOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<VehicleJourneyFilterInput>>;
  route?: InputMaybe<RouteFilterInput>;
  routeId?: InputMaybe<UuidOperationFilterInput>;
  startMileageKm?: InputMaybe<DecimalOperationFilterInput>;
  startTime?: InputMaybe<DateTimeOperationFilterInput>;
  vehicle?: InputMaybe<VehicleFilterInput>;
  vehicleId?: InputMaybe<UuidOperationFilterInput>;
};

export type VehicleSortInput = {
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  depot?: InputMaybe<DepotSortInput>;
  depotId?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  parcelCapacity?: InputMaybe<SortEnumType>;
  registrationPlate?: InputMaybe<SortEnumType>;
  status?: InputMaybe<SortEnumType>;
  type?: InputMaybe<SortEnumType>;
  weightCapacityKg?: InputMaybe<SortEnumType>;
};

export enum VehicleStatus {
  Available = 'AVAILABLE',
  InUse = 'IN_USE',
  Maintenance = 'MAINTENANCE',
  Retired = 'RETIRED'
}

export type VehicleStatusOperationFilterInput = {
  eq?: InputMaybe<VehicleStatus>;
  in?: InputMaybe<Array<VehicleStatus>>;
  neq?: InputMaybe<VehicleStatus>;
  nin?: InputMaybe<Array<VehicleStatus>>;
};

export enum VehicleType {
  Bike = 'BIKE',
  Car = 'CAR',
  Van = 'VAN'
}

export type VehicleTypeOperationFilterInput = {
  eq?: InputMaybe<VehicleType>;
  in?: InputMaybe<Array<VehicleType>>;
  neq?: InputMaybe<VehicleType>;
  nin?: InputMaybe<Array<VehicleType>>;
};

export enum WeightUnit {
  Kg = 'KG',
  Lb = 'LB'
}

export type WeightUnitOperationFilterInput = {
  eq?: InputMaybe<WeightUnit>;
  in?: InputMaybe<Array<WeightUnit>>;
  neq?: InputMaybe<WeightUnit>;
  nin?: InputMaybe<Array<WeightUnit>>;
};

export type Zone = {
  __typename?: 'Zone';
  aisles: Array<Aisle>;
  boundaryGeometry?: Maybe<Scalars['Geometry']['output']>;
  createdAt: Scalars['DateTime']['output'];
  createdBy?: Maybe<Scalars['String']['output']>;
  deletedAt?: Maybe<Scalars['DateTime']['output']>;
  deletedBy?: Maybe<Scalars['String']['output']>;
  depot: Depot;
  depotId: Scalars['UUID']['output'];
  id: Scalars['UUID']['output'];
  isActive: Scalars['Boolean']['output'];
  isDeleted: Scalars['Boolean']['output'];
  lastModifiedAt?: Maybe<Scalars['DateTime']['output']>;
  lastModifiedBy?: Maybe<Scalars['String']['output']>;
  name: Scalars['String']['output'];
};

export type ZoneFilterInput = {
  aisles?: InputMaybe<ListFilterInputTypeOfAisleFilterInput>;
  and?: InputMaybe<Array<ZoneFilterInput>>;
  boundaryGeometry?: InputMaybe<GeometryFilterInput>;
  createdAt?: InputMaybe<DateTimeOperationFilterInput>;
  createdBy?: InputMaybe<StringOperationFilterInput>;
  deletedAt?: InputMaybe<DateTimeOperationFilterInput>;
  deletedBy?: InputMaybe<StringOperationFilterInput>;
  depot?: InputMaybe<DepotFilterInput>;
  depotId?: InputMaybe<UuidOperationFilterInput>;
  id?: InputMaybe<UuidOperationFilterInput>;
  isActive?: InputMaybe<BooleanOperationFilterInput>;
  isDeleted?: InputMaybe<BooleanOperationFilterInput>;
  lastModifiedAt?: InputMaybe<DateTimeOperationFilterInput>;
  lastModifiedBy?: InputMaybe<StringOperationFilterInput>;
  name?: InputMaybe<StringOperationFilterInput>;
  or?: InputMaybe<Array<ZoneFilterInput>>;
};

export type ZoneLookupDto = {
  __typename?: 'ZoneLookupDto';
  depotId: Scalars['UUID']['output'];
  id: Scalars['UUID']['output'];
  name: Scalars['String']['output'];
};

export type ZoneResult = {
  __typename?: 'ZoneResult';
  createdAt: Scalars['DateTime']['output'];
  depotId: Scalars['UUID']['output'];
  id: Scalars['UUID']['output'];
  isActive: Scalars['Boolean']['output'];
  name: Scalars['String']['output'];
};

export type ZoneSortInput = {
  boundaryGeometry?: InputMaybe<GeometrySortInput>;
  createdAt?: InputMaybe<SortEnumType>;
  createdBy?: InputMaybe<SortEnumType>;
  deletedAt?: InputMaybe<SortEnumType>;
  deletedBy?: InputMaybe<SortEnumType>;
  depot?: InputMaybe<DepotSortInput>;
  depotId?: InputMaybe<SortEnumType>;
  id?: InputMaybe<SortEnumType>;
  isActive?: InputMaybe<SortEnumType>;
  isDeleted?: InputMaybe<SortEnumType>;
  lastModifiedAt?: InputMaybe<SortEnumType>;
  lastModifiedBy?: InputMaybe<SortEnumType>;
  name?: InputMaybe<SortEnumType>;
};

/** A connection to a list of items. */
export type ZonesConnection = {
  __typename?: 'ZonesConnection';
  /** A list of edges. */
  edges?: Maybe<Array<ZonesEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<Zone>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
  /** Identifies the total count of items in the connection. */
  totalCount: Scalars['Int']['output'];
};

/** An edge in a connection. */
export type ZonesEdge = {
  __typename?: 'ZonesEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: Zone;
};

export type GetDepotsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetDepotsQuery = { __typename?: 'Query', depots?: { __typename?: 'DepotsConnection', nodes?: Array<{ __typename?: 'Depot', id: any, name: string, isActive: boolean, createdAt: any, address: { __typename?: 'Address', street1: string, street2?: string | null, city: string, state: string, postalCode: string, countryCode: string, isResidential: boolean, contactName?: string | null, companyName?: string | null, phone?: string | null, email?: string | null, geoLocation?: { __typename?: 'GeoJSONPointType', coordinates?: any | null } | null } }> | null } | null };

export type GetDepotQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetDepotQuery = { __typename?: 'Query', depot?: { __typename?: 'Depot', id: any, name: string, isActive: boolean, createdAt: any, address: { __typename?: 'Address', street1: string, street2?: string | null, city: string, state: string, postalCode: string, countryCode: string, isResidential: boolean, contactName?: string | null, companyName?: string | null, phone?: string | null, email?: string | null, geoLocation?: { __typename?: 'GeoJSONPointType', coordinates?: any | null } | null }, shiftSchedules: Array<{ __typename?: 'ShiftSchedule', dayOfWeek: DayOfWeek, openTime: any, closeTime: any }>, zones: Array<{ __typename?: 'Zone', id: any }> } | null };

export type CreateDepotMutationVariables = Exact<{
  input: CreateDepotCommandInput;
}>;


export type CreateDepotMutation = { __typename?: 'Mutation', createDepot: { __typename?: 'DepotResult', id: any, name: string, isActive: boolean, createdAt: any } };

export type UpdateDepotMutationVariables = Exact<{
  input: UpdateDepotCommandInput;
}>;


export type UpdateDepotMutation = { __typename?: 'Mutation', updateDepot: { __typename?: 'DepotResult', id: any, name: string, isActive: boolean, createdAt: any } };

export type DeleteDepotMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteDepotMutation = { __typename?: 'Mutation', deleteDepot: boolean };

export type GetDriversQueryVariables = Exact<{ [key: string]: never; }>;


export type GetDriversQuery = { __typename?: 'Query', drivers?: { __typename?: 'DriversConnection', nodes?: Array<{ __typename?: 'Driver', id: any, licenseNumber: string, licenseExpiryDate: any, photo?: string | null, userId: any, createdAt: any, user: { __typename?: 'User', id: any, firstName: string, lastName: string, email?: string | null, phoneNumber?: string | null, isActive: boolean, zone?: { __typename?: 'Zone', id: any, name: string } | null, depot?: { __typename?: 'Depot', id: any, name: string } | null }, shiftSchedules: Array<{ __typename?: 'ShiftSchedule', dayOfWeek: DayOfWeek, openTime: any, closeTime: any }>, daysOff: Array<{ __typename?: 'DayOff', date: any }> }> | null } | null };

export type GetDriverQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetDriverQuery = { __typename?: 'Query', driver?: { __typename?: 'Driver', id: any, licenseNumber: string, licenseExpiryDate: any, photo?: string | null, userId: any, createdAt: any, lastModifiedAt?: any | null, user: { __typename?: 'User', id: any, firstName: string, lastName: string, email?: string | null, phoneNumber?: string | null, isActive: boolean, zone?: { __typename?: 'Zone', id: any, name: string } | null, depot?: { __typename?: 'Depot', id: any, name: string } | null }, shiftSchedules: Array<{ __typename?: 'ShiftSchedule', dayOfWeek: DayOfWeek, openTime: any, closeTime: any }>, daysOff: Array<{ __typename?: 'DayOff', date: any }> } | null };

export type CreateDriverMutationVariables = Exact<{
  input: CreateDriverCommandInput;
}>;


export type CreateDriverMutation = { __typename?: 'Mutation', createDriver: { __typename?: 'DriverResult', id: any, licenseNumber: string, licenseExpiryDate: any, photo?: string | null, userId: any, createdAt: any } };

export type UpdateDriverMutationVariables = Exact<{
  input: UpdateDriverCommandInput;
}>;


export type UpdateDriverMutation = { __typename?: 'Mutation', updateDriver: { __typename?: 'DriverResult', id: any, licenseNumber: string, licenseExpiryDate: any, photo?: string | null, userId: any, createdAt: any } };

export type DeleteDriverMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteDriverMutation = { __typename?: 'Mutation', deleteDriver: boolean };

export type GetParcelByTrackingNumberQueryVariables = Exact<{
  trackingNumber: Scalars['String']['input'];
}>;


export type GetParcelByTrackingNumberQuery = { __typename?: 'Query', parcelByTrackingNumber?: { __typename?: 'Parcel', id: any, trackingNumber: string, description?: string | null, serviceType: ServiceType, status: ParcelStatus, weight: any, weightUnit: WeightUnit, length: any, width: any, height: any, dimensionUnit: DimensionUnit, declaredValue: any, currency: string, estimatedDeliveryDate?: any | null, actualDeliveryDate?: any | null, deliveryAttempts: number, parcelType?: ParcelType | null, notes?: string | null, createdAt: any, lastModifiedAt?: any | null, shipperAddress: { __typename?: 'Address', street1: string, street2?: string | null, city: string, state: string, postalCode: string, countryCode: string, isResidential: boolean, contactName?: string | null, companyName?: string | null, phone?: string | null, email?: string | null }, recipientAddress: { __typename?: 'Address', street1: string, street2?: string | null, city: string, state: string, postalCode: string, countryCode: string, isResidential: boolean, contactName?: string | null, companyName?: string | null, phone?: string | null, email?: string | null }, zone?: { __typename?: 'Zone', id: any, name: string } | null, deliveryConfirmation?: { __typename?: 'DeliveryConfirmation', id: any, receivedBy?: string | null, deliveryLocation?: string | null, signatureImage?: string | null, photo?: string | null, deliveredAt: any } | null, routeStop?: { __typename?: 'RouteStop', id: any, sequenceNumber: number, status: RouteStopStatus, street1: string, arrivalTime?: any | null, departureTime?: any | null, route?: { __typename?: 'Route', id: any, name: string, status: RouteStatus, plannedStartTime: any, driver?: { __typename?: 'Driver', user: { __typename?: 'User', firstName: string, lastName: string } } | null, vehicle?: { __typename?: 'Vehicle', registrationPlate: string } | null } | null } | null, bin?: { __typename?: 'Bin', id: any, label: string, aisle: { __typename?: 'Aisle', label: string, name: string }, zone: { __typename?: 'Zone', name: string, depot: { __typename?: 'Depot', name: string } } } | null } | null };

export type GetTrackingEventsQueryVariables = Exact<{
  parcelId: Scalars['UUID']['input'];
  first?: InputMaybe<Scalars['Int']['input']>;
  after?: InputMaybe<Scalars['String']['input']>;
  order?: InputMaybe<Array<TrackingEventSortInput> | TrackingEventSortInput>;
}>;


export type GetTrackingEventsQuery = { __typename?: 'Query', trackingEvents?: { __typename?: 'TrackingEventsConnection', totalCount: number, nodes?: Array<{ __typename?: 'TrackingEvent', id: any, timestamp: any, eventType: EventType, description?: string | null, locationCity?: string | null, locationState?: string | null, locationCountry?: string | null, operator?: string | null, delayReason?: ExceptionReason | null }> | null, pageInfo: { __typename?: 'PageInfo', hasNextPage: boolean, endCursor?: string | null } } | null };

export type GetParcelAuditLogsQueryVariables = Exact<{
  parcelId: Scalars['UUID']['input'];
  first?: InputMaybe<Scalars['Int']['input']>;
  after?: InputMaybe<Scalars['String']['input']>;
  order?: InputMaybe<Array<ParcelAuditLogSortInput> | ParcelAuditLogSortInput>;
}>;


export type GetParcelAuditLogsQuery = { __typename?: 'Query', parcelAuditLogs?: { __typename?: 'ParcelAuditLogsConnection', nodes?: Array<{ __typename?: 'ParcelAuditLog', id: any, propertyName: string, oldValue?: string | null, newValue?: string | null, changedBy: string, createdAt: any }> | null, pageInfo: { __typename?: 'PageInfo', hasNextPage: boolean, endCursor?: string | null } } | null };

export type CreateParcelMutationVariables = Exact<{
  input: CreateParcelCommandInput;
}>;


export type CreateParcelMutation = { __typename?: 'Mutation', createParcel: { __typename?: 'CreateParcelResult', id: any, trackingNumber: string, status: ParcelStatus, serviceType: ServiceType, createdAt: any, estimatedDeliveryDate: any } };

export type UpdateParcelMutationVariables = Exact<{
  input: UpdateParcelCommandInput;
}>;


export type UpdateParcelMutation = { __typename?: 'Mutation', updateParcel: { __typename?: 'UpdateParcelResult', id: any, trackingNumber: string, status: ParcelStatus, description?: string | null, weight: any, lastModifiedAt: any } };

export type CancelParcelMutationVariables = Exact<{
  input: CancelParcelCommandInput;
}>;


export type CancelParcelMutation = { __typename?: 'Mutation', cancelParcel: { __typename?: 'CancelParcelResult', id: any, trackingNumber: string, status: ParcelStatus } };

export type ChangeParcelStatusMutationVariables = Exact<{
  input: ChangeParcelStatusCommandInput;
}>;


export type ChangeParcelStatusMutation = { __typename?: 'Mutation', changeParcelStatus: { __typename?: 'ChangeParcelStatusResult', id: any, trackingNumber: string, status: ParcelStatus, deliveryAttempts: number } };

export type GetRoutesQueryVariables = Exact<{
  where?: InputMaybe<RouteFilterInput>;
  order?: InputMaybe<Array<RouteSortInput> | RouteSortInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
  after?: InputMaybe<Scalars['String']['input']>;
}>;


export type GetRoutesQuery = { __typename?: 'Query', routes?: { __typename?: 'RoutesConnection', totalCount: number, nodes?: Array<{ __typename?: 'Route', id: any, name: string, status: RouteStatus, plannedStartTime: any, actualStartTime?: any | null, actualEndTime?: any | null, totalDistanceKm: any, totalParcelCount: number, zoneId?: any | null, vehicleId?: any | null, driverId?: any | null, zone?: { __typename?: 'Zone', name: string } | null, vehicle?: { __typename?: 'Vehicle', registrationPlate: string } | null, driver?: { __typename?: 'Driver', user: { __typename?: 'User', firstName: string, lastName: string } } | null }> | null, pageInfo: { __typename?: 'PageInfo', hasNextPage: boolean, hasPreviousPage: boolean, startCursor?: string | null, endCursor?: string | null } } | null };

export type GetRouteQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetRouteQuery = { __typename?: 'Query', route?: { __typename?: 'Route', id: any, name: string, status: RouteStatus, plannedStartTime: any, actualStartTime?: any | null, actualEndTime?: any | null, totalDistanceKm: any, totalParcelCount: number, zoneId?: any | null, vehicleId?: any | null, driverId?: any | null, createdAt: any, zone?: { __typename?: 'Zone', name: string } | null, vehicle?: { __typename?: 'Vehicle', registrationPlate: string } | null, driver?: { __typename?: 'Driver', user: { __typename?: 'User', firstName: string, lastName: string } } | null, routeStops: Array<{ __typename?: 'RouteStop', id: any, sequenceNumber: number, status: RouteStopStatus, arrivalTime?: any | null, departureTime?: any | null, estimatedServiceMinutes: number, accessInstructions?: string | null, street1: string, parcels: Array<{ __typename?: 'Parcel', id: any, trackingNumber: string, status: ParcelStatus }> }> } | null };

export type GetAvailableDriversQueryVariables = Exact<{
  date: Scalars['DateTime']['input'];
}>;


export type GetAvailableDriversQuery = { __typename?: 'Query', availableDrivers: Array<{ __typename?: 'AvailableDriverDto', id: any, name: string, shift?: { __typename?: 'ShiftInfoDto', openTime: any, closeTime: any } | null, assignedRoutes: Array<{ __typename?: 'DriverRouteSummaryDto', id: any, name: string, status: RouteStatus }> }> };

export type GetRoutesForMapQueryVariables = Exact<{
  where?: InputMaybe<RouteFilterInput>;
  first?: InputMaybe<Scalars['Int']['input']>;
}>;


export type GetRoutesForMapQuery = { __typename?: 'Query', routes?: { __typename?: 'RoutesConnection', nodes?: Array<{ __typename?: 'Route', id: any, name: string, status: RouteStatus, zoneId?: any | null, routeStops: Array<{ __typename?: 'RouteStop', id: any, sequenceNumber: number, status: RouteStopStatus, street1: string, geoLocation?: { __typename?: 'GeoJSONPointType', coordinates?: any | null } | null, parcels: Array<{ __typename?: 'Parcel', id: any, trackingNumber: string, status: ParcelStatus }> }> }> | null } | null };

export type CreateRouteMutationVariables = Exact<{
  input: CreateRouteCommandInput;
}>;


export type CreateRouteMutation = { __typename?: 'Mutation', createRoute: { __typename?: 'RouteDto', id: any, name: string, status: RouteStatus, plannedStartTime: any, zoneId?: any | null, zoneName?: string | null, estimatedStopCount: number, vehicleId?: any | null, vehiclePlate?: string | null, driverId?: any | null, driverName?: string | null, createdAt: any } };

export type UpdateRouteMutationVariables = Exact<{
  input: UpdateRouteCommandInput;
}>;


export type UpdateRouteMutation = { __typename?: 'Mutation', updateRoute: { __typename?: 'RouteDto', id: any, name: string, status: RouteStatus, plannedStartTime: any, zoneId?: any | null, zoneName?: string | null, estimatedStopCount: number, vehicleId?: any | null, vehiclePlate?: string | null, driverId?: any | null, driverName?: string | null, createdAt: any } };

export type DeleteRouteMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteRouteMutation = { __typename?: 'Mutation', deleteRoute: boolean };

export type ChangeRouteStatusMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
  newStatus: RouteStatus;
}>;


export type ChangeRouteStatusMutation = { __typename?: 'Mutation', changeRouteStatus: { __typename?: 'RouteDto', id: any, name: string, status: RouteStatus, plannedStartTime: any, actualStartTime?: any | null, actualEndTime?: any | null, totalDistanceKm: any, totalParcelCount: number, vehicleId?: any | null, vehiclePlate?: string | null, driverId?: any | null, driverName?: string | null, createdAt: any } };

export type AssignDriverToRouteMutationVariables = Exact<{
  routeId: Scalars['UUID']['input'];
  driverId?: InputMaybe<Scalars['UUID']['input']>;
}>;


export type AssignDriverToRouteMutation = { __typename?: 'Mutation', assignDriverToRoute: { __typename?: 'RouteDto', id: any, name: string, status: RouteStatus, driverId?: any | null, driverName?: string | null, vehicleId?: any | null, vehiclePlate?: string | null } };

export type AddParcelsToRouteMutationVariables = Exact<{
  input: AddParcelsToRouteCommandInput;
}>;


export type AddParcelsToRouteMutation = { __typename?: 'Mutation', addParcelsToRoute: { __typename?: 'RouteDto', id: any, name: string, status: RouteStatus, totalParcelCount: number, estimatedStopCount: number, stops: Array<{ __typename?: 'RouteStopDto', id: any, sequenceNumber: number, street1: string, parcelCount: number, parcels: Array<{ __typename?: 'RouteStopParcelDto', id: any, trackingNumber: string, status: ParcelStatus }> }> } };

export type AutoAssignParcelsByZoneMutationVariables = Exact<{
  input: AutoAssignParcelsByZoneCommandInput;
}>;


export type AutoAssignParcelsByZoneMutation = { __typename?: 'Mutation', autoAssignParcelsByZone: { __typename?: 'RouteDto', id: any, name: string, status: RouteStatus, totalParcelCount: number, estimatedStopCount: number, stops: Array<{ __typename?: 'RouteStopDto', id: any, sequenceNumber: number, street1: string, parcelCount: number, parcels: Array<{ __typename?: 'RouteStopParcelDto', id: any, trackingNumber: string, status: ParcelStatus }> }> } };

export type RemoveParcelsFromRouteMutationVariables = Exact<{
  input: RemoveParcelsFromRouteCommandInput;
}>;


export type RemoveParcelsFromRouteMutation = { __typename?: 'Mutation', removeParcelsFromRoute: { __typename?: 'RouteDto', id: any, name: string, status: RouteStatus, totalParcelCount: number, estimatedStopCount: number, stops: Array<{ __typename?: 'RouteStopDto', id: any, sequenceNumber: number, street1: string, parcelCount: number, parcels: Array<{ __typename?: 'RouteStopParcelDto', id: any, trackingNumber: string, status: ParcelStatus }> }> } };

export type ReorderRouteStopsMutationVariables = Exact<{
  input: ReorderRouteStopsCommandInput;
}>;


export type ReorderRouteStopsMutation = { __typename?: 'Mutation', reorderRouteStops: { __typename?: 'RouteDto', id: any, name: string, status: RouteStatus, stops: Array<{ __typename?: 'RouteStopDto', id: any, sequenceNumber: number, street1: string, parcelCount: number }> } };

export type OptimizeRouteStopOrderMutationVariables = Exact<{
  input: OptimizeRouteStopOrderCommandInput;
}>;


export type OptimizeRouteStopOrderMutation = { __typename?: 'Mutation', optimizeRouteStopOrder: { __typename?: 'RouteDto', id: any, name: string, status: RouteStatus, stops: Array<{ __typename?: 'RouteStopDto', id: any, sequenceNumber: number, street1: string, parcelCount: number }> } };

export type DispatchRouteMutationVariables = Exact<{
  input: DispatchRouteCommandInput;
}>;


export type DispatchRouteMutation = { __typename?: 'Mutation', dispatchRoute: { __typename?: 'RouteDto', id: any, name: string, status: RouteStatus, plannedStartTime: any, actualStartTime?: any | null, actualEndTime?: any | null, totalDistanceKm: any, totalParcelCount: number, vehicleId?: any | null, vehiclePlate?: string | null, driverId?: any | null, driverName?: string | null, createdAt: any } };

export type GetUsersQueryVariables = Exact<{
  first?: InputMaybe<Scalars['Int']['input']>;
  after?: InputMaybe<Scalars['String']['input']>;
  where?: InputMaybe<UserFilterInput>;
  order?: InputMaybe<Array<UserSortInput> | UserSortInput>;
}>;


export type GetUsersQuery = { __typename?: 'Query', users?: { __typename?: 'UsersConnection', totalCount: number, nodes?: Array<{ __typename?: 'User', id: any, firstName: string, lastName: string, email?: string | null, phoneNumber?: string | null, status: UserStatus, roleId?: any | null, zoneId?: any | null, depotId?: any | null, createdAt: any, role?: { __typename?: 'Role', id: any, name?: string | null } | null, zone?: { __typename?: 'Zone', id: any, name: string } | null, depot?: { __typename?: 'Depot', id: any, name: string } | null }> | null, pageInfo: { __typename?: 'PageInfo', hasNextPage: boolean, hasPreviousPage: boolean, startCursor?: string | null, endCursor?: string | null } } | null };

export type GetUserQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetUserQuery = { __typename?: 'Query', user?: { __typename?: 'User', id: any, firstName: string, lastName: string, email?: string | null, phoneNumber?: string | null, status: UserStatus, roleId?: any | null, zoneId?: any | null, depotId?: any | null, createdAt: any, role?: { __typename?: 'Role', id: any, name?: string | null } | null, zone?: { __typename?: 'Zone', id: any, name: string } | null, depot?: { __typename?: 'Depot', id: any, name: string } | null } | null };

export type GetUserManagementLookupsQueryVariables = Exact<{ [key: string]: never; }>;


export type GetUserManagementLookupsQuery = { __typename?: 'Query', userManagementLookups: { __typename?: 'UserManagementLookupsDto', roles: Array<{ __typename?: 'RoleLookupDto', id: any, name: string, description?: string | null }>, depots: Array<{ __typename?: 'DepotLookupDto', id: any, name: string }>, zones: Array<{ __typename?: 'ZoneLookupDto', id: any, name: string, depotId: any }> } };

export type CreateUserMutationVariables = Exact<{
  firstName: Scalars['String']['input'];
  lastName: Scalars['String']['input'];
  email: Scalars['String']['input'];
  phone?: InputMaybe<Scalars['String']['input']>;
  roleId: Scalars['UUID']['input'];
  zoneId?: InputMaybe<Scalars['UUID']['input']>;
  depotId?: InputMaybe<Scalars['UUID']['input']>;
  password: Scalars['String']['input'];
}>;


export type CreateUserMutation = { __typename?: 'Mutation', createUser: { __typename?: 'UserDto', id: any, firstName: string, lastName: string, email: string, phoneNumber?: string | null, status: UserStatus, roleId?: any | null, roleName?: string | null, zoneId?: any | null, zoneName?: string | null, depotId?: any | null, depotName?: string | null, createdAt: any } };

export type UpdateUserMutationVariables = Exact<{
  userId: Scalars['UUID']['input'];
  firstName: Scalars['String']['input'];
  lastName: Scalars['String']['input'];
  email: Scalars['String']['input'];
  phone?: InputMaybe<Scalars['String']['input']>;
  roleId?: InputMaybe<Scalars['UUID']['input']>;
  zoneId?: InputMaybe<Scalars['UUID']['input']>;
  depotId?: InputMaybe<Scalars['UUID']['input']>;
}>;


export type UpdateUserMutation = { __typename?: 'Mutation', updateUser: { __typename?: 'UserDto', id: any, firstName: string, lastName: string, email: string, phoneNumber?: string | null, status: UserStatus, roleId?: any | null, roleName?: string | null, zoneId?: any | null, zoneName?: string | null, depotId?: any | null, depotName?: string | null, createdAt: any } };

export type DeactivateUserMutationVariables = Exact<{
  userId: Scalars['UUID']['input'];
}>;


export type DeactivateUserMutation = { __typename?: 'Mutation', deactivateUser: { __typename?: 'UserDto', id: any, firstName: string, lastName: string, email: string, phoneNumber?: string | null, status: UserStatus, roleId?: any | null, roleName?: string | null, zoneId?: any | null, zoneName?: string | null, depotId?: any | null, depotName?: string | null, createdAt: any } };

export type ActivateUserMutationVariables = Exact<{
  userId: Scalars['UUID']['input'];
}>;


export type ActivateUserMutation = { __typename?: 'Mutation', activateUser: { __typename?: 'UserDto', id: any, firstName: string, lastName: string, email: string, phoneNumber?: string | null, status: UserStatus, roleId?: any | null, roleName?: string | null, zoneId?: any | null, zoneName?: string | null, depotId?: any | null, depotName?: string | null, createdAt: any } };

export type ResetPasswordMutationVariables = Exact<{
  email: Scalars['String']['input'];
}>;


export type ResetPasswordMutation = { __typename?: 'Mutation', resetPassword: boolean };

export type GetVehiclesQueryVariables = Exact<{
  where?: InputMaybe<VehicleFilterInput>;
}>;


export type GetVehiclesQuery = { __typename?: 'Query', vehicles: Array<{ __typename?: 'Vehicle', id: any, registrationPlate: string, type: VehicleType, status: VehicleStatus, depotId?: any | null }> };

export type GetVehicleQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetVehicleQuery = { __typename?: 'Query', vehicle?: { __typename?: 'Vehicle', id: any, registrationPlate: string, type: VehicleType, parcelCapacity: number, weightCapacityKg: any, status: VehicleStatus, depotId?: any | null, createdAt: any, depot?: { __typename?: 'Depot', name: string } | null } | null };

export type GetVehicleHistoryQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetVehicleHistoryQuery = { __typename?: 'Query', vehicleHistory?: { __typename?: 'VehicleHistoryDto', id: any, registrationPlate: string, type: VehicleType, totalMileageKm: any, totalRoutesCompleted: number, routes: Array<{ __typename?: 'RouteHistoryDto', routeId: any, routeName: string, completedAt: any, distanceKm: any, parcelCount: number }> } | null };

export type CreateVehicleMutationVariables = Exact<{
  registrationPlate: Scalars['String']['input'];
  type: VehicleType;
  parcelCapacity: Scalars['Int']['input'];
  weightCapacityKg: Scalars['Decimal']['input'];
  depotId?: InputMaybe<Scalars['UUID']['input']>;
}>;


export type CreateVehicleMutation = { __typename?: 'Mutation', createVehicle: { __typename?: 'VehicleDto', id: any, registrationPlate: string, type: VehicleType, parcelCapacity: number, weightCapacityKg: any, status: VehicleStatus, depotId?: any | null, createdAt: any } };

export type UpdateVehicleMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
  registrationPlate: Scalars['String']['input'];
  type: VehicleType;
  parcelCapacity: Scalars['Int']['input'];
  weightCapacityKg: Scalars['Decimal']['input'];
  depotId?: InputMaybe<Scalars['UUID']['input']>;
}>;


export type UpdateVehicleMutation = { __typename?: 'Mutation', updateVehicle: { __typename?: 'VehicleDto', id: any, registrationPlate: string, type: VehicleType, parcelCapacity: number, weightCapacityKg: any, status: VehicleStatus, depotId?: any | null, createdAt: any } };

export type DeleteVehicleMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteVehicleMutation = { __typename?: 'Mutation', deleteVehicle: boolean };

export type ChangeVehicleStatusMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
  newStatus: VehicleStatus;
}>;


export type ChangeVehicleStatusMutation = { __typename?: 'Mutation', changeVehicleStatus: { __typename?: 'VehicleDto', id: any, registrationPlate: string, type: VehicleType, parcelCapacity: number, weightCapacityKg: any, status: VehicleStatus, depotId?: any | null, createdAt: any } };

export type GetZonesQueryVariables = Exact<{ [key: string]: never; }>;


export type GetZonesQuery = { __typename?: 'Query', zones?: { __typename?: 'ZonesConnection', nodes?: Array<{ __typename?: 'Zone', id: any, name: string, boundaryGeometry?: any | null, depotId: any, isActive: boolean, createdAt: any, depot: { __typename?: 'Depot', name: string } }> | null } | null };

export type GetZoneQueryVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type GetZoneQuery = { __typename?: 'Query', zone?: { __typename?: 'Zone', id: any, name: string, boundaryGeometry?: any | null, depotId: any, isActive: boolean, createdAt: any, lastModifiedAt?: any | null, depot: { __typename?: 'Depot', name: string } } | null };

export type CreateZoneMutationVariables = Exact<{
  input: CreateZoneCommandInput;
}>;


export type CreateZoneMutation = { __typename?: 'Mutation', createZone: { __typename?: 'ZoneResult', id: any, name: string, depotId: any, isActive: boolean, createdAt: any } };

export type UpdateZoneMutationVariables = Exact<{
  input: UpdateZoneCommandInput;
}>;


export type UpdateZoneMutation = { __typename?: 'Mutation', updateZone: { __typename?: 'ZoneResult', id: any, name: string, depotId: any, isActive: boolean, createdAt: any } };

export type DeleteZoneMutationVariables = Exact<{
  id: Scalars['UUID']['input'];
}>;


export type DeleteZoneMutation = { __typename?: 'Mutation', deleteZone: boolean };


export const GetDepotsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDepots"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"depots"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"address"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"street2"}},{"kind":"Field","name":{"kind":"Name","value":"city"}},{"kind":"Field","name":{"kind":"Name","value":"state"}},{"kind":"Field","name":{"kind":"Name","value":"postalCode"}},{"kind":"Field","name":{"kind":"Name","value":"countryCode"}},{"kind":"Field","name":{"kind":"Name","value":"isResidential"}},{"kind":"Field","name":{"kind":"Name","value":"contactName"}},{"kind":"Field","name":{"kind":"Name","value":"companyName"}},{"kind":"Field","name":{"kind":"Name","value":"phone"}},{"kind":"Field","name":{"kind":"Name","value":"email"}},{"kind":"Field","name":{"kind":"Name","value":"geoLocation"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coordinates"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"isActive"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]}}]} as unknown as DocumentNode<GetDepotsQuery, GetDepotsQueryVariables>;
export const GetDepotDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDepot"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"depot"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"address"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"street2"}},{"kind":"Field","name":{"kind":"Name","value":"city"}},{"kind":"Field","name":{"kind":"Name","value":"state"}},{"kind":"Field","name":{"kind":"Name","value":"postalCode"}},{"kind":"Field","name":{"kind":"Name","value":"countryCode"}},{"kind":"Field","name":{"kind":"Name","value":"isResidential"}},{"kind":"Field","name":{"kind":"Name","value":"contactName"}},{"kind":"Field","name":{"kind":"Name","value":"companyName"}},{"kind":"Field","name":{"kind":"Name","value":"phone"}},{"kind":"Field","name":{"kind":"Name","value":"email"}},{"kind":"Field","name":{"kind":"Name","value":"geoLocation"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coordinates"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"shiftSchedules"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"dayOfWeek"}},{"kind":"Field","name":{"kind":"Name","value":"openTime"}},{"kind":"Field","name":{"kind":"Name","value":"closeTime"}}]}},{"kind":"Field","name":{"kind":"Name","value":"isActive"}},{"kind":"Field","name":{"kind":"Name","value":"zones"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<GetDepotQuery, GetDepotQueryVariables>;
export const CreateDepotDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateDepot"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateDepotCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createDepot"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"isActive"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<CreateDepotMutation, CreateDepotMutationVariables>;
export const UpdateDepotDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateDepot"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateDepotCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateDepot"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"isActive"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<UpdateDepotMutation, UpdateDepotMutationVariables>;
export const DeleteDepotDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteDepot"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteDepot"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteDepotMutation, DeleteDepotMutationVariables>;
export const GetDriversDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDrivers"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"drivers"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"licenseNumber"}},{"kind":"Field","name":{"kind":"Name","value":"licenseExpiryDate"}},{"kind":"Field","name":{"kind":"Name","value":"photo"}},{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"user"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"email"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"isActive"}},{"kind":"Field","name":{"kind":"Name","value":"zone"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"depot"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"shiftSchedules"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"dayOfWeek"}},{"kind":"Field","name":{"kind":"Name","value":"openTime"}},{"kind":"Field","name":{"kind":"Name","value":"closeTime"}}]}},{"kind":"Field","name":{"kind":"Name","value":"daysOff"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"date"}}]}}]}}]}}]}}]} as unknown as DocumentNode<GetDriversQuery, GetDriversQueryVariables>;
export const GetDriverDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetDriver"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"driver"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"licenseNumber"}},{"kind":"Field","name":{"kind":"Name","value":"licenseExpiryDate"}},{"kind":"Field","name":{"kind":"Name","value":"photo"}},{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastModifiedAt"}},{"kind":"Field","name":{"kind":"Name","value":"user"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"email"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"isActive"}},{"kind":"Field","name":{"kind":"Name","value":"zone"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"depot"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"shiftSchedules"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"dayOfWeek"}},{"kind":"Field","name":{"kind":"Name","value":"openTime"}},{"kind":"Field","name":{"kind":"Name","value":"closeTime"}}]}},{"kind":"Field","name":{"kind":"Name","value":"daysOff"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"date"}}]}}]}}]}}]} as unknown as DocumentNode<GetDriverQuery, GetDriverQueryVariables>;
export const CreateDriverDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateDriver"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateDriverCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createDriver"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"licenseNumber"}},{"kind":"Field","name":{"kind":"Name","value":"licenseExpiryDate"}},{"kind":"Field","name":{"kind":"Name","value":"photo"}},{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<CreateDriverMutation, CreateDriverMutationVariables>;
export const UpdateDriverDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateDriver"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateDriverCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateDriver"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"licenseNumber"}},{"kind":"Field","name":{"kind":"Name","value":"licenseExpiryDate"}},{"kind":"Field","name":{"kind":"Name","value":"photo"}},{"kind":"Field","name":{"kind":"Name","value":"userId"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<UpdateDriverMutation, UpdateDriverMutationVariables>;
export const DeleteDriverDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteDriver"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteDriver"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteDriverMutation, DeleteDriverMutationVariables>;
export const GetParcelByTrackingNumberDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetParcelByTrackingNumber"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"trackingNumber"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"parcelByTrackingNumber"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"trackingNumber"},"value":{"kind":"Variable","name":{"kind":"Name","value":"trackingNumber"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackingNumber"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"serviceType"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"weight"}},{"kind":"Field","name":{"kind":"Name","value":"weightUnit"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"width"}},{"kind":"Field","name":{"kind":"Name","value":"height"}},{"kind":"Field","name":{"kind":"Name","value":"dimensionUnit"}},{"kind":"Field","name":{"kind":"Name","value":"declaredValue"}},{"kind":"Field","name":{"kind":"Name","value":"currency"}},{"kind":"Field","name":{"kind":"Name","value":"estimatedDeliveryDate"}},{"kind":"Field","name":{"kind":"Name","value":"actualDeliveryDate"}},{"kind":"Field","name":{"kind":"Name","value":"deliveryAttempts"}},{"kind":"Field","name":{"kind":"Name","value":"parcelType"}},{"kind":"Field","name":{"kind":"Name","value":"notes"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastModifiedAt"}},{"kind":"Field","name":{"kind":"Name","value":"shipperAddress"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"street2"}},{"kind":"Field","name":{"kind":"Name","value":"city"}},{"kind":"Field","name":{"kind":"Name","value":"state"}},{"kind":"Field","name":{"kind":"Name","value":"postalCode"}},{"kind":"Field","name":{"kind":"Name","value":"countryCode"}},{"kind":"Field","name":{"kind":"Name","value":"isResidential"}},{"kind":"Field","name":{"kind":"Name","value":"contactName"}},{"kind":"Field","name":{"kind":"Name","value":"companyName"}},{"kind":"Field","name":{"kind":"Name","value":"phone"}},{"kind":"Field","name":{"kind":"Name","value":"email"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recipientAddress"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"street2"}},{"kind":"Field","name":{"kind":"Name","value":"city"}},{"kind":"Field","name":{"kind":"Name","value":"state"}},{"kind":"Field","name":{"kind":"Name","value":"postalCode"}},{"kind":"Field","name":{"kind":"Name","value":"countryCode"}},{"kind":"Field","name":{"kind":"Name","value":"isResidential"}},{"kind":"Field","name":{"kind":"Name","value":"contactName"}},{"kind":"Field","name":{"kind":"Name","value":"companyName"}},{"kind":"Field","name":{"kind":"Name","value":"phone"}},{"kind":"Field","name":{"kind":"Name","value":"email"}}]}},{"kind":"Field","name":{"kind":"Name","value":"zone"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"deliveryConfirmation"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"receivedBy"}},{"kind":"Field","name":{"kind":"Name","value":"deliveryLocation"}},{"kind":"Field","name":{"kind":"Name","value":"signatureImage"}},{"kind":"Field","name":{"kind":"Name","value":"photo"}},{"kind":"Field","name":{"kind":"Name","value":"deliveredAt"}}]}},{"kind":"Field","name":{"kind":"Name","value":"routeStop"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"sequenceNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"arrivalTime"}},{"kind":"Field","name":{"kind":"Name","value":"departureTime"}},{"kind":"Field","name":{"kind":"Name","value":"route"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"plannedStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"driver"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"user"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"vehicle"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"registrationPlate"}}]}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"bin"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"label"}},{"kind":"Field","name":{"kind":"Name","value":"aisle"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"label"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"zone"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"depot"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<GetParcelByTrackingNumberQuery, GetParcelByTrackingNumberQueryVariables>;
export const GetTrackingEventsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetTrackingEvents"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"parcelId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"first"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"after"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"order"}},"type":{"kind":"ListType","type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"TrackingEventSortInput"}}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"trackingEvents"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"parcelId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"parcelId"}}},{"kind":"Argument","name":{"kind":"Name","value":"first"},"value":{"kind":"Variable","name":{"kind":"Name","value":"first"}}},{"kind":"Argument","name":{"kind":"Name","value":"after"},"value":{"kind":"Variable","name":{"kind":"Name","value":"after"}}},{"kind":"Argument","name":{"kind":"Name","value":"order"},"value":{"kind":"Variable","name":{"kind":"Name","value":"order"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"timestamp"}},{"kind":"Field","name":{"kind":"Name","value":"eventType"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"locationCity"}},{"kind":"Field","name":{"kind":"Name","value":"locationState"}},{"kind":"Field","name":{"kind":"Name","value":"locationCountry"}},{"kind":"Field","name":{"kind":"Name","value":"operator"}},{"kind":"Field","name":{"kind":"Name","value":"delayReason"}}]}},{"kind":"Field","name":{"kind":"Name","value":"pageInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"hasNextPage"}},{"kind":"Field","name":{"kind":"Name","value":"endCursor"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}}]} as unknown as DocumentNode<GetTrackingEventsQuery, GetTrackingEventsQueryVariables>;
export const GetParcelAuditLogsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetParcelAuditLogs"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"parcelId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"first"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"after"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"order"}},"type":{"kind":"ListType","type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ParcelAuditLogSortInput"}}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"parcelAuditLogs"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"parcelId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"parcelId"}}},{"kind":"Argument","name":{"kind":"Name","value":"first"},"value":{"kind":"Variable","name":{"kind":"Name","value":"first"}}},{"kind":"Argument","name":{"kind":"Name","value":"after"},"value":{"kind":"Variable","name":{"kind":"Name","value":"after"}}},{"kind":"Argument","name":{"kind":"Name","value":"order"},"value":{"kind":"Variable","name":{"kind":"Name","value":"order"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"propertyName"}},{"kind":"Field","name":{"kind":"Name","value":"oldValue"}},{"kind":"Field","name":{"kind":"Name","value":"newValue"}},{"kind":"Field","name":{"kind":"Name","value":"changedBy"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}},{"kind":"Field","name":{"kind":"Name","value":"pageInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"hasNextPage"}},{"kind":"Field","name":{"kind":"Name","value":"endCursor"}}]}}]}}]}}]} as unknown as DocumentNode<GetParcelAuditLogsQuery, GetParcelAuditLogsQueryVariables>;
export const CreateParcelDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateParcel"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateParcelCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createParcel"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackingNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"serviceType"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"estimatedDeliveryDate"}}]}}]}}]} as unknown as DocumentNode<CreateParcelMutation, CreateParcelMutationVariables>;
export const UpdateParcelDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateParcel"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateParcelCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateParcel"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackingNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"weight"}},{"kind":"Field","name":{"kind":"Name","value":"lastModifiedAt"}}]}}]}}]} as unknown as DocumentNode<UpdateParcelMutation, UpdateParcelMutationVariables>;
export const CancelParcelDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CancelParcel"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CancelParcelCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"cancelParcel"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackingNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]}}]} as unknown as DocumentNode<CancelParcelMutation, CancelParcelMutationVariables>;
export const ChangeParcelStatusDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ChangeParcelStatus"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ChangeParcelStatusCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"changeParcelStatus"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackingNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"deliveryAttempts"}}]}}]}}]} as unknown as DocumentNode<ChangeParcelStatusMutation, ChangeParcelStatusMutationVariables>;
export const GetRoutesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetRoutes"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"where"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"RouteFilterInput"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"order"}},"type":{"kind":"ListType","type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"RouteSortInput"}}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"first"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"after"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"routes"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"Variable","name":{"kind":"Name","value":"where"}}},{"kind":"Argument","name":{"kind":"Name","value":"order"},"value":{"kind":"Variable","name":{"kind":"Name","value":"order"}}},{"kind":"Argument","name":{"kind":"Name","value":"first"},"value":{"kind":"Variable","name":{"kind":"Name","value":"first"}}},{"kind":"Argument","name":{"kind":"Name","value":"after"},"value":{"kind":"Variable","name":{"kind":"Name","value":"after"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"plannedStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"actualStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"actualEndTime"}},{"kind":"Field","name":{"kind":"Name","value":"totalDistanceKm"}},{"kind":"Field","name":{"kind":"Name","value":"totalParcelCount"}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"zone"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"vehicleId"}},{"kind":"Field","name":{"kind":"Name","value":"vehicle"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"registrationPlate"}}]}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driver"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"user"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}}]}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"pageInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"hasNextPage"}},{"kind":"Field","name":{"kind":"Name","value":"hasPreviousPage"}},{"kind":"Field","name":{"kind":"Name","value":"startCursor"}},{"kind":"Field","name":{"kind":"Name","value":"endCursor"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}}]} as unknown as DocumentNode<GetRoutesQuery, GetRoutesQueryVariables>;
export const GetRouteDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetRoute"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"route"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"plannedStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"actualStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"actualEndTime"}},{"kind":"Field","name":{"kind":"Name","value":"totalDistanceKm"}},{"kind":"Field","name":{"kind":"Name","value":"totalParcelCount"}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"zone"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"vehicleId"}},{"kind":"Field","name":{"kind":"Name","value":"vehicle"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"registrationPlate"}}]}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driver"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"user"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"routeStops"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"sequenceNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"arrivalTime"}},{"kind":"Field","name":{"kind":"Name","value":"departureTime"}},{"kind":"Field","name":{"kind":"Name","value":"estimatedServiceMinutes"}},{"kind":"Field","name":{"kind":"Name","value":"accessInstructions"}},{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"parcels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackingNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<GetRouteQuery, GetRouteQueryVariables>;
export const GetAvailableDriversDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetAvailableDrivers"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"date"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DateTime"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"availableDrivers"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"date"},"value":{"kind":"Variable","name":{"kind":"Name","value":"date"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"shift"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"openTime"}},{"kind":"Field","name":{"kind":"Name","value":"closeTime"}}]}},{"kind":"Field","name":{"kind":"Name","value":"assignedRoutes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]}}]}}]} as unknown as DocumentNode<GetAvailableDriversQuery, GetAvailableDriversQueryVariables>;
export const GetRoutesForMapDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetRoutesForMap"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"where"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"RouteFilterInput"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"first"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"routes"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"Variable","name":{"kind":"Name","value":"where"}}},{"kind":"Argument","name":{"kind":"Name","value":"first"},"value":{"kind":"Variable","name":{"kind":"Name","value":"first"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"routeStops"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"sequenceNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"geoLocation"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coordinates"}}]}},{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"parcels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackingNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<GetRoutesForMapQuery, GetRoutesForMapQueryVariables>;
export const CreateRouteDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateRoute"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateRouteCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createRoute"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"plannedStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"zoneName"}},{"kind":"Field","name":{"kind":"Name","value":"estimatedStopCount"}},{"kind":"Field","name":{"kind":"Name","value":"vehicleId"}},{"kind":"Field","name":{"kind":"Name","value":"vehiclePlate"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<CreateRouteMutation, CreateRouteMutationVariables>;
export const UpdateRouteDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateRoute"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateRouteCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateRoute"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"plannedStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"zoneName"}},{"kind":"Field","name":{"kind":"Name","value":"estimatedStopCount"}},{"kind":"Field","name":{"kind":"Name","value":"vehicleId"}},{"kind":"Field","name":{"kind":"Name","value":"vehiclePlate"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<UpdateRouteMutation, UpdateRouteMutationVariables>;
export const DeleteRouteDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteRoute"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteRoute"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteRouteMutation, DeleteRouteMutationVariables>;
export const ChangeRouteStatusDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ChangeRouteStatus"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"newStatus"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"RouteStatus"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"changeRouteStatus"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"newStatus"},"value":{"kind":"Variable","name":{"kind":"Name","value":"newStatus"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"plannedStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"actualStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"actualEndTime"}},{"kind":"Field","name":{"kind":"Name","value":"totalDistanceKm"}},{"kind":"Field","name":{"kind":"Name","value":"totalParcelCount"}},{"kind":"Field","name":{"kind":"Name","value":"vehicleId"}},{"kind":"Field","name":{"kind":"Name","value":"vehiclePlate"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<ChangeRouteStatusMutation, ChangeRouteStatusMutationVariables>;
export const AssignDriverToRouteDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AssignDriverToRoute"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"routeId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"assignDriverToRoute"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"routeId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"routeId"}}},{"kind":"Argument","name":{"kind":"Name","value":"driverId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"driverId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"vehicleId"}},{"kind":"Field","name":{"kind":"Name","value":"vehiclePlate"}}]}}]}}]} as unknown as DocumentNode<AssignDriverToRouteMutation, AssignDriverToRouteMutationVariables>;
export const AddParcelsToRouteDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AddParcelsToRoute"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AddParcelsToRouteCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"addParcelsToRoute"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"totalParcelCount"}},{"kind":"Field","name":{"kind":"Name","value":"estimatedStopCount"}},{"kind":"Field","name":{"kind":"Name","value":"stops"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"sequenceNumber"}},{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"parcelCount"}},{"kind":"Field","name":{"kind":"Name","value":"parcels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackingNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]}}]}}]}}]} as unknown as DocumentNode<AddParcelsToRouteMutation, AddParcelsToRouteMutationVariables>;
export const AutoAssignParcelsByZoneDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AutoAssignParcelsByZone"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"AutoAssignParcelsByZoneCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"autoAssignParcelsByZone"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"totalParcelCount"}},{"kind":"Field","name":{"kind":"Name","value":"estimatedStopCount"}},{"kind":"Field","name":{"kind":"Name","value":"stops"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"sequenceNumber"}},{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"parcelCount"}},{"kind":"Field","name":{"kind":"Name","value":"parcels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackingNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]}}]}}]}}]} as unknown as DocumentNode<AutoAssignParcelsByZoneMutation, AutoAssignParcelsByZoneMutationVariables>;
export const RemoveParcelsFromRouteDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RemoveParcelsFromRoute"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"RemoveParcelsFromRouteCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"removeParcelsFromRoute"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"totalParcelCount"}},{"kind":"Field","name":{"kind":"Name","value":"estimatedStopCount"}},{"kind":"Field","name":{"kind":"Name","value":"stops"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"sequenceNumber"}},{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"parcelCount"}},{"kind":"Field","name":{"kind":"Name","value":"parcels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackingNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]}}]}}]}}]} as unknown as DocumentNode<RemoveParcelsFromRouteMutation, RemoveParcelsFromRouteMutationVariables>;
export const ReorderRouteStopsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ReorderRouteStops"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ReorderRouteStopsCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"reorderRouteStops"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"stops"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"sequenceNumber"}},{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"parcelCount"}}]}}]}}]}}]} as unknown as DocumentNode<ReorderRouteStopsMutation, ReorderRouteStopsMutationVariables>;
export const OptimizeRouteStopOrderDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"OptimizeRouteStopOrder"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"OptimizeRouteStopOrderCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"optimizeRouteStopOrder"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"stops"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"sequenceNumber"}},{"kind":"Field","name":{"kind":"Name","value":"street1"}},{"kind":"Field","name":{"kind":"Name","value":"parcelCount"}}]}}]}}]}}]} as unknown as DocumentNode<OptimizeRouteStopOrderMutation, OptimizeRouteStopOrderMutationVariables>;
export const DispatchRouteDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DispatchRoute"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DispatchRouteCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"dispatchRoute"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"plannedStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"actualStartTime"}},{"kind":"Field","name":{"kind":"Name","value":"actualEndTime"}},{"kind":"Field","name":{"kind":"Name","value":"totalDistanceKm"}},{"kind":"Field","name":{"kind":"Name","value":"totalParcelCount"}},{"kind":"Field","name":{"kind":"Name","value":"vehicleId"}},{"kind":"Field","name":{"kind":"Name","value":"vehiclePlate"}},{"kind":"Field","name":{"kind":"Name","value":"driverId"}},{"kind":"Field","name":{"kind":"Name","value":"driverName"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<DispatchRouteMutation, DispatchRouteMutationVariables>;
export const GetUsersDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetUsers"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"first"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"after"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"where"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UserFilterInput"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"order"}},"type":{"kind":"ListType","type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UserSortInput"}}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"users"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"first"},"value":{"kind":"Variable","name":{"kind":"Name","value":"first"}}},{"kind":"Argument","name":{"kind":"Name","value":"after"},"value":{"kind":"Variable","name":{"kind":"Name","value":"after"}}},{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"Variable","name":{"kind":"Name","value":"where"}}},{"kind":"Argument","name":{"kind":"Name","value":"order"},"value":{"kind":"Variable","name":{"kind":"Name","value":"order"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"email"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"roleId"}},{"kind":"Field","name":{"kind":"Name","value":"role"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"zone"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"depot"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}},{"kind":"Field","name":{"kind":"Name","value":"pageInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"hasNextPage"}},{"kind":"Field","name":{"kind":"Name","value":"hasPreviousPage"}},{"kind":"Field","name":{"kind":"Name","value":"startCursor"}},{"kind":"Field","name":{"kind":"Name","value":"endCursor"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalCount"}}]}}]}}]} as unknown as DocumentNode<GetUsersQuery, GetUsersQueryVariables>;
export const GetUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"user"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"email"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"roleId"}},{"kind":"Field","name":{"kind":"Name","value":"role"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"zone"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"depot"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<GetUserQuery, GetUserQueryVariables>;
export const GetUserManagementLookupsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetUserManagementLookups"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"userManagementLookups"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"roles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}}]}},{"kind":"Field","name":{"kind":"Name","value":"depots"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"zones"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}}]}}]}}]}}]} as unknown as DocumentNode<GetUserManagementLookupsQuery, GetUserManagementLookupsQueryVariables>;
export const CreateUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"firstName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"lastName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"email"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"phone"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"zoneId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"depotId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"password"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"firstName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"firstName"}}},{"kind":"Argument","name":{"kind":"Name","value":"lastName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"lastName"}}},{"kind":"Argument","name":{"kind":"Name","value":"email"},"value":{"kind":"Variable","name":{"kind":"Name","value":"email"}}},{"kind":"Argument","name":{"kind":"Name","value":"phone"},"value":{"kind":"Variable","name":{"kind":"Name","value":"phone"}}},{"kind":"Argument","name":{"kind":"Name","value":"roleId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}}},{"kind":"Argument","name":{"kind":"Name","value":"zoneId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"zoneId"}}},{"kind":"Argument","name":{"kind":"Name","value":"depotId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"depotId"}}},{"kind":"Argument","name":{"kind":"Name","value":"password"},"value":{"kind":"Variable","name":{"kind":"Name","value":"password"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"email"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"roleId"}},{"kind":"Field","name":{"kind":"Name","value":"roleName"}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"zoneName"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"depotName"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<CreateUserMutation, CreateUserMutationVariables>;
export const UpdateUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"firstName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"lastName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"email"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"phone"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"zoneId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"depotId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}},{"kind":"Argument","name":{"kind":"Name","value":"firstName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"firstName"}}},{"kind":"Argument","name":{"kind":"Name","value":"lastName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"lastName"}}},{"kind":"Argument","name":{"kind":"Name","value":"email"},"value":{"kind":"Variable","name":{"kind":"Name","value":"email"}}},{"kind":"Argument","name":{"kind":"Name","value":"phone"},"value":{"kind":"Variable","name":{"kind":"Name","value":"phone"}}},{"kind":"Argument","name":{"kind":"Name","value":"roleId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"roleId"}}},{"kind":"Argument","name":{"kind":"Name","value":"zoneId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"zoneId"}}},{"kind":"Argument","name":{"kind":"Name","value":"depotId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"depotId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"email"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"roleId"}},{"kind":"Field","name":{"kind":"Name","value":"roleName"}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"zoneName"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"depotName"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<UpdateUserMutation, UpdateUserMutationVariables>;
export const DeactivateUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeactivateUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deactivateUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"email"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"roleId"}},{"kind":"Field","name":{"kind":"Name","value":"roleName"}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"zoneName"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"depotName"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<DeactivateUserMutation, DeactivateUserMutationVariables>;
export const ActivateUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ActivateUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"activateUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstName"}},{"kind":"Field","name":{"kind":"Name","value":"lastName"}},{"kind":"Field","name":{"kind":"Name","value":"email"}},{"kind":"Field","name":{"kind":"Name","value":"phoneNumber"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"roleId"}},{"kind":"Field","name":{"kind":"Name","value":"roleName"}},{"kind":"Field","name":{"kind":"Name","value":"zoneId"}},{"kind":"Field","name":{"kind":"Name","value":"zoneName"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"depotName"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<ActivateUserMutation, ActivateUserMutationVariables>;
export const ResetPasswordDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ResetPassword"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"email"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"resetPassword"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"email"},"value":{"kind":"Variable","name":{"kind":"Name","value":"email"}}}]}]}}]} as unknown as DocumentNode<ResetPasswordMutation, ResetPasswordMutationVariables>;
export const GetVehiclesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetVehicles"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"where"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"VehicleFilterInput"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"vehicles"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"where"},"value":{"kind":"Variable","name":{"kind":"Name","value":"where"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"registrationPlate"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}}]}}]}}]} as unknown as DocumentNode<GetVehiclesQuery, GetVehiclesQueryVariables>;
export const GetVehicleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetVehicle"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"vehicle"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"registrationPlate"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"parcelCapacity"}},{"kind":"Field","name":{"kind":"Name","value":"weightCapacityKg"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"depot"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<GetVehicleQuery, GetVehicleQueryVariables>;
export const GetVehicleHistoryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetVehicleHistory"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"vehicleHistory"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"registrationPlate"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"totalMileageKm"}},{"kind":"Field","name":{"kind":"Name","value":"totalRoutesCompleted"}},{"kind":"Field","name":{"kind":"Name","value":"routes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"routeId"}},{"kind":"Field","name":{"kind":"Name","value":"routeName"}},{"kind":"Field","name":{"kind":"Name","value":"completedAt"}},{"kind":"Field","name":{"kind":"Name","value":"distanceKm"}},{"kind":"Field","name":{"kind":"Name","value":"parcelCount"}}]}}]}}]}}]} as unknown as DocumentNode<GetVehicleHistoryQuery, GetVehicleHistoryQueryVariables>;
export const CreateVehicleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateVehicle"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"registrationPlate"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"type"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"VehicleType"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"parcelCapacity"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"weightCapacityKg"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Decimal"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"depotId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createVehicle"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"registrationPlate"},"value":{"kind":"Variable","name":{"kind":"Name","value":"registrationPlate"}}},{"kind":"Argument","name":{"kind":"Name","value":"type"},"value":{"kind":"Variable","name":{"kind":"Name","value":"type"}}},{"kind":"Argument","name":{"kind":"Name","value":"parcelCapacity"},"value":{"kind":"Variable","name":{"kind":"Name","value":"parcelCapacity"}}},{"kind":"Argument","name":{"kind":"Name","value":"weightCapacityKg"},"value":{"kind":"Variable","name":{"kind":"Name","value":"weightCapacityKg"}}},{"kind":"Argument","name":{"kind":"Name","value":"depotId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"depotId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"registrationPlate"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"parcelCapacity"}},{"kind":"Field","name":{"kind":"Name","value":"weightCapacityKg"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<CreateVehicleMutation, CreateVehicleMutationVariables>;
export const UpdateVehicleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateVehicle"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"registrationPlate"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"type"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"VehicleType"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"parcelCapacity"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"weightCapacityKg"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Decimal"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"depotId"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateVehicle"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"registrationPlate"},"value":{"kind":"Variable","name":{"kind":"Name","value":"registrationPlate"}}},{"kind":"Argument","name":{"kind":"Name","value":"type"},"value":{"kind":"Variable","name":{"kind":"Name","value":"type"}}},{"kind":"Argument","name":{"kind":"Name","value":"parcelCapacity"},"value":{"kind":"Variable","name":{"kind":"Name","value":"parcelCapacity"}}},{"kind":"Argument","name":{"kind":"Name","value":"weightCapacityKg"},"value":{"kind":"Variable","name":{"kind":"Name","value":"weightCapacityKg"}}},{"kind":"Argument","name":{"kind":"Name","value":"depotId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"depotId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"registrationPlate"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"parcelCapacity"}},{"kind":"Field","name":{"kind":"Name","value":"weightCapacityKg"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<UpdateVehicleMutation, UpdateVehicleMutationVariables>;
export const DeleteVehicleDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteVehicle"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteVehicle"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteVehicleMutation, DeleteVehicleMutationVariables>;
export const ChangeVehicleStatusDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ChangeVehicleStatus"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"newStatus"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"VehicleStatus"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"changeVehicleStatus"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}},{"kind":"Argument","name":{"kind":"Name","value":"newStatus"},"value":{"kind":"Variable","name":{"kind":"Name","value":"newStatus"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"registrationPlate"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"parcelCapacity"}},{"kind":"Field","name":{"kind":"Name","value":"weightCapacityKg"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<ChangeVehicleStatusMutation, ChangeVehicleStatusMutationVariables>;
export const GetZonesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetZones"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"zones"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"nodes"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"boundaryGeometry"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"depot"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"isActive"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]}}]} as unknown as DocumentNode<GetZonesQuery, GetZonesQueryVariables>;
export const GetZoneDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"GetZone"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"zone"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"boundaryGeometry"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"depot"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"isActive"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"lastModifiedAt"}}]}}]}}]} as unknown as DocumentNode<GetZoneQuery, GetZoneQueryVariables>;
export const CreateZoneDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateZone"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"CreateZoneCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createZone"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"isActive"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<CreateZoneMutation, CreateZoneMutationVariables>;
export const UpdateZoneDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateZone"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateZoneCommandInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateZone"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"depotId"}},{"kind":"Field","name":{"kind":"Name","value":"isActive"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]} as unknown as DocumentNode<UpdateZoneMutation, UpdateZoneMutationVariables>;
export const DeleteZoneDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteZone"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"id"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteZone"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"id"}}}]}]}}]} as unknown as DocumentNode<DeleteZoneMutation, DeleteZoneMutationVariables>;