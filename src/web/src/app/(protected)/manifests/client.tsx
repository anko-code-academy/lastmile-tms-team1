"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { ManifestStatus } from "@/graphql/generated/graphql";
import { ManifestTable } from "@/components/manifests/manifest-table";
import { useManifests } from "@/hooks/use-manifests";
import { useDepots } from "@/hooks/use-depots";
import { Button } from "@/components/ui/button";

const selectClass =
  "flex w-[200px] items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50";

export function ManifestListClient() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const statusParam = searchParams.get("status") as ManifestStatus | null;
  const depotParam = searchParams.get("depotId") as string | null;
  const cursorParam = searchParams.get("cursor") as string | null;

  const [afterCursor, setAfterCursor] = useState<string | null>(cursorParam);
  const [cursorHistory, setCursorHistory] = useState<(string | null)[]>([]);

  const filters = {
    status: statusParam ?? undefined,
    depotId: depotParam ?? undefined,
    first: 25,
    after: afterCursor || undefined,
  };

  const { data, isLoading, error } = useManifests(filters);
  const { data: depots } = useDepots();

  const nodes = data?.nodes ?? [];
  const pageInfo = data?.pageInfo;

  const handleStatusFilter = (value: string) => {
    const params = new URLSearchParams(searchParams.toString());
    if (value === "all") params.delete("status");
    else params.set("status", value);
    params.delete("cursor");
    setAfterCursor(null);
    setCursorHistory([]);
    router.push(`/manifests?${params.toString()}`);
  };

  const handleDepotFilter = (value: string) => {
    const params = new URLSearchParams(searchParams.toString());
    if (value === "all") params.delete("depotId");
    else params.set("depotId", value);
    params.delete("cursor");
    setAfterCursor(null);
    setCursorHistory([]);
    router.push(`/manifests?${params.toString()}`);
  };

  const handleClearFilters = () => {
    setAfterCursor(null);
    setCursorHistory([]);
    router.push("/manifests");
  };

  const handleNext = () => {
    if (pageInfo?.endCursor) {
      setCursorHistory((prev) => [...prev, afterCursor]);
      setAfterCursor(pageInfo.endCursor);
    }
  };

  const handlePrevious = () => {
    const prev = cursorHistory[cursorHistory.length - 1] ?? null;
    setCursorHistory((h) => h.slice(0, -1));
    setAfterCursor(prev);
  };

  const hasFilters = statusParam || depotParam;

  if (isLoading) {
    return <div className="py-8 text-center text-muted-foreground">Loading manifests...</div>;
  }

  if (error) {
    return <div className="py-8 text-center text-destructive">Failed to load manifests</div>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-4">
        <select
          className={selectClass}
          value={statusParam ?? "all"}
          onChange={(e) => handleStatusFilter(e.target.value)}
        >
          <option value="all">All Statuses</option>
          <option value={ManifestStatus.Open}>Open</option>
          <option value={ManifestStatus.Receiving}>Receiving</option>
          <option value={ManifestStatus.Completed}>Completed</option>
        </select>

        <select
          className={selectClass}
          value={depotParam ?? "all"}
          onChange={(e) => handleDepotFilter(e.target.value)}
        >
          <option value="all">All Depots</option>
          {(depots ?? []).map((depot) => (
            <option key={depot.id} value={depot.id}>
              {depot.name}
            </option>
          ))}
        </select>

        {hasFilters && (
          <Button variant="ghost" size="sm" onClick={handleClearFilters}>
            Clear filters
          </Button>
        )}

        {data?.totalCount !== undefined && (
          <span className="text-sm text-muted-foreground ml-auto">
            {data.totalCount} total
          </span>
        )}
      </div>

      <ManifestTable data={nodes} />

      {pageInfo && (pageInfo.hasNextPage || pageInfo.hasPreviousPage) && (
        <div className="flex justify-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={handlePrevious}
            disabled={!pageInfo.hasPreviousPage}
          >
            Previous
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleNext}
            disabled={!pageInfo.hasNextPage}
          >
            Next
          </Button>
        </div>
      )}
    </div>
  );
}
