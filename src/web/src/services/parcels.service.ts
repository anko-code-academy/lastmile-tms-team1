import { apiFetch } from "@/lib/api";
import type { ParcelSummaryDto, ParcelDto, CreateParcelInput } from "@/lib/graphql/types";

const GET_PARCELS_QUERY = `
  query GetParcels {
    parcels {
      nodes {
        id
        trackingNumber
        serviceType
        status
        createdAt
      }
    }
  }
`;

const GET_PARCEL_QUERY = `
  query GetParcel($id: UUID!) {
    parcel(id: $id) {
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
    }
  }
`;

interface ParcelsResponse {
  parcels: {
    nodes: ParcelSummaryDto[];
  };
}

interface ParcelResponse {
  parcel: ParcelDto | null;
}

interface CreateParcelResponse {
  createParcel: ParcelSummaryDto;
}

export async function fetchParcels(token: string): Promise<ParcelSummaryDto[]> {
  const response = await apiFetch<{ data: ParcelsResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: GET_PARCELS_QUERY,
    }),
  });
  return response.data.parcels.nodes;
}

export async function fetchParcel(token: string, id: string): Promise<ParcelDto | null> {
  const response = await apiFetch<{ data: ParcelResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: GET_PARCEL_QUERY,
      variables: { id },
    }),
  });
  return response.data.parcel;
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
