"use client";

import { EventType, type GetTrackingEventsQuery } from "@/graphql/generated/graphql";

type TrackingEventNode = NonNullable<NonNullable<GetTrackingEventsQuery['trackingEvents']>['nodes']>[number];
import {
  Package,
  Truck,
  CheckCircle2,
  AlertTriangle,
  MapPin,
  Clock,
  ArrowRightCircle,
  type LucideIcon,
} from "lucide-react";

const eventIconMap: Record<string, LucideIcon> = {
  [EventType.LabelCreated]: Package,
  [EventType.PickedUp]: ArrowRightCircle,
  [EventType.InTransit]: Truck,
  [EventType.OutForDelivery]: Truck,
  [EventType.Delivered]: CheckCircle2,
  [EventType.DeliveryAttempted]: Clock,
  [EventType.Exception]: AlertTriangle,
  [EventType.Returned]: ArrowRightCircle,
  [EventType.ArrivedAtFacility]: MapPin,
  [EventType.DepartedFacility]: MapPin,
  [EventType.HeldAtFacility]: MapPin,
  [EventType.AddressCorrection]: AlertTriangle,
  [EventType.CustomsClearance]: Clock,
};

const eventColorMap: Record<string, string> = {
  [EventType.LabelCreated]: "text-gray-500 bg-gray-100",
  [EventType.PickedUp]: "text-blue-500 bg-blue-100",
  [EventType.InTransit]: "text-blue-500 bg-blue-100",
  [EventType.OutForDelivery]: "text-amber-500 bg-amber-100",
  [EventType.Delivered]: "text-green-600 bg-green-100",
  [EventType.DeliveryAttempted]: "text-amber-500 bg-amber-100",
  [EventType.Exception]: "text-red-500 bg-red-100",
  [EventType.Returned]: "text-red-500 bg-red-100",
  [EventType.ArrivedAtFacility]: "text-gray-500 bg-gray-100",
  [EventType.DepartedFacility]: "text-gray-500 bg-gray-100",
  [EventType.HeldAtFacility]: "text-amber-500 bg-amber-100",
  [EventType.AddressCorrection]: "text-amber-500 bg-amber-100",
  [EventType.CustomsClearance]: "text-blue-500 bg-blue-100",
};

interface ParcelStatusTimelineProps {
  events: TrackingEventNode[];
}

function formatEventLabel(type: EventType): string {
  return type.replace(/_/g, " ").toLowerCase().replace(/^\w/, (c) => c.toUpperCase());
}

export function ParcelStatusTimeline({ events }: ParcelStatusTimelineProps) {
  if (events.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        No tracking events recorded yet.
      </div>
    );
  }

  return (
    <div className="relative space-y-0">
      {events.map((event, index) => {
        const Icon = eventIconMap[event.eventType] ?? MapPin;
        const color = eventColorMap[event.eventType] ?? "text-gray-500 bg-gray-100";
        const isLast = index === events.length - 1;

        return (
          <div key={String(event.id)} className="flex gap-4">
            {/* Timeline line + icon */}
            <div className="flex flex-col items-center">
              <div className={`flex items-center justify-center w-8 h-8 rounded-full shrink-0 ${color}`}>
                <Icon className="h-4 w-4" />
              </div>
              {!isLast && (
                <div className="w-px flex-1 bg-border min-h-8" />
              )}
            </div>

            {/* Content */}
            <div className={`pb-6 ${isLast ? "" : ""}`}>
              <p className="font-medium text-sm">{formatEventLabel(event.eventType)}</p>
              {event.description && (
                <p className="text-sm text-muted-foreground">{event.description}</p>
              )}
              <div className="flex items-center gap-2 mt-1">
                <span className="text-xs text-muted-foreground">
                  {new Date(event.timestamp).toLocaleString()}
                </span>
                {(event.locationCity || event.locationState) && (
                  <span className="text-xs text-muted-foreground">
                    &middot; {[event.locationCity, event.locationState].filter(Boolean).join(", ")}
                  </span>
                )}
                {event.operator && (
                  <span className="text-xs text-muted-foreground">
                    &middot; {event.operator}
                  </span>
                )}
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
