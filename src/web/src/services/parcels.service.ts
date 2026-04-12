import { print } from "graphql";
import { apiFetch } from "@/lib/api";
import type { ParcelsResponse } from "@/types/parcel";
import { buildParcelsQuery } from "@/lib/build-parcels-query";
import {
  type ColumnKey,
  DEFAULT_COLUMNS,
} from "@/components/parcel/column-registry";
import {
  GetParcelByTrackingNumberDocument,
  GetTrackingEventsDocument,
  GetParcelAuditLogsDocument,
  CreateParcelDocument,
  UpdateParcelDocument,
  CancelParcelDocument,
  ChangeParcelStatusDocument,
  type GetParcelByTrackingNumberQuery,
  type GetTrackingEventsQuery,
  type GetParcelAuditLogsQuery,
  type CreateParcelMutation,
  type UpdateParcelMutation,
  type CancelParcelMutation,
  type ChangeParcelStatusMutation,
  type CreateParcelCommandInput,
  type UpdateParcelCommandInput,
  type CancelParcelCommandInput,
  type ChangeParcelStatusCommandInput,
} from "@/graphql/generated/graphql";

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
    token,
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

export async function fetchParcel(
  token: string,
  trackingNumber: string,
): Promise<GetParcelByTrackingNumberQuery["parcelByTrackingNumber"]> {
  const response = await apiFetch<{ data: GetParcelByTrackingNumberQuery }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(GetParcelByTrackingNumberDocument),
      variables: { trackingNumber },
    }),
  });
  return response.data.parcelByTrackingNumber;
}

export async function fetchTrackingEvents(
  token: string,
  parcelId: string,
  first: number = 50,
  after?: string,
): Promise<GetTrackingEventsQuery["trackingEvents"]> {
  const response = await apiFetch<{ data: GetTrackingEventsQuery }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(GetTrackingEventsDocument),
      variables: {
        parcelId,
        first,
        after: after || null,
        order: [{ timestamp: "DESC" }],
      },
    }),
  });
  return response.data.trackingEvents;
}

export async function fetchParcelAuditLogs(
  token: string,
  parcelId: string,
  first: number = 25,
  after?: string,
): Promise<GetParcelAuditLogsQuery["parcelAuditLogs"]> {
  const response = await apiFetch<{ data: GetParcelAuditLogsQuery }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(GetParcelAuditLogsDocument),
      variables: {
        parcelId,
        first,
        after: after || null,
        order: [{ createdAt: "DESC" }],
      },
    }),
  });
  return response.data.parcelAuditLogs;
}

export async function createParcel(
  token: string,
  input: CreateParcelCommandInput,
): Promise<CreateParcelMutation["createParcel"]> {
  const response = await apiFetch<{ data: CreateParcelMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(CreateParcelDocument),
      variables: { input },
    }),
  });
  return response.data.createParcel;
}

export async function updateParcel(
  token: string,
  input: UpdateParcelCommandInput,
): Promise<UpdateParcelMutation["updateParcel"]> {
  const response = await apiFetch<{ data: UpdateParcelMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(UpdateParcelDocument),
      variables: { input },
    }),
  });
  return response.data.updateParcel;
}

export async function cancelParcel(
  token: string,
  input: CancelParcelCommandInput,
): Promise<CancelParcelMutation["cancelParcel"]> {
  const response = await apiFetch<{ data: CancelParcelMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(CancelParcelDocument),
      variables: { input },
    }),
  });
  return response.data.cancelParcel;
}

export async function changeParcelStatus(
  token: string,
  input: ChangeParcelStatusCommandInput,
): Promise<ChangeParcelStatusMutation["changeParcelStatus"]> {
  const response = await apiFetch<{ data: ChangeParcelStatusMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(ChangeParcelStatusDocument),
      variables: { input },
    }),
  });
  return response.data.changeParcelStatus;
}
