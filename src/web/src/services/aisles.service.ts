import { apiFetch } from "@/lib/api";
import type { AisleDto, AisleResult } from "@/lib/graphql/types";

const GET_AISLES_QUERY = `
  query GetAisles($zoneId: UUID!, $order: [AisleSortInput!]) {
    aisles(where: { zoneId: { eq: $zoneId } }, order: $order) {
      edges {
        node {
          id
          name
          label
          order
          isActive
          zoneId
        }
      }
    }
  }
`;

const CREATE_AISLE_MUTATION = `
  mutation CreateAisle($input: CreateAisleCommandInput!) {
    createAisle(input: $input) {
      id
      name
      label
      order
      isActive
      zoneId
      zoneName
      createdAt
    }
  }
`;

const UPDATE_AISLE_MUTATION = `
  mutation UpdateAisle($input: UpdateAisleCommandInput!) {
    updateAisle(input: $input) {
      id
      name
      label
      order
      isActive
      zoneId
      zoneName
      createdAt
    }
  }
`;

const DELETE_AISLE_MUTATION = `
  mutation DeleteAisle($id: UUID!) {
    deleteAisle(id: $id)
  }
`;

interface AislesResponse {
  aisles: {
    edges: Array<{ node: AisleDto }>;
  };
}

export async function fetchAislesByZone(
  token: string,
  zoneId: string
): Promise<AisleDto[]> {
  const response = await apiFetch<{ data: AislesResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: GET_AISLES_QUERY,
      variables: { zoneId, order: [{ name: "ASC" }] },
    }),
  });
  return response.data.aisles.edges.map((e) => e.node);
}

export async function createAisle(
  token: string,
  input: { name: string; zoneId: string; order?: number; isActive?: boolean }
): Promise<AisleResult> {
  const response = await apiFetch<{
    data: { createAisle: AisleResult };
  }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: CREATE_AISLE_MUTATION,
      variables: { input },
    }),
  });
  return response.data.createAisle;
}

export async function updateAisle(
  token: string,
  input: { id: string; name: string; zoneId: string; order: number; isActive: boolean }
): Promise<AisleResult> {
  const response = await apiFetch<{
    data: { updateAisle: AisleResult };
  }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({
      query: UPDATE_AISLE_MUTATION,
      variables: { input },
    }),
  });
  return response.data.updateAisle;
}

export async function deleteAisle(token: string, id: string): Promise<boolean> {
  const response = await apiFetch<{ data: { deleteAisle: boolean } }>(
    "/api/graphql",
    {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: JSON.stringify({
        query: DELETE_AISLE_MUTATION,
        variables: { id },
      }),
    }
  );
  return response.data.deleteAisle;
}
