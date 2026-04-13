"use client";

import { useState, useMemo, type ReactNode } from "react";
import Link from "next/link";
import { useParcels } from "@/hooks/use-parcels";
import { useZones } from "@/hooks/use-zones";
import { useDebounce } from "@/hooks/use-debounce";
import { useColumnParams } from "@/hooks/use-column-params";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Search,
  ChevronLeft,
  ChevronRight,
  Package,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
  CalendarIcon,
  Plus,
  X,
} from "lucide-react";
import { ParcelStatus, type ParcelSummaryDto } from "@/types/parcel";
import type { DateRange } from "react-day-picker";
import { format } from "date-fns";
import {
  COLUMN_REGISTRY,
  type ColumnKey,
} from "@/components/parcel/column-registry";
import { ColumnPicker } from "@/components/parcel/column-picker";

const PAGE_SIZE_OPTIONS = [10, 25, 50, 100];
const DEFAULT_PAGE_SIZE = 25;

const STATUS_OPTIONS: { value: string; label: string }[] = [
  { value: "ALL", label: "All Statuses" },
  { value: ParcelStatus.Registered, label: "Registered" },
  { value: ParcelStatus.ReceivedAtDepot, label: "Received at Depot" },
  { value: ParcelStatus.Sorted, label: "Sorted" },
  { value: ParcelStatus.Staged, label: "Staged" },
  { value: ParcelStatus.Loaded, label: "Loaded" },
  { value: ParcelStatus.OutForDelivery, label: "Out for Delivery" },
  { value: ParcelStatus.Delivered, label: "Delivered" },
  { value: ParcelStatus.FailedAttempt, label: "Failed Attempt" },
  { value: ParcelStatus.ReturnedToDepot, label: "Returned to Depot" },
  { value: ParcelStatus.Cancelled, label: "Cancelled" },
  { value: ParcelStatus.Exception, label: "Exception" },
];

const SERVICE_TYPE_OPTIONS: { value: string; label: string }[] = [
  { value: "ALL", label: "All Service Types" },
  { value: "ECONOMY", label: "Economy" },
  { value: "STANDARD", label: "Standard" },
  { value: "EXPRESS", label: "Express" },
  { value: "OVERNIGHT", label: "Overnight" },
];

const PARCEL_TYPE_OPTIONS: { value: string; label: string }[] = [
  { value: "ALL", label: "All Parcel Types" },
  { value: "PACKAGE", label: "Package" },
  { value: "ENVELOPE", label: "Envelope" },
  { value: "PALLET", label: "Pallet" },
  { value: "BULK", label: "Bulk" },
];

type SortDirection = "ASC" | "DESC";

interface SortState {
  field: string;
  direction: SortDirection;
}

function getStatusBadgeClasses(status: string): string {
  switch (status) {
    case "DELIVERED":
      return "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100";
    case "EXCEPTION":
    case "CANCELLED":
      return "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-100";
    case "OUT_FOR_DELIVERY":
      return "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-100";
    case "REGISTERED":
      return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-400";
    default:
      return "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-100";
  }
}

function formatStatus(status: string): string {
  return status
    .replace(/_/g, " ")
    .toLowerCase()
    .replace(/\b\w/g, (c) => c.toUpperCase());
}

