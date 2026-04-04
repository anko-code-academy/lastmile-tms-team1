"use client";

import { use } from "react";
import { ParcelDetail } from "@/components/parcels/parcel-detail";

interface ParcelDetailPageProps {
  params: Promise<{ id: string }>;
}

export default function ParcelDetailPage({ params }: ParcelDetailPageProps) {
  const { id } = use(params);
  return <ParcelDetail id={id} />;
}
