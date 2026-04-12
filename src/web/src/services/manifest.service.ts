import { print } from "graphql";
import { apiFetch } from "@/lib/api";
import {
  GetManifestsDocument,
  GetManifestDocument,
  CreateManifestDocument,
  ReceiveParcelDocument,
  CompleteManifestReceivingDocument,
  type GetManifestsQuery,
  type GetManifestQuery,
  type CreateManifestMutation,
  type ReceiveParcelMutation,
  type CompleteManifestReceivingMutation,
  type CreateManifestCommandInput,
  type ReceiveParcelCommandInput,
  type CompleteManifestReceivingCommandInput,
  type ManifestStatus,
} from "@/graphql/generated/graphql";

export interface FetchManifestsFilters {
  status?: ManifestStatus | ManifestStatus[];
  depotId?: string;
  first?: number;
  after?: string;
}

export async function fetchManifests(
  token: string,
  filters?: FetchManifestsFilters
): Promise<GetManifestsQuery["manifests"]> {
  const where: Record<string, unknown> = {};
  if (filters?.status) {
    where.status = Array.isArray(filters.status)
      ? { in: filters.status }
      : { eq: filters.status };
  }
  if (filters?.depotId) {
    where.depotId = { eq: filters.depotId };
  }

  const response = await apiFetch<{ data: GetManifestsQuery }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(GetManifestsDocument),
      variables: {
        where: Object.keys(where).length > 0 ? where : undefined,
        first: filters?.first ?? 25,
        after: filters?.after || null,
      },
    }),
  });
  return response.data.manifests;
}

export async function fetchManifest(
  token: string,
  id: string
): Promise<GetManifestQuery["manifest"]> {
  const response = await apiFetch<{ data: GetManifestQuery }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(GetManifestDocument),
      variables: { id },
    }),
  });
  return response.data.manifest;
}

export async function createManifest(
  token: string,
  input: CreateManifestCommandInput
): Promise<CreateManifestMutation["createManifest"]> {
  const response = await apiFetch<{ data: CreateManifestMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(CreateManifestDocument),
      variables: { input },
    }),
  });
  return response.data.createManifest;
}

export async function receiveParcel(
  token: string,
  input: ReceiveParcelCommandInput
): Promise<ReceiveParcelMutation["receiveParcel"]> {
  const response = await apiFetch<{ data: ReceiveParcelMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(ReceiveParcelDocument),
      variables: { input },
    }),
  });
  return response.data.receiveParcel;
}

export async function completeManifestReceiving(
  token: string,
  input: CompleteManifestReceivingCommandInput
): Promise<CompleteManifestReceivingMutation["completeManifestReceiving"]> {
  const response = await apiFetch<{ data: CompleteManifestReceivingMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(CompleteManifestReceivingDocument),
      variables: { input },
    }),
  });
  return response.data.completeManifestReceiving;
}
