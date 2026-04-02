import { apiFetch } from "@/lib/api";
import type { ZoneSummaryDto, ZoneDto } from "@/lib/graphql/types";

const GET_ZONES_QUERY = `
  query GetZones {
    zones {
      nodes {
        id
        name
        boundaryGeometry
        depotId
        depot {
          name
        }
        isActive
        createdAt
      }
    }
  }
`;

const GET_ZONE_QUERY = `
  query GetZone($id: UUID!) {
    zone(id: $id) {
      id
      name
      boundaryGeometry
      depotId
      depot {
        name
      }
      isActive
      createdAt
      lastModifiedAt
    }
  }
`;

const CREATE_ZONE_MUTATION = `
  mutation CreateZone($input: CreateZoneCommandInput!) {
    createZone(input: $input) {
      id
      name
      depotId
      isActive
      createdAt
    }
  }
`;

const UPDATE_ZONE_MUTATION = `
  mutation UpdateZone($input: UpdateZoneCommandInput!) {
    updateZone(input: $input) {
      id
      name
      depotId
      isActive
      createdAt
    }
  }
`;

const DELETE_ZONE_MUTATION = `
  mutation DeleteZone($id: UUID!) {
    deleteZone(id: $id)
  }
`;

interface ZonesResponse {
  zones: {
    nodes: ZoneSummaryDto[];
  };
}

interface ZoneResponse {
  zone: ZoneDto | null;
}

export async function fetchZones(token: string): Promise<ZoneSummaryDto[]> {
  const response = await apiFetch<{ data: ZonesResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: GET_ZONES_QUERY,
    }),
  });
  return response.data.zones.nodes;
}

export async function fetchZone(token: string, id: string): Promise<ZoneDto | null> {
  const response = await apiFetch<{ data: ZoneResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: GET_ZONE_QUERY,
      variables: { id },
    }),
  });
  return response.data.zone;
}

export async function createZone(token: string, input: {
  name: string;
  geoJson: string;
  depotId: string;
  isActive?: boolean;
}): Promise<ZoneSummaryDto> {
  const response = await apiFetch<{
    data: { createZone: ZoneSummaryDto };
  }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: CREATE_ZONE_MUTATION,
      variables: { input },
    }),
  });
  return response.data.createZone;
}

export async function updateZone(token: string, input: {
  id: string;
  name: string;
  geoJson?: string;
  depotId: string;
  isActive: boolean;
}): Promise<ZoneSummaryDto> {
  const response = await apiFetch<{
    data: { updateZone: ZoneSummaryDto };
  }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: UPDATE_ZONE_MUTATION,
      variables: { input },
    }),
  });
  return response.data.updateZone;
}

export async function deleteZone(token: string, id: string): Promise<boolean> {
  const response = await apiFetch<{ data: { deleteZone: boolean } }>(
    "/api/graphql",
    {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: JSON.stringify({
        query: DELETE_ZONE_MUTATION,
        variables: { id },
      }),
    }
  );
  return response.data.deleteZone;
}
