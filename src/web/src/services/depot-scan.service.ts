import { print } from "graphql";
import { apiFetch } from "@/lib/api";
import {
  ScanParcelDocument,
  type ScanParcelMutation,
  type ScanParcelCommandInput,
} from "@/graphql/generated/graphql";

export async function scanParcel(
  token: string,
  input: ScanParcelCommandInput
): Promise<ScanParcelMutation["scanParcel"]> {
  const response = await apiFetch<{ data: ScanParcelMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(ScanParcelDocument),
      variables: { input },
    }),
  });
  return response.data.scanParcel;
}
