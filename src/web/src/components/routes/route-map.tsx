"use client";

import { useEffect, useRef, useCallback } from "react";
import mapboxgl from "mapbox-gl";
import "mapbox-gl/dist/mapbox-gl.css";
import {
  parseZoneGeoJson,
  createPolygonFeature,
  getZoneCenter,
} from "@/components/map/map-utils";
import type { ZoneSummaryDto, DepotSummaryDto } from "@/lib/graphql/types";
import type { GetRoutesForMapQuery } from "@/graphql/generated/graphql";
import type { RouteStatus, RouteStopStatus } from "@/graphql/generated/graphql";

const MAPBOX_TOKEN = process.env.NEXT_PUBLIC_MAPBOX_ACCESS_TOKEN ?? "";

const ROUTE_STATUS_COLORS: Record<RouteStatus, string> = {
  DRAFT: "#9ca3af",
  IN_PROGRESS: "#3b82f6",
  COMPLETED: "#22c55e",
};

const STOP_STATUS_COLORS: Record<RouteStopStatus, string> = {
  PENDING: "#f59e0b",
  ARRIVED: "#3b82f6",
  COMPLETED: "#22c55e",
  SKIPPED: "#ef4444",
};

const ZONE_COLORS = [
  "#3b82f6", "#10b981", "#f59e0b", "#ef4444",
  "#8b5cf6", "#ec4899", "#06b6d4", "#84cc16",
];

type RouteMapNode = NonNullable<NonNullable<GetRoutesForMapQuery["routes"]>["nodes"]>[number];

interface RouteMapProps {
  routes: RouteMapNode[];
  zones: ZoneSummaryDto[];
  depots: DepotSummaryDto[];
}

function getZoneColor(index: number): string {
  return ZONE_COLORS[index % ZONE_COLORS.length];
}

