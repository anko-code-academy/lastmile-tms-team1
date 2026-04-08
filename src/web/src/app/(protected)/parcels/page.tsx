"use client";

import { Suspense } from "react";
import { ParcelList } from "@/components/parcel/parcel-list";

export default function ParcelsPage() {
  return (
    <Suspense>
      <ParcelList />
    </Suspense>
  );
}
