"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { useCancelParcel } from "@/hooks/use-parcels";

interface CancelParcelDialogProps {
  parcelId: string;
  trackingNumber: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CancelParcelDialog({
  parcelId,
  trackingNumber,
  open,
  onOpenChange,
}: CancelParcelDialogProps) {
  const [reason, setReason] = useState("");
  const cancelParcel = useCancelParcel();

  async function handleCancel() {
    if (!reason.trim()) return;
    try {
      await cancelParcel.mutateAsync({ id: parcelId, reason: reason.trim() });
      onOpenChange(false);
    } catch (err) {
      console.error(err);
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Cancel Parcel</DialogTitle>
          <DialogDescription>
            Are you sure you want to cancel parcel {trackingNumber}? This action cannot be undone.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          <div className="rounded-md border border-destructive/50 bg-destructive/5 p-3 text-sm text-destructive">
            Cancelling will permanently stop all processing and delivery of this parcel.
          </div>
          <div className="space-y-2">
            <label htmlFor="cancel-reason" className="text-sm font-medium">
              Reason <span className="text-destructive">*</span>
            </label>
            <Input
              id="cancel-reason"
              placeholder="Provide a reason for cancellation..."
              value={reason}
              onChange={(e) => setReason(e.target.value)}
            />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Keep Parcel
          </Button>
          <Button
            variant="destructive"
            onClick={handleCancel}
            disabled={!reason.trim() || cancelParcel.isPending}
          >
            {cancelParcel.isPending ? "Cancelling..." : "Cancel Parcel"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
