import { apiFetch } from "@/lib/api";
import type { DepotSummaryDto, DepotDto } from "@/lib/graphql/types";

const GET_DEPOTS_QUERY = `
  query GetDepots {
    depots {
      nodes {
        id
        name
        address {
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
        isActive
        createdAt
      }
    }
  }
`;

const GET_DEPOT_QUERY = `
  query GetDepot($id: UUID!) {
    depot(id: $id) {
      id
      name
      address {
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
      shiftSchedules {
        dayOfWeek
        openTime
        closeTime
      }
      isActive
      zones {
        id
      }
      createdAt
    }
  }
`;

const CREATE_DEPOT_MUTATION = `
  mutation CreateDepot($input: CreateDepotCommandInput!) {
    createDepot(input: $input) {
      id
      name
      isActive
      createdAt
    }
  }
`;

const UPDATE_DEPOT_MUTATION = `
  mutation UpdateDepot($input: UpdateDepotCommandInput!) {
    updateDepot(input: $input) {
      id
      name
      isActive
      createdAt
    }
  }
`;

const DELETE_DEPOT_MUTATION = `
  mutation DeleteDepot($id: UUID!) {
    deleteDepot(id: $id)
  }
`;

interface DepotsResponse {
  depots: {
    nodes: DepotSummaryDto[];
  };
}

interface DepotResponse {
  depot: DepotDto | null;
}

export async function fetchDepots(token: string): Promise<DepotSummaryDto[]> {
  const response = await apiFetch<{ data: DepotsResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: GET_DEPOTS_QUERY,
    }),
  });
  return response.data.depots.nodes;
}

export async function fetchDepot(token: string, id: string): Promise<DepotDto | null> {
  const response = await apiFetch<{ data: DepotResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: GET_DEPOT_QUERY,
      variables: { id },
    }),
  });
  return response.data.depot;
}

export async function createDepot(token: string, input: {
  name: string;
  address: {
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
  };
  operatingHours?: Array<{
    dayOfWeek: string;
    openTime: string | null;
    closeTime: string | null;
  }>;
  isActive?: boolean;
}): Promise<DepotSummaryDto> {
  const response = await apiFetch<{
    data: { createDepot: DepotSummaryDto };
  }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: CREATE_DEPOT_MUTATION,
      variables: { input },
    }),
  });
  return response.data.createDepot;
}

export async function updateDepot(token: string, input: {
  id: string;
  name: string;
  address: {
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
  };
  operatingHours?: Array<{
    dayOfWeek: string;
    openTime: string | null;
    closeTime: string | null;
  }>;
  isActive: boolean;
}): Promise<DepotSummaryDto> {
  const response = await apiFetch<{
    data: { updateDepot: DepotSummaryDto };
  }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: UPDATE_DEPOT_MUTATION,
      variables: { input },
    }),
  });
  return response.data.updateDepot;
}

export async function deleteDepot(token: string, id: string): Promise<boolean> {
  const response = await apiFetch<{ data: { deleteDepot: boolean } }>(
    "/api/graphql",
    {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: JSON.stringify({
        query: DELETE_DEPOT_MUTATION,
        variables: { id },
      }),
    }
  );
  return response.data.deleteDepot;
}
