export interface ScanOperationConfig {
  title: string;
  description: string;
  targetStatus: string;
  inputPlaceholder: string;
  allowUnexpected: boolean;
}

export type ScanEntryStatus = "success" | "error" | "duplicate" | "unexpected";

export interface ScannedParcelEntry {
  trackingNumber: string;
  status: ScanEntryStatus;
  errorMessage?: string;
  previousStatus?: string;
  newStatus?: string;
  zoneName?: string;
  binLabel?: string;
  routeName?: string;
  scannedAt: Date;
}

export interface ScanResult {
  trackingNumber: string;
  previousStatus: string;
  newStatus: string;
  zoneName?: string | null;
  binLabel?: string | null;
  routeName?: string | null;
}
