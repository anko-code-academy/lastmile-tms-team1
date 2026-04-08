"use client";

import { use } from "react";
import { ParcelDetail } from "@/components/parcels/parcel-detail";

interface ParcelDetailPageProps {
  params: Promise<{ trackingNumber: string }>;
}

export default function ParcelDetailPage({ params }: ParcelDetailPageProps) {
  const { trackingNumber } = use(params);
  return <ParcelDetail trackingNumber={trackingNumber} />;
}
