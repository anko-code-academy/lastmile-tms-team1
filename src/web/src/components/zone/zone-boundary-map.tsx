"use client";

import { useCallback, useEffect, useRef } from "react";
import mapboxgl from "mapbox-gl";
import MapboxDraw from "@mapbox/mapbox-gl-draw";
import "mapbox-gl/dist/mapbox-gl.css";
import "@mapbox/mapbox-gl-draw/dist/mapbox-gl-draw.css";
import { parseZoneGeoJson, formatGeoJson, createPolygonFeature } from "@/components/map/map-utils";
import type { ZoneGeoJson } from "@/components/map/map-utils";

const MAPBOX_TOKEN = process.env.NEXT_PUBLIC_MAPBOX_ACCESS_TOKEN ?? "";

interface ZoneBoundaryMapProps {
  initialGeoJson?: string;
  onGeoJsonChange: (geoJson: string) => void;
  readOnly?: boolean;
}

export function ZoneBoundaryMap({
  initialGeoJson,
  onGeoJsonChange,
  readOnly = false,
}: ZoneBoundaryMapProps) {
  const mapContainer = useRef<HTMLDivElement>(null);
  const map = useRef<mapboxgl.Map | null>(null);
  const draw = useRef<MapboxDraw | null>(null);

  const handleDrawCreate = useCallback((e: { features: GeoJSON.Feature[] }) => {
    if (e.features.length > 0) {
      const feature = e.features[0];
      if (feature.geometry.type === "Polygon" || feature.geometry.type === "MultiPolygon") {
        const geoJson: ZoneGeoJson = {
          type: feature.geometry.type,
          coordinates: feature.geometry.coordinates,
        };
        onGeoJsonChange(formatGeoJson(geoJson));
      }
    }
  }, [onGeoJsonChange]);

  const handleDrawUpdate = useCallback((e: { features: GeoJSON.Feature[] }) => {
    if (e.features.length > 0) {
      const feature = e.features[0];
      if (feature.geometry.type === "Polygon" || feature.geometry.type === "MultiPolygon") {
        const geoJson: ZoneGeoJson = {
          type: feature.geometry.type,
          coordinates: feature.geometry.coordinates,
        };
        onGeoJsonChange(formatGeoJson(geoJson));
      }
    }
  }, [onGeoJsonChange]);

  const handleDrawDelete = useCallback(() => {
    onGeoJsonChange("");
  }, [onGeoJsonChange]);

  useEffect(() => {
    if (!mapContainer.current || !MAPBOX_TOKEN) return;

    mapboxgl.accessToken = MAPBOX_TOKEN;

    map.current = new mapboxgl.Map({
      container: mapContainer.current,
      style: "mapbox://styles/mapbox/streets-v12",
      center: [-74.006, 40.7128],
      zoom: 12,
    });

    if (!readOnly) {
      draw.current = new MapboxDraw({
        displayControlsDefault: false,
        controls: {
          polygon: true,
          trash: true,
        },
        defaultMode: "draw_polygon",
        styles: [
          {
            id: "gl-draw-polygon-fill",
            type: "fill",
            filter: ["all", ["==", "$type", "Polygon"]],
            paint: {
              "fill-color": "#3b82f6",
              "fill-opacity": 0.3,
            },
          },
          {
            id: "gl-draw-polygon-stroke",
            type: "line",
            filter: ["all", ["==", "$type", "Polygon"]],
            paint: {
              "line-color": "#3b82f6",
              "line-width": 2,
            },
          },
        ],
      });

      map.current.addControl(draw.current);
    }

    const currentMap = map.current;
    const currentDraw = draw.current;

    currentMap.on("load", () => {
      if (initialGeoJson) {
        const parsed = parseZoneGeoJson(initialGeoJson);
        if (parsed) {
          const feature = createPolygonFeature(parsed);
          if (!readOnly) {
            if (currentDraw) {
              currentDraw.add(feature as GeoJSON.Feature);
            }
          } else {
            if (!currentMap.getSource("zone-boundary")) {
              currentMap.addSource("zone-boundary", {
                type: "geojson",
                data: feature as GeoJSON.Feature,
              });
              currentMap.addLayer({
                id: "zone-boundary-fill",
                type: "fill",
                source: "zone-boundary",
                paint: {
                  "fill-color": "#3b82f6",
                  "fill-opacity": 0.3,
                },
              });
              currentMap.addLayer({
                id: "zone-boundary-stroke",
                type: "line",
                source: "zone-boundary",
                paint: {
                  "line-color": "#3b82f6",
                  "line-width": 2,
                },
              });
            }

            const coords =
              parsed.type === "Polygon"
                ? (parsed.coordinates as unknown as number[][][])[0]
                : ((parsed.coordinates as unknown as number[][][][])[0] as unknown as number[][])[0];

            if (coords && coords.length > 0) {
              const bounds = coords.reduce(
                (b, c) => b.extend(c as [number, number]),
                new mapboxgl.LngLatBounds(
                  coords[0] as [number, number],
                  coords[0] as [number, number]
                )
              );
              currentMap.fitBounds(bounds, { padding: 50 });
            }
          }
        }
      }
    });

    if (!readOnly && currentDraw) {
      currentMap.on("draw.create", handleDrawCreate);
      currentMap.on("draw.update", handleDrawUpdate);
      currentMap.on("draw.delete", handleDrawDelete);
    }

    return () => {
      currentMap.remove();
    };
  }, [handleDrawCreate, handleDrawUpdate, handleDrawDelete, initialGeoJson, readOnly]);

  if (!MAPBOX_TOKEN) {
    return (
      <div className="flex h-[400px] items-center justify-center rounded-lg border bg-muted">
        <p className="text-muted-foreground">
          Mapbox token not configured. Set NEXT_PUBLIC_MAPBOX_ACCESS_TOKEN.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      {!readOnly && (
        <p className="text-sm text-muted-foreground">
          Draw a polygon on the map to define the zone boundary. Click the polygon tool to start drawing.
        </p>
      )}
      <div ref={mapContainer} className="h-[400px] rounded-lg" />
    </div>
  );
}
