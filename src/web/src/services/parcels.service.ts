import { apiFetch } from "@/lib/api";
import type { ParcelSummaryDto, ParcelDto, CreateParcelInput } from "@/lib/graphql/types";
import type { ParcelsResponse } from "@/types/parcel";
import { buildParcelsQuery } from "@/lib/build-parcels-query";
import {
  type ColumnKey,
  DEFAULT_COLUMNS,
} from "@/components/parcel/column-registry";

const GET_PARCEL_QUERY = `
  query GetParcelByTrackingNumber($trackingNumber: String!) {
    parcelByTrackingNumber(trackingNumber: $trackingNumber) {
      id
      trackingNumber
      description
      serviceType
      status
      weight
      weightUnit
      length
      width
      height
      dimensionUnit
      declaredValue
      currency
      estimatedDeliveryDate
      actualDeliveryDate
      deliveryAttempts
      parcelType
      notes
      shipperAddress {
        street1
        street2
        city
        state
        postalCode
        countryCode
        isResidential
        contactName
        companyName
        phone
        email
      }
      recipientAddress {
        street1
        street2
        city
        state
        postalCode
        countryCode
        isResidential
        contactName
        companyName
        phone
        email
      }
      zone {
        id
        name
      }
      createdAt
      lastModifiedAt
    }
  }
`;

const CREATE_PARCEL_MUTATION = `
  mutation CreateParcel($input: CreateParcelCommandInput!) {
    createParcel(input: $input) {
      id
      trackingNumber
      status
      serviceType
      createdAt
      estimatedDeliveryDate
    }
  }
`;

interface ParcelResponse {
  parcelByTrackingNumber: ParcelDto | null;
}

interface CreateParcelResponse {
  createParcel: ParcelSummaryDto;
}

export interface FetchParcelsFilters {
  recipientSearch?: string;
  addressSearch?: string;
  trackingNumber?: string;
  status?: string;
  serviceType?: string;
  parcelType?: string;
  zoneId?: string;
  first?: number;
  after?: string;
  order?: Record<string, string>;
  createdAfter?: string;
  createdBefore?: string;
  estimatedDeliveryAfter?: string;
  estimatedDeliveryBefore?: string;
  actualDeliveryAfter?: string;
  actualDeliveryBefore?: string;
  columns?: ColumnKey[];
}

function addDateRange(
  where: Record<string, unknown>,
  field: string,
  after?: string,
  before?: string,
) {
  if (after || before) {
    const range: Record<string, string> = {};
    if (after) range.gte = after;
    if (before) range.lte = before;
    where[field] = range;
  }
}

export async function fetchParcels(
  token: string,
  filters?: FetchParcelsFilters,
): Promise<ParcelsResponse["parcels"]> {
  const where: Record<string, unknown> = {};

  if (filters?.trackingNumber) {
    where.trackingNumber = { contains: filters.trackingNumber };
  }
  if (filters?.status) {
    where.status = { eq: filters.status };
  }
  if (filters?.serviceType) {
    where.serviceType = { eq: filters.serviceType };
  }
  if (filters?.parcelType) {
    where.parcelType = { eq: filters.parcelType };
  }
  if (filters?.zoneId) {
    where.zoneId = { eq: filters.zoneId };
  }

  addDateRange(where, "createdAt", filters?.createdAfter, filters?.createdBefore);
  addDateRange(
    where,
    "estimatedDeliveryDate",
    filters?.estimatedDeliveryAfter,
    filters?.estimatedDeliveryBefore,
  );
  addDateRange(
    where,
    "actualDeliveryDate",
    filters?.actualDeliveryAfter,
    filters?.actualDeliveryBefore,
  );

  const order = filters?.order
    ? Object.entries(filters.order).map(([field, direction]) => ({
        [field]: direction,
      }))
    : undefined;

  const query = buildParcelsQuery(filters?.columns ?? DEFAULT_COLUMNS);

  const response = await apiFetch<{ data: ParcelsResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query,
      variables: {
        recipientSearch: filters?.recipientSearch || null,
        addressSearch: filters?.addressSearch || null,
        where: Object.keys(where).length > 0 ? where : null,
        order: order ?? null,
        first: filters?.first ?? 25,
        after: filters?.after || null,
      },
    }),
  });
  return response.data.parcels;
}

export async function fetchParcel(token: string, trackingNumber: string): Promise<ParcelDto | null> {
  const response = await apiFetch<{ data: ParcelResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: GET_PARCEL_QUERY,
      variables: { trackingNumber },
    }),
  });
  return response.data.parcelByTrackingNumber;
}

export async function createParcel(
  token: string,
  input: CreateParcelInput
): Promise<ParcelSummaryDto> {
  const response = await apiFetch<{ data: CreateParcelResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: CREATE_PARCEL_MUTATION,
      variables: { input },
    }),
  });
  return response.data.createParcel;
}