export function RouteMap({ routes, zones, depots }: RouteMapProps) {
  const mapContainer = useRef<HTMLDivElement>(null);
  const mapRef = useRef<mapboxgl.Map | null>(null);
  const addedLayersRef = useRef<Set<string>>(new Set());

  const cleanup = useCallback((currentMap: mapboxgl.Map) => {
    addedLayersRef.current.forEach((id) => {
      if (currentMap.getLayer(id)) currentMap.removeLayer(id);
      if (currentMap.getSource(id)) currentMap.removeSource(id);
    });
    addedLayersRef.current.clear();
  }, []);

  const addZones = useCallback((currentMap: mapboxgl.Map) => {
    zones.forEach((zone, i) => {
      if (!zone.boundaryGeometry) return;
      const geoJsonStr =
        typeof zone.boundaryGeometry === "string"
          ? zone.boundaryGeometry
          : JSON.stringify(zone.boundaryGeometry);
      const parsed = parseZoneGeoJson(geoJsonStr);
      if (!parsed) return;

      const color = getZoneColor(i);
      const srcId = `zone-${zone.id}`;
      if (currentMap.getSource(srcId)) return;

      currentMap.addSource(srcId, {
        type: "geojson",
        data: createPolygonFeature(parsed) as GeoJSON.Feature,
      });
      addedLayersRef.current.add(srcId);

      currentMap.addLayer({
        id: `zone-fill-${zone.id}`,
        type: "fill",
        source: srcId,
        paint: { "fill-color": color, "fill-opacity": 0.15 },
      });
      addedLayersRef.current.add(`zone-fill-${zone.id}`);

      currentMap.addLayer({
        id: `zone-stroke-${zone.id}`,
        type: "line",
        source: srcId,
        paint: { "line-color": color, "line-width": 2 },
      });
      addedLayersRef.current.add(`zone-stroke-${zone.id}`);

      const center = getZoneCenter(geoJsonStr);
      if (center) {
        const labelSrcId = `zone-label-${zone.id}`;
        currentMap.addSource(labelSrcId, {
          type: "geojson",
          data: {
            type: "Feature",
            properties: { name: zone.name },
            geometry: { type: "Point", coordinates: center },
          } as unknown as GeoJSON.Feature,
        });
        addedLayersRef.current.add(labelSrcId);

        currentMap.addLayer({
          id: `zone-label-layer-${zone.id}`,
          type: "symbol",
          source: labelSrcId,
          layout: {
            "text-field": ["get", "name"],
            "text-size": 11,
            "text-anchor": "center",
          },
          paint: {
            "text-color": "#ffffff",
            "text-halo-color": color,
            "text-halo-width": 1.5,
          },
        });
        addedLayersRef.current.add(`zone-label-layer-${zone.id}`);
      }
    });
  }, [zones]);

  const addDepots = useCallback((currentMap: mapboxgl.Map) => {
    depots.forEach((depot) => {
      const coords = depot.address?.geoLocation?.coordinates;
      if (!coords) return;

      const el = document.createElement("div");
      el.className = "depot-marker";
      Object.assign(el.style, {
        width: "24px",
        height: "24px",
        borderRadius: "50%",
        background: "#ef4444",
        border: "3px solid #fff",
        boxShadow: "0 2px 6px rgba(0,0,0,0.3)",
        cursor: "pointer",
      });

      new mapboxgl.Marker({ element: el })
        .setLngLat([coords[0], coords[1]])
        .setPopup(
          new mapboxgl.Popup({ offset: 12, closeButton: false }).setText(depot.name)
        )
        .addTo(currentMap);
    });
  }, [depots]);

  const addRoutes = useCallback((currentMap: mapboxgl.Map) => {
    const bounds = new mapboxgl.LngLatBounds();

    // Build zoneId → depot coords lookup
    const zoneDepotCoords = new Map<string, [number, number]>();
    for (const zone of zones) {
      const depot = depots.find((d) => d.id === zone.depotId);
      const depotCoords = depot?.address?.geoLocation?.coordinates;
      if (depotCoords) zoneDepotCoords.set(zone.id, [depotCoords[0], depotCoords[1]]);
    }

    routes.forEach((route) => {
      const color = ROUTE_STATUS_COLORS[route.status as RouteStatus] ?? "#9ca3af";
      const stops = route.routeStops
        .filter((s) => s.geoLocation?.coordinates)
        .sort((a, b) => a.sequenceNumber - b.sequenceNumber);

      if (stops.length === 0) return;

      // Route line: depot → stops
      const stopCoords = stops.map(
        (s) => s.geoLocation!.coordinates! as [number, number]
      );
      const depotCoord = route.zoneId ? zoneDepotCoords.get(route.zoneId) : undefined;
      const lineCoords = depotCoord ? [depotCoord, ...stopCoords] : stopCoords;

      if (lineCoords.length >= 2) {
        const lineSrcId = `route-line-${route.id}`;
        currentMap.addSource(lineSrcId, {
          type: "geojson",
          data: {
            type: "Feature",
            geometry: { type: "LineString", coordinates: lineCoords },
            properties: {},
          },
        });
        addedLayersRef.current.add(lineSrcId);

        currentMap.addLayer({
          id: `route-line-layer-${route.id}`,
          type: "line",
          source: lineSrcId,
          paint: {
            "line-color": color,
            "line-width": 3,
            "line-opacity": 0.6,
            "line-dasharray": route.status === ("COMPLETED" as RouteStatus) ? [2, 2] : [1],
          },
        });
        addedLayersRef.current.add(`route-line-layer-${route.id}`);
      }

      // Stop markers
      stops.forEach((stop) => {
        const coords = stop.geoLocation!.coordinates! as [number, number];
        bounds.extend(coords);
        if (depotCoord) bounds.extend(depotCoord);
        const stopColor = STOP_STATUS_COLORS[stop.status as RouteStopStatus] ?? "#9ca3af";

        const el = document.createElement("div");
        Object.assign(el.style, {
          width: "14px",
          height: "14px",
          borderRadius: "50%",
          background: stopColor,
          border: "2px solid #fff",
          boxShadow: "0 1px 4px rgba(0,0,0,0.3)",
          cursor: "pointer",
        });

        const parcelList = stop.parcels
          .map((p) => `${p.trackingNumber} (${p.status})`)
          .join("<br>");

        const popup = new mapboxgl.Popup({ offset: 8, closeButton: false })
          .setHTML(
            `<div style="font-size:13px;min-width:160px">
              <strong style="color:${color}">${route.name}</strong>
              <div style="margin:4px 0">Stop #${stop.sequenceNumber} — ${stop.street1}</div>
              <div style="color:#888">Status: ${stop.status}</div>
              ${stop.parcels.length > 0 ? `<div style="margin-top:4px;font-size:12px">${parcelList}</div>` : ""}
            </div>`
          );

        new mapboxgl.Marker({ element: el })
          .setLngLat(coords)
          .setPopup(popup)
          .addTo(currentMap);
      });
    });

    if (!bounds.isEmpty()) {
      currentMap.fitBounds(bounds, { padding: 60, maxZoom: 14 });
    }
  }, [routes, zones, depots]);

  useEffect(() => {
    if (!mapContainer.current || !MAPBOX_TOKEN) return;

    mapboxgl.accessToken = MAPBOX_TOKEN;

    const currentMap = new mapboxgl.Map({
      container: mapContainer.current,
      style: "mapbox://styles/mapbox/streets-v12",
      center: [-74.006, 40.7128],
      zoom: 12,
    });

    currentMap.on("load", () => {
      addZones(currentMap);
      addDepots(currentMap);
      addRoutes(currentMap);
    });

    mapRef.current = currentMap;

    return () => {
      cleanup(currentMap);
      currentMap.remove();
    };
  }, [addZones, addDepots, addRoutes, cleanup]);

  if (!MAPBOX_TOKEN) {
    return (
      <div className="flex h-[400px] items-center justify-center rounded-lg border bg-muted">
        <p className="text-muted-foreground">
          Mapbox token not configured. Set NEXT_PUBLIC_MAPBOX_ACCESS_TOKEN.
        </p>
      </div>
    );
  }

  return <div ref={mapContainer} className="h-[500px] rounded-lg" />;
}
