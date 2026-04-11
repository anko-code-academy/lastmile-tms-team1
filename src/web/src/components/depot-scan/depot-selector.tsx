"use client";

import { useSession } from "next-auth/react";
import { depotKeys } from "@/lib/query-key-factory";
import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api";

interface Depot {
  id: string;
  name: string;
  isActive: boolean;
}

async function fetchDepots(token: string): Promise<Depot[]> {
  const response = await apiFetch<{ data: { depots: { nodes: Depot[] } } }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: `query { depots(first: 50) { nodes { id name isActive } } }`,
    }),
  });
  return response.data.depots.nodes.filter((d) => d.isActive);
}

interface DepotSelectorProps {
  value: string | null;
  onChange: (depotId: string) => void;
}

export function DepotSelector({ value, onChange }: DepotSelectorProps) {
  const { data: session } = useSession();

  const { data: depots, isLoading } = useQuery({
    queryKey: depotKeys.lists(),
    queryFn: () => fetchDepots(session!.user.accessToken),
    enabled: !!session?.user?.accessToken,
  });

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">Loading depots...</p>;
  }

  return (
    <select
      value={value ?? ""}
      onChange={(e) => onChange(e.target.value)}
      className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
    >
      <option value="" disabled>
        Select a depot...
      </option>
      {depots?.map((depot) => (
        <option key={depot.id} value={depot.id}>
          {depot.name}
        </option>
      ))}
    </select>
  );
}
