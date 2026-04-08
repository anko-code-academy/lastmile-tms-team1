"use client";

import { useSearchParams, useRouter, usePathname } from "next/navigation";
import { useCallback, useMemo } from "react";
import {
  type ColumnKey,
  DEFAULT_COLUMNS,
  COLUMN_REGISTRY,
} from "@/components/parcel/column-registry";

const VALID_KEYS = new Set<string>(COLUMN_REGISTRY.map((c) => c.key));
const ALWAYS_ON_KEYS = new Set<string>(
  COLUMN_REGISTRY.filter((c) => c.alwaysOn).map((c) => c.key),
);

export function useColumnParams() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();

  const columnsParam = searchParams.get("columns");

  const appliedColumns: ColumnKey[] = useMemo(() => {
    if (!columnsParam) return DEFAULT_COLUMNS;
    const parsed = columnsParam
      .split(",")
      .filter((k): k is ColumnKey => VALID_KEYS.has(k));
    if (parsed.length === 0) return DEFAULT_COLUMNS;
    for (const key of ALWAYS_ON_KEYS) {
      if (!parsed.includes(key as ColumnKey)) {
        parsed.unshift(key as ColumnKey);
      }
    }
    return parsed;
  }, [columnsParam]);

  const applyColumns = useCallback(
    (keys: ColumnKey[]) => {
      const params = new URLSearchParams(searchParams.toString());
      const sortedDefault = [...DEFAULT_COLUMNS].sort().join(",");
      const sortedNew = [...keys].sort().join(",");
      if (sortedNew === sortedDefault) {
        params.delete("columns");
      } else {
        params.set("columns", keys.join(","));
      }
      const qs = params.toString();
      router.push(qs ? `${pathname}?${qs}` : pathname);
    },
    [searchParams, router, pathname],
  );

  return { appliedColumns, applyColumns };
}
