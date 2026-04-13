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

export interface ParcelSummaryDto {
  id: string;
  trackingNumber: string;
  description?: string;
  status: ParcelStatus;
  serviceType: string;
  weight?: number;
  weightUnit?: string;
  length?: number;
  width?: number;
  height?: number;
  dimensionUnit?: string;
  declaredValue?: number;
  currency?: string;
  parcelType?: string;
  estimatedDeliveryDate?: string;
  actualDeliveryDate?: string;
  deliveryAttempts?: number;
  createdAt: string;
  recipientAddress?: AddressDto;
  shipperAddress?: AddressDto;
  zone?: { id: string; name: string };
  depot?: { id: string; name: string };
}

export enum ParcelStatus {
  Registered = "REGISTERED",
  ReceivedAtDepot = "RECEIVED_AT_DEPOT",
  Sorted = "SORTED",
  Staged = "STAGED",
  Loaded = "LOADED",
  OutForDelivery = "OUT_FOR_DELIVERY",
  Delivered = "DELIVERED",
  FailedAttempt = "FAILED_ATTEMPT",
  ReturnedToDepot = "RETURNED_TO_DEPOT",
  Cancelled = "CANCELLED",
  Exception = "EXCEPTION",
}

export interface PageInfo {
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  startCursor?: string;
  endCursor?: string;
}

export interface ParcelsConnection {
  nodes: ParcelSummaryDto[];
  pageInfo: PageInfo;
  totalCount: number;
}

export interface ParcelsResponse {
  parcels: ParcelsConnection;
}