function renderCell(key: ColumnKey, parcel: ParcelSummaryDto): ReactNode {
  switch (key) {
    case "trackingNumber":
      return <span className="font-mono font-medium">{parcel.trackingNumber}</span>;
    case "status":
      return (
        <span
          className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${getStatusBadgeClasses(parcel.status ?? "")}`}
        >
          {formatStatus(parcel.status ?? "")}
        </span>
      );
    case "serviceType":
      return parcel.serviceType ?? "\u2014";
    case "description":
      return parcel.description ?? "\u2014";
    case "parcelType":
      return parcel.parcelType ?? "\u2014";
    case "weight":
      return parcel.weight
        ? `${parcel.weight} ${parcel.weightUnit ?? "lb"}`
        : "\u2014";
    case "length":
      return parcel.length
        ? `${parcel.length} ${parcel.dimensionUnit ?? "cm"}`
        : "\u2014";
    case "width":
      return parcel.width
        ? `${parcel.width} ${parcel.dimensionUnit ?? "cm"}`
        : "\u2014";
    case "height":
      return parcel.height
        ? `${parcel.height} ${parcel.dimensionUnit ?? "cm"}`
        : "\u2014";
    case "dimensionUnit":
      return parcel.dimensionUnit ?? "\u2014";
    case "declaredValue":
      return parcel.declaredValue
        ? `${parcel.declaredValue} ${parcel.currency ?? "USD"}`
        : "\u2014";
    case "currency":
      return parcel.currency ?? "\u2014";
    case "estimatedDeliveryDate":
      return parcel.estimatedDeliveryDate
        ? new Date(parcel.estimatedDeliveryDate).toLocaleDateString()
        : "\u2014";
    case "actualDeliveryDate":
      return parcel.actualDeliveryDate
        ? new Date(parcel.actualDeliveryDate).toLocaleDateString()
        : "\u2014";
    case "deliveryAttempts":
      return String(parcel.deliveryAttempts ?? 0);
    case "createdAt":
      return new Date(parcel.createdAt).toLocaleDateString();
    case "recipientContactName":
      return parcel.recipientAddress?.contactName ?? "\u2014";
    case "recipientPhone":
      return parcel.recipientAddress?.phone ?? "\u2014";
    case "recipientEmail":
      return parcel.recipientAddress?.email ?? "\u2014";
    case "recipientStreet":
      return parcel.recipientAddress?.street1 ?? "\u2014";
    case "recipientCity":
      return parcel.recipientAddress?.city ?? "\u2014";
    case "recipientState":
      return parcel.recipientAddress?.state ?? "\u2014";
    case "recipientPostalCode":
      return parcel.recipientAddress?.postalCode ?? "\u2014";
    case "recipientCountryCode":
      return parcel.recipientAddress?.countryCode ?? "\u2014";
    case "shipperContactName":
      return parcel.shipperAddress?.contactName ?? "\u2014";
    case "shipperCity":
      return parcel.shipperAddress?.city ?? "\u2014";
    case "zone":
      return parcel.zone?.name ?? "\u2014";
    case "depot":
      return parcel.depot?.name ?? "\u2014";
    case "link":
      return (
        <Link
          href={`/parcels/${parcel.trackingNumber}`}
          className="text-primary hover:underline text-sm"
        >
          Link to parcel
        </Link>
      );
    default:
      return "\u2014";
  }
}

function SortIcon({ field, sort }: { field: string; sort: SortState | null }) {
  if (!sort || sort.field !== field) {
    return <ArrowUpDown className="size-3 ml-1 opacity-50" />;
  }
  return sort.direction === "ASC" ? (
    <ArrowUp className="size-3 ml-1" />
  ) : (
    <ArrowDown className="size-3 ml-1" />
  );
}

function DateRangePicker({
  value,
  onChange,
  placeholder,
}: {
  value: DateRange | undefined;
  onChange: (range: DateRange | undefined) => void;
  placeholder?: string;
}) {
  const [open, setOpen] = useState(false);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          className="justify-start text-left font-normal h-9 min-w-[180px]"
        >
          <CalendarIcon className="size-3.5 mr-1 opacity-50" />
          {value?.from ? (
            value.to ? (
              <>
                {format(value.from, "MMM d, yyyy")} –{" "}
                {format(value.to, "MMM d, yyyy")}
              </>
            ) : (
              format(value.from, "MMM d, yyyy")
            )
          ) : (
            <span className="text-muted-foreground">
              {placeholder ?? "Pick a date range"}
            </span>
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar
          mode="range"
          selected={value}
          onSelect={(range) => {
            onChange(range);
            if (range?.to) setOpen(false);
          }}
          numberOfMonths={2}
        />
        {value && (
          <div className="flex justify-end p-2 border-t">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => {
                onChange(undefined);
                setOpen(false);
              }}
            >
              Clear
            </Button>
          </div>
        )}
      </PopoverContent>
    </Popover>
  );
}

export function ParcelList() {
  const { appliedColumns, applyColumns } = useColumnParams();

  const [recipientSearch, setRecipientSearch] = useState("");
  const [addressSearch, setAddressSearch] = useState("");
  const [trackingNumber, setTrackingNumber] = useState("");
  const [statusFilter, setStatusFilter] = useState("ALL");
  const [serviceTypeFilter, setServiceTypeFilter] = useState("ALL");
  const [parcelTypeFilter, setParcelTypeFilter] = useState("ALL");
  const [zoneFilter, setZoneFilter] = useState("ALL");
  const [createdDateRange, setCreatedDateRange] = useState<
    DateRange | undefined
  >(undefined);
  const [estimatedDeliveryDateRange, setEstimatedDeliveryDateRange] = useState<
    DateRange | undefined
  >(undefined);
  const [actualDeliveryDateRange, setActualDeliveryDateRange] = useState<
    DateRange | undefined
  >(undefined);
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [sort, setSort] = useState<SortState | null>(null);
  const [cursor, setCursor] = useState<string | undefined>(undefined);
  const [history, setHistory] = useState<string[]>([]);

  const debouncedRecipientSearch = useDebounce(recipientSearch, 300);
  const debouncedAddressSearch = useDebounce(addressSearch, 300);
  const debouncedTrackingNumber = useDebounce(trackingNumber, 300);

  const { data: zonesData } = useZones();
  const zones = zonesData ?? [];

  const resetPagination = () => {
    setCursor(undefined);
    setHistory([]);
  };

  const order = sort ? { [sort.field]: sort.direction } : undefined;

  const filters = {
    recipientSearch: debouncedRecipientSearch || undefined,
    addressSearch: debouncedAddressSearch || undefined,
    trackingNumber: debouncedTrackingNumber || undefined,
    status: statusFilter !== "ALL" ? statusFilter : undefined,
    serviceType: serviceTypeFilter !== "ALL" ? serviceTypeFilter : undefined,
    parcelType: parcelTypeFilter !== "ALL" ? parcelTypeFilter : undefined,
    zoneId: zoneFilter !== "ALL" ? zoneFilter : undefined,
    first: pageSize,
    after: cursor,
    order,
    columns: appliedColumns,
    createdAfter: createdDateRange?.from?.toISOString(),
    createdBefore: createdDateRange?.to?.toISOString(),
    estimatedDeliveryAfter: estimatedDeliveryDateRange?.from?.toISOString(),
    estimatedDeliveryBefore: estimatedDeliveryDateRange?.to?.toISOString(),
    actualDeliveryAfter: actualDeliveryDateRange?.from?.toISOString(),
    actualDeliveryBefore: actualDeliveryDateRange?.to?.toISOString(),
  };

  const { data, isLoading, error } = useParcels(filters);

  const parcels = data?.nodes ?? [];
  const pageInfo = data?.pageInfo;
  const totalCount = data?.totalCount ?? 0;

  const visibleColumns = useMemo(
    () => COLUMN_REGISTRY.filter((c) => appliedColumns.includes(c.key)),
    [appliedColumns],
  );

  const handleNextPage = () => {
    if (pageInfo?.endCursor) {
      setHistory((prev) => [...prev, cursor ?? ""]);
      setCursor(pageInfo.endCursor);
    }
  };

  const handlePreviousPage = () => {
    const prev = history[history.length - 1];
    setHistory((h) => h.slice(0, -1));
    setCursor(prev || undefined);
  };

  const handleSort = (field: string) => {
    setSort((current) => {
      if (!current || current.field !== field) {
        return { field, direction: "ASC" };
      }
      if (current.direction === "ASC") {
        return { field, direction: "DESC" };
      }
      return null;
    });
    resetPagination();
  };

  const hasActiveFilters =
    debouncedRecipientSearch ||
    debouncedAddressSearch ||
    debouncedTrackingNumber ||
    statusFilter !== "ALL" ||
    serviceTypeFilter !== "ALL" ||
    parcelTypeFilter !== "ALL" ||
    zoneFilter !== "ALL" ||
    createdDateRange ||
    estimatedDeliveryDateRange ||
    actualDeliveryDateRange;

  const clearAllFilters = () => {
    setRecipientSearch("");
    setAddressSearch("");
    setTrackingNumber("");
    setStatusFilter("ALL");
    setServiceTypeFilter("ALL");
    setParcelTypeFilter("ALL");
    setZoneFilter("ALL");
    setCreatedDateRange(undefined);
    setEstimatedDeliveryDateRange(undefined);
    setActualDeliveryDateRange(undefined);
    resetPagination();
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Parcels</CardTitle>
            <CardDescription>Search and manage parcels</CardDescription>
          </div>
          <Link
            href="/parcels/new"
            className="inline-flex items-center gap-1 rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:bg-primary/90"
          >
            <Plus className="size-4" />
            New Parcel
          </Link>
        </div>

        {/* Row 1: Text search inputs */}
        <div className="flex items-center gap-3 pt-2 flex-wrap">
          <div className="relative flex-1 min-w-[180px] max-w-xs">
            <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Tracking #..."
              value={trackingNumber}
              onChange={(e) => {
                setTrackingNumber(e.target.value);
                resetPagination();
              }}
              className="pl-9"
            />
          </div>

          <div className="relative flex-1 min-w-[180px] max-w-xs">
            <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Recipient name..."
              value={recipientSearch}
              onChange={(e) => {
                setRecipientSearch(e.target.value);
                resetPagination();
              }}
              className="pl-9"
            />
          </div>

          <div className="relative flex-1 min-w-[180px] max-w-xs">
            <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Address (street, city, zip)..."
              value={addressSearch}
              onChange={(e) => {
                setAddressSearch(e.target.value);
                resetPagination();
              }}
              className="pl-9"
            />
          </div>
        </div>

        {/* Row 2: Selects + page size + column picker */}
        <div className="flex items-center gap-3 pt-2 flex-wrap">
          <Select
            value={statusFilter}
            onValueChange={(value) => {
              setStatusFilter(value);
              resetPagination();
            }}
          >
            <SelectTrigger className="inline-flex shrink-0 items-center justify-center text-sm font-medium border border-input bg-transparent rounded-md h-9 px-3 w-[180px]">
              <SelectValue placeholder="Status" />
            </SelectTrigger>
            <SelectContent>
              {STATUS_OPTIONS.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>
                  {opt.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select
            value={serviceTypeFilter}
            onValueChange={(value) => {
              setServiceTypeFilter(value);
              resetPagination();
            }}
          >
            <SelectTrigger className="inline-flex shrink-0 items-center justify-center text-sm font-medium border border-input bg-transparent rounded-md h-9 px-3 w-[180px]">
              <SelectValue placeholder="Service type" />
            </SelectTrigger>
            <SelectContent>
              {SERVICE_TYPE_OPTIONS.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>
                  {opt.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select
            value={parcelTypeFilter}
            onValueChange={(value) => {
              setParcelTypeFilter(value);
              resetPagination();
            }}
          >
            <SelectTrigger className="inline-flex shrink-0 items-center justify-center text-sm font-medium border border-input bg-transparent rounded-md h-9 px-3 w-[180px]">
              <SelectValue placeholder="Parcel type" />
            </SelectTrigger>
            <SelectContent>
              {PARCEL_TYPE_OPTIONS.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>
                  {opt.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select
            value={zoneFilter}
            onValueChange={(value) => {
              setZoneFilter(value);
              resetPagination();
            }}
          >
            <SelectTrigger className="inline-flex shrink-0 items-center justify-center text-sm font-medium border border-input bg-transparent rounded-md h-9 px-3 w-[180px]">
              <SelectValue placeholder="All Zones" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="ALL">All Zones</SelectItem>
              {zones.map((zone) => (
                <SelectItem key={zone.id} value={zone.id}>
                  {zone.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select
            value={String(pageSize)}
            onValueChange={(value) => {
              setPageSize(Number(value));
              resetPagination();
            }}
          >
            <SelectTrigger className="inline-flex shrink-0 items-center justify-center text-sm font-medium border border-input bg-transparent rounded-md h-9 px-3 w-[120px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {PAGE_SIZE_OPTIONS.map((size) => (
                <SelectItem key={size} value={String(size)}>
                  {size} / page
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <ColumnPicker
            appliedColumns={appliedColumns}
            onApply={applyColumns}
          />

          {hasActiveFilters && (
            <Button
              variant="ghost"
              size="sm"
              onClick={clearAllFilters}
              className="text-muted-foreground"
            >
              <X className="size-3.5 mr-1" />
              Clear filters
            </Button>
          )}
        </div>

        {/* Row 3: Date range filters */}
        <div className="flex items-center gap-3 pt-2 flex-wrap">
          <DateRangePicker
            value={createdDateRange}
            onChange={(range) => {
              setCreatedDateRange(range);
              resetPagination();
            }}
            placeholder="Created date range"
          />

          <DateRangePicker
            value={estimatedDeliveryDateRange}
            onChange={(range) => {
              setEstimatedDeliveryDateRange(range);
              resetPagination();
            }}
            placeholder="Est. delivery date range"
          />

          <DateRangePicker
            value={actualDeliveryDateRange}
            onChange={(range) => {
              setActualDeliveryDateRange(range);
              resetPagination();
            }}
            placeholder="Actual delivery date range"
          />
        </div>
      </CardHeader>

      <CardContent>
        {isLoading ? (
          <div className="py-8 text-center text-muted-foreground">
            Loading parcels...
          </div>
        ) : error ? (
          <div className="py-8 text-center text-destructive">
            Error loading parcels
          </div>
        ) : parcels.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 text-muted-foreground">
            <Package className="size-12 mb-3" />
            <p>No parcels found</p>
            {hasActiveFilters && (
              <p className="text-sm mt-1">
                Try adjusting your search or filters
              </p>
            )}
          </div>
        ) : (
          <>
            <div className="text-sm text-muted-foreground mb-3">
              {totalCount} parcel{totalCount !== 1 ? "s" : ""} total
            </div>

            <Table>
              <TableHeader>
                <TableRow>
                  {visibleColumns.map((col) =>
                    col.sortable ? (
                      <TableHead
                        key={col.key}
                        className="cursor-pointer select-none hover:bg-muted/50"
                        onClick={() => handleSort(col.key)}
                      >
                        <div className="flex items-center">
                          {col.label}
                          <SortIcon field={col.key} sort={sort} />
                        </div>
                      </TableHead>
                    ) : (
                      <TableHead key={col.key}>{col.label}</TableHead>
                    ),
                  )}
                </TableRow>
              </TableHeader>
              <TableBody>
                {parcels.map((parcel) => (
                  <TableRow key={parcel.id}>
                    {visibleColumns.map((col) => (
                      <TableCell key={col.key}>
                        {renderCell(col.key, parcel)}
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            {/* Pagination */}
            <div className="flex items-center justify-between pt-4">
              <div className="text-sm text-muted-foreground">
                Showing {parcels.length} of {totalCount}
              </div>
              <div className="flex items-center gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handlePreviousPage}
                  disabled={!pageInfo?.hasPreviousPage}
                >
                  <ChevronLeft className="size-4 mr-1" />
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleNextPage}
                  disabled={!pageInfo?.hasNextPage}
                >
                  Next
                  <ChevronRight className="size-4 ml-1" />
                </Button>
              </div>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}
