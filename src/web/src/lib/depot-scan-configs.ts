import type { ScanOperationConfig } from "@/types/depot-scan";

export const scanConfigs: Record<string, ScanOperationConfig> = {
  inbound: {
    title: "Inbound Receiving",
    description: "Scan incoming parcels against a manifest",
    targetStatus: "RECEIVED_AT_DEPOT",
    inputPlaceholder: "Scan or type tracking number...",
    allowUnexpected: true,
  },
  sorting: {
    title: "Sort & Zone Assignment",
    description: "Scan parcels to sort and assign to zones",
    targetStatus: "SORTED",
    inputPlaceholder: "Scan or type tracking number...",
    allowUnexpected: false,
  },
  staging: {
    title: "Staging for Route Load-Out",
    description: "Move sorted parcels to staging area for a route",
    targetStatus: "STAGED",
    inputPlaceholder: "Scan or type tracking number...",
    allowUnexpected: false,
  },
  loadout: {
    title: "Route Load-Out",
    description: "Scan parcels as they are loaded onto the vehicle",
    targetStatus: "LOADED",
    inputPlaceholder: "Scan or type tracking number...",
    allowUnexpected: false,
  },
  returns: {
    title: "Returns Receiving",
    description: "Scan returned parcels back into the depot",
    targetStatus: "RETURNED_TO_DEPOT",
    inputPlaceholder: "Scan or type tracking number...",
    allowUnexpected: true,
  },
};
