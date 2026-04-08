"use client";

import { useState, useMemo } from "react";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Settings2 } from "lucide-react";
import {
  COLUMN_REGISTRY,
  DEFAULT_COLUMNS,
  type ColumnKey,
} from "./column-registry";

interface ColumnPickerProps {
  appliedColumns: ColumnKey[];
  onApply: (columns: ColumnKey[]) => void;
}

const GROUP_LABELS: Record<string, string> = {
  parcel: "Parcel Details",
  dimensions: "Dimensions",
  financials: "Financials",
  dates: "Dates",
  recipient: "Recipient Address",
  shipper: "Shipper Address",
  zone: "Zone",
};
const GROUP_ORDER = ["parcel", "dimensions", "financials", "dates", "recipient", "shipper", "zone"];

export function ColumnPicker({
  appliedColumns,
  onApply,
}: ColumnPickerProps) {
  const [pendingKeys, setPendingKeys] = useState<Set<string>>(
    () => new Set(appliedColumns),
  );

  // Sync pending state when applied columns change (e.g., URL navigation)
  const [prevApplied, setPrevApplied] = useState(appliedColumns);
  if (
    prevApplied.length !== appliedColumns.length ||
    prevApplied.some((k, i) => k !== appliedColumns[i])
  ) {
    setPrevApplied(appliedColumns);
    setPendingKeys(new Set(appliedColumns));
  }

  const isDirty = useMemo(() => {
    if (pendingKeys.size !== appliedColumns.length) return true;
    for (const k of appliedColumns) {
      if (!pendingKeys.has(k)) return true;
    }
    return false;
  }, [pendingKeys, appliedColumns]);

  const handleToggle = (key: ColumnKey, checked: boolean) => {
    setPendingKeys((prev) => {
      const next = new Set(prev);
      if (checked) {
        next.add(key);
      } else {
        next.delete(key);
      }
      return next;
    });
  };

  const handleApply = () => {
    onApply(Array.from(pendingKeys) as ColumnKey[]);
  };

  const handleReset = () => {
    setPendingKeys(new Set(DEFAULT_COLUMNS));
  };

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button variant="outline" size="sm" className="gap-1.5">
          <Settings2 className="size-3.5" />
          Columns
        </Button>
      </PopoverTrigger>
      <PopoverContent align="end" className="w-56 p-0">
        <div className="px-3 py-2 border-b">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium">Columns</span>
            <button
              onClick={handleReset}
              className="text-xs text-muted-foreground hover:text-foreground transition-colors"
            >
              Reset defaults
            </button>
          </div>
        </div>

        <div className="p-3 space-y-3 max-h-[340px] overflow-y-auto">
          {GROUP_ORDER.map((group) => {
            const cols = COLUMN_REGISTRY.filter((c) => c.group === group);
            if (cols.length === 0) return null;
            return (
              <div key={group}>
                <p className="text-xs font-medium text-muted-foreground mb-1.5">
                  {GROUP_LABELS[group]}
                </p>
                <div className="space-y-1">
                  {cols.map((col) => {
                    const checked = pendingKeys.has(col.key);
                    const disabled = col.alwaysOn === true;
                    return (
                      <Label
                        key={col.key}
                        className={`flex items-center gap-2 py-0.5 px-1.5 rounded cursor-pointer transition-colors
                          ${disabled ? "opacity-60 cursor-not-allowed" : "hover:bg-muted"}
                          ${checked && !disabled ? "bg-muted/50" : ""}
                        `}
                      >
                        <Checkbox
                          checked={checked}
                          disabled={disabled}
                          onCheckedChange={(val) =>
                            handleToggle(col.key, val === true)
                          }
                          className="size-3.5"
                        />
                        <span className="text-sm">{col.label}</span>
                        {disabled && (
                          <span className="ml-auto text-xs text-muted-foreground">
                            always
                          </span>
                        )}
                      </Label>
                    );
                  })}
                </div>
              </div>
            );
          })}
        </div>

        <div className="px-3 py-2 border-t flex items-center justify-between gap-2">
          <span className="text-xs text-muted-foreground">
            {pendingKeys.size} columns
          </span>
          <Button
            size="sm"
            variant={isDirty ? "default" : "secondary"}
            disabled={!isDirty}
            onClick={handleApply}
            className="h-7 text-xs gap-1"
          >
            {isDirty && (
              <span className="size-1.5 rounded-full bg-primary-foreground/50 animate-pulse" />
            )}
            Apply
          </Button>
        </div>
      </PopoverContent>
    </Popover>
  );
}
