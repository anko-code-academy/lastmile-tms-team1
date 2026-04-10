"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import mapboxgl from "mapbox-gl";
import MapboxDraw from "@mapbox/mapbox-gl-draw";
import { Undo2, Redo2 } from "lucide-react";
import "mapbox-gl/dist/mapbox-gl.css";
import "@mapbox/mapbox-gl-draw/dist/mapbox-gl-draw.css";
import {
  parseZoneGeoJson,
  formatGeoJson,
  createPolygonFeature,
  getZoneCenter,
  type ZoneGeoJson,
} from "@/components/map/map-utils";
import { syncDrawToSnapshot } from "@/components/map/draw-history-sync";
import { useDrawHistory } from "@/hooks/use-draw-history";
import { useDrawKeyboardShortcuts } from "@/hooks/use-draw-keyboard-shortcuts";

const MAPBOX_TOKEN = process.env.NEXT_PUBLIC_MAPBOX_ACCESS_TOKEN ?? "";

interface ZoneBoundaryMapProps {
  initialGeoJson?: string;
  onGeoJsonChange: (geoJson: string) => void;
  readOnly?: boolean;
  existingZones?: Array<{
    id: string;
    name: string;
    boundaryGeometry: unknown;
  }>;
  depotLocation?: { lng: number; lat: number } | null;
  onZoneSelect?: (zoneId: string) => void;
}

const ZONE_COLORS = [
  "#3b82f6", // blue
  "#10b981", // green
  "#f59e0b", // amber
  "#ef4444", // red
  "#8b5cf6", // violet
  "#ec4899", // pink
  "#06b6d4", // cyan
  "#84cc16", // lime
];

function getZoneColor(index: number): string {
  return ZONE_COLORS[index % ZONE_COLORS.length];
}

export function ZoneBoundaryMap({
  initialGeoJson,
  onGeoJsonChange,
  readOnly = false,
  existingZones = [],
  depotLocation,
  onZoneSelect,
}: ZoneBoundaryMapProps) {
  const mapContainer = useRef<HTMLDivElement>(null);
  const map = useRef<mapboxgl.Map | null>(null);
  const draw = useRef<MapboxDraw | null>(null);
  const depotMarker = useRef<mapboxgl.Marker | null>(null);
  const [selectedZoneId, setSelectedZoneId] = useState<string | null>(null);
  const addedZoneLayersRef = useRef<Set<string>>(new Set());
  const mapInitializedRef = useRef(false);
  // State (not just ref) so the initial-polygon load effect re-runs when map becomes ready
  const [mapReady, setMapReady] = useState(false);

  // ── Refs for all props/callbacks used inside stable closures ──
  const onGeoJsonChangeRef = useRef(onGeoJsonChange);
  useEffect(() => { onGeoJsonChangeRef.current = onGeoJsonChange; }, [onGeoJsonChange]);

  const onZoneSelectRef = useRef(onZoneSelect);
  useEffect(() => { onZoneSelectRef.current = onZoneSelect; }, [onZoneSelect]);

  const readOnlyRef = useRef(readOnly);
  useEffect(() => { readOnlyRef.current = readOnly; }, [readOnly]);

  const depotLocationRef = useRef(depotLocation);
  useEffect(() => { depotLocationRef.current = depotLocation; }, [depotLocation]);

  const existingZonesRef = useRef(existingZones);
  useEffect(() => { existingZonesRef.current = existingZones; }, [existingZones]);

  // ── Undo/redo history ──
  const history = useDrawHistory({ initialSnapshot: "" });
  const historyRef = useRef(history);
  useEffect(() => { historyRef.current = history; }, [history]);

  // Track whether a pending undo/redo sync is needed
  const [syncVersion, setSyncVersion] = useState(0);

  const handleUndo = useCallback(() => {
    historyRef.current.undo();
    setSyncVersion((v) => v + 1);
  }, []);
  const handleRedo = useCallback(() => {
    historyRef.current.redo();
    setSyncVersion((v) => v + 1);
  }, []);

  useDrawKeyboardShortcuts({
    undo: handleUndo,
    redo: handleRedo,
    canUndo: history.canUndo,
    canRedo: history.canRedo,
    containerRef: mapContainer,
  });

  // Sync MapboxDraw visual when history changes via undo/redo
  const syncCountRef = useRef(0);
  useEffect(() => {
    syncCountRef.current++;
    if (syncCountRef.current <= 1) return;
    if (!draw.current || !mapInitializedRef.current) return;

    syncDrawToSnapshot({ draw: draw.current, targetGeoJson: historyRef.current.present });
    onGeoJsonChangeRef.current(historyRef.current.present);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [syncVersion]);

  // ── Stable draw event handlers (empty deps — use refs) ──
  const handleDrawCreate = useCallback(
    (e: { features: GeoJSON.Feature[] }) => {
      if (e.features.length > 0) {
        const feature = e.features[0];
        if (
          feature.geometry.type === "Polygon" ||
          feature.geometry.type === "MultiPolygon"
        ) {
          const geoJson: ZoneGeoJson = {
            type: feature.geometry.type,
            coordinates: feature.geometry.coordinates,
          };
          const snapshot = formatGeoJson(geoJson);
          historyRef.current.pushSnapshot(snapshot);
          onGeoJsonChangeRef.current(snapshot);
        }
      }
    },
    [],
  );

  const handleDrawUpdate = useCallback(
    (e: { features: GeoJSON.Feature[] }) => {
      if (e.features.length > 0) {
        const feature = e.features[0];
        if (
          feature.geometry.type === "Polygon" ||
          feature.geometry.type === "MultiPolygon"
        ) {
          const geoJson: ZoneGeoJson = {
            type: feature.geometry.type,
            coordinates: feature.geometry.coordinates,
          };
          const snapshot = formatGeoJson(geoJson);
          historyRef.current.pushSnapshot(snapshot);
          onGeoJsonChangeRef.current(snapshot);
        }
      }
    },
    [],
  );

  const handleDrawDelete = useCallback(() => {
    historyRef.current.pushSnapshot("");
    onGeoJsonChangeRef.current("");
  }, []);

  // ── Stable overlay helper (empty deps — use refs) ──
  const addZoneOverlayLayer = useCallback(
    (
      currentMap: mapboxgl.Map,
      zone: { id: string; name: string; boundaryGeometry: unknown },
      index: number,
    ) => {
      if (!zone.boundaryGeometry) return;

      const geoJsonStr =
        typeof zone.boundaryGeometry === "string"
          ? zone.boundaryGeometry
          : JSON.stringify(zone.boundaryGeometry);

      const parsed = parseZoneGeoJson(geoJsonStr);
      if (!parsed) return;

      const feature = createPolygonFeature(parsed);
      const color = getZoneColor(index);
      const sourceId = `zone-overlay-${zone.id}`;
      const fillLayerId = `zone-overlay-fill-${zone.id}`;
      const strokeLayerId = `zone-overlay-stroke-${zone.id}`;
      const labelLayerId = `zone-overlay-label-${zone.id}`;

      if (currentMap.getSource(sourceId)) return;

      currentMap.addSource(sourceId, {
        type: "geojson",
        data: feature as GeoJSON.Feature,
      });
      addedZoneLayersRef.current.add(sourceId);

      currentMap.addLayer({
        id: fillLayerId,
        type: "fill",
        source: sourceId,
        paint: {
          "fill-color": color,
          "fill-opacity": 0.2,
        },
      });
      addedZoneLayersRef.current.add(fillLayerId);

      currentMap.addLayer({
        id: strokeLayerId,
        type: "line",
        source: sourceId,
        paint: {
          "line-color": color,
          "line-width": 2,
        },
      });
      addedZoneLayersRef.current.add(strokeLayerId);

      const center = getZoneCenter(geoJsonStr);
      if (center) {
        const labelSourceId = `zone-label-${zone.id}`;
        currentMap.addSource(labelSourceId, {
          type: "geojson",
          data: {
            type: "Feature",
            properties: { name: zone.name },
            geometry: {
              type: "Point",
              coordinates: center,
            },
          } as GeoJSON.Feature,
        });
        addedZoneLayersRef.current.add(labelSourceId);

        currentMap.addLayer({
          id: labelLayerId,
          type: "symbol",
          source: labelSourceId,
          layout: {
            "text-field": ["get", "name"],
            "text-size": 12,
            "text-font": ["DIN Pro Regular", "Arial Unicode MS Regular"],
            "text-anchor": "center",
          },
          paint: {
            "text-color": "#ffffff",
            "text-halo-color": color,
            "text-halo-width": 1.5,
          },
        });
        addedZoneLayersRef.current.add(labelLayerId);
      }

      currentMap.on("mouseenter", fillLayerId, () => {
        currentMap.setPaintProperty(fillLayerId, "fill-opacity", 0.4);
        currentMap.getCanvas().style.cursor = "pointer";
      });

      currentMap.on("mouseleave", fillLayerId, () => {
        currentMap.setPaintProperty(fillLayerId, "fill-opacity", 0.2);
        currentMap.getCanvas().style.cursor = "";
      });

      currentMap.on("click", fillLayerId, () => {
        setSelectedZoneId(zone.id);
        currentMap.setPaintProperty(fillLayerId, "fill-opacity", 0.5);
        onZoneSelectRef.current?.(zone.id);
      });
    },
    [],
  );

  const cleanupZoneLayers = useCallback((currentMap: mapboxgl.Map) => {
    addedZoneLayersRef.current.forEach((layerId) => {
      if (currentMap.getLayer(layerId)) {
        currentMap.removeLayer(layerId);
      }
      if (layerId.startsWith("zone-overlay-") || layerId.startsWith("zone-label-")) {
        const sourceId = layerId.replace("-fill", "").replace("-stroke", "").replace("-label", "");
        if (currentMap.getSource(sourceId)) {
          currentMap.removeSource(sourceId);
        }
      }
    });
    addedZoneLayersRef.current.clear();
  }, []);

  // ── Main map setup — runs ONCE ──
  useEffect(() => {
    if (!mapContainer.current || !MAPBOX_TOKEN) return;

    mapboxgl.accessToken = MAPBOX_TOKEN;

    map.current = new mapboxgl.Map({
      container: mapContainer.current,
      style: "mapbox://styles/mapbox/streets-v12",
      center: [-74.006, 40.7128],
      zoom: 12,
    });

    if (!readOnlyRef.current) {
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
          {
            id: "gl-draw-polygon-and-line-vertex-active",
            type: "circle",
            filter: ["all", ["==", "meta", "vertex"], ["==", "$type", "Point"]],
            paint: {
              "circle-radius": 7,
              "circle-color": "#3b82f6",
            },
          },
          {
            id: "gl-draw-polygon-midpoint",
            type: "circle",
            filter: ["all", ["==", "meta", "midpoint"], ["==", "$type", "Point"]],
            paint: {
              "circle-radius": 5,
              "circle-color": "#ffffff",
              "circle-stroke-color": "#3b82f6",
              "circle-stroke-width": 2,
            },
          },
        ],
      });

      map.current.addControl(draw.current);
    }

    const currentMap = map.current;
    const currentDraw = draw.current;

    currentMap.on("load", () => {
      mapInitializedRef.current = true;
      setMapReady(true);

      // Add existing zones as overlay layers
      existingZonesRef.current.forEach((zone, index) => {
        addZoneOverlayLayer(currentMap, zone, index);
      });

      // Add depot marker
      const loc = depotLocationRef.current;
      if (loc) {
        depotMarker.current = new mapboxgl.Marker({ color: "#ef4444" })
          .setLngLat([loc.lng, loc.lat])
          .addTo(currentMap);
      }
    });

    if (!readOnlyRef.current && currentDraw) {
      currentMap.on("draw.create", handleDrawCreate);
      currentMap.on("draw.update", handleDrawUpdate);
      currentMap.on("draw.delete", handleDrawDelete);
    }

    return () => {
      if (depotMarker.current) {
        depotMarker.current.remove();
      }
      cleanupZoneLayers(currentMap);
      currentMap.remove();
      mapInitializedRef.current = false;
      setMapReady(false);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ── Load initial polygon into draw when zone data arrives (async) ──
  // Uses a ref flag to ensure the polygon is loaded exactly once — prevents
  // re-loading on drawing feedback (onGeoJsonChange → parent updates geoJson →
  // passed back as initialGeoJson).
  const hasLoadedInitialPolygon = useRef(false);
  useEffect(() => {
    if (!mapReady || !draw.current) return;
    if (!initialGeoJson) return;
    if (hasLoadedInitialPolygon.current) return;

    const parsed = parseZoneGeoJson(initialGeoJson);
    if (!parsed) return;

    hasLoadedInitialPolygon.current = true;
    const currentDraw = draw.current;

    if (!readOnlyRef.current) {
      // Clear any stale features first (e.g. from strict mode re-mount)
      currentDraw.deleteAll();
      const feature = createPolygonFeature(parsed);
      const addedIds = currentDraw.add(feature as GeoJSON.Feature);
      if (addedIds.length > 0) {
        currentDraw.changeMode("direct_select", {
          featureId: addedIds[0],
        });
      }
      // Initialize history with this snapshot
      historyRef.current.reset(initialGeoJson);
    }

    // Fit bounds to zone
    const coords =
      parsed.type === "Polygon"
        ? (parsed.coordinates as unknown as number[][][])[0]
        : (
            (parsed.coordinates as unknown as number[][][][])[0] as unknown as number[][]
          )[0];

    if (coords && coords.length > 0) {
      const bounds = coords.reduce(
        (b, c) => b.extend(c as [number, number]),
        new mapboxgl.LngLatBounds(
          coords[0] as [number, number],
          coords[0] as [number, number],
        ),
      );
      map.current?.fitBounds(bounds, { padding: 50 });
    }
  }, [mapReady, initialGeoJson]);

  // ── Handle existingZones changes after map is initialized ──
  useEffect(() => {
    if (!map.current || !mapInitializedRef.current) return;

    const currentMap = map.current;

    const handleIdle = () => {
      existingZones.forEach((zone, index) => {
        const sourceId = `zone-overlay-${zone.id}`;
        if (!currentMap.getSource(sourceId)) {
          addZoneOverlayLayer(currentMap, zone, index);
        }
      });

      const currentZoneIds = new Set(existingZones.map((z) => z.id));
      const toRemove: string[] = [];

      addedZoneLayersRef.current.forEach((layerId) => {
        if (layerId.startsWith("zone-overlay-fill-")) {
          const zoneId = layerId.replace("zone-overlay-fill-", "");
          if (!currentZoneIds.has(zoneId)) {
            toRemove.push(zoneId);
          }
        }
      });

      toRemove.forEach((zoneId) => {
        const fillLayerId = `zone-overlay-fill-${zoneId}`;
        const strokeLayerId = `zone-overlay-stroke-${zoneId}`;
        const labelLayerId = `zone-overlay-label-${zoneId}`;
        const sourceId = `zone-overlay-${zoneId}`;
        const labelSourceId = `zone-label-${zoneId}`;

        [fillLayerId, strokeLayerId, labelLayerId].forEach((id) => {
          if (currentMap.getLayer(id)) currentMap.removeLayer(id);
        });
        if (currentMap.getSource(sourceId)) currentMap.removeSource(sourceId);
        if (currentMap.getSource(labelSourceId)) currentMap.removeSource(labelSourceId);
        addedZoneLayersRef.current.delete(fillLayerId);
        addedZoneLayersRef.current.delete(strokeLayerId);
        addedZoneLayersRef.current.delete(labelLayerId);
        addedZoneLayersRef.current.delete(sourceId);
        addedZoneLayersRef.current.delete(labelSourceId);
      });

      currentMap.off("idle", handleIdle);
    };

    if (currentMap.loaded() && !currentMap.isMoving()) {
      handleIdle();
    } else {
      currentMap.on("idle", handleIdle);
    }
  }, [existingZones, addZoneOverlayLayer]);

  // ── Update zone opacities when selectedZoneId changes ──
  useEffect(() => {
    if (!map.current || !mapInitializedRef.current) return;

    existingZones.forEach((zone) => {
      const fillLayerId = `zone-overlay-fill-${zone.id}`;
      if (map.current!.getLayer(fillLayerId)) {
        const opacity = selectedZoneId === zone.id ? 0.5 : 0.2;
        map.current!.setPaintProperty(fillLayerId, "fill-opacity", opacity);
      }
    });
  }, [selectedZoneId, existingZones]);

  // ── Update depot marker when depotLocation changes ──
  useEffect(() => {
    if (!map.current || !mapInitializedRef.current) return;
    if (depotMarker.current) {
      depotMarker.current.remove();
      depotMarker.current = null;
    }
    if (depotLocation) {
      depotMarker.current = new mapboxgl.Marker({ color: "#ef4444" })
        .setLngLat([depotLocation.lng, depotLocation.lat])
        .addTo(map.current!);
    }
  }, [depotLocation]);

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
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            {initialGeoJson
              ? "Drag vertices to edit the zone boundary. Click midpoints on edges to add vertices."
              : "Draw a polygon on the map to define the zone boundary. Click the polygon tool to start drawing."}
          </p>
          <div className="flex gap-1">
            <button
              type="button"
              onClick={handleUndo}
              disabled={!history.canUndo}
              className="rounded p-1.5 text-muted-foreground hover:bg-muted disabled:opacity-40 disabled:cursor-not-allowed"
              title="Undo (Ctrl+Z)"
              aria-label="Undo"
            >
              <Undo2 className="h-4 w-4" />
            </button>
            <button
              type="button"
              onClick={handleRedo}
              disabled={!history.canRedo}
              className="rounded p-1.5 text-muted-foreground hover:bg-muted disabled:opacity-40 disabled:cursor-not-allowed"
              title="Redo (Ctrl+Shift+Z)"
              aria-label="Redo"
            >
              <Redo2 className="h-4 w-4" />
            </button>
          </div>
        </div>
      )}
      <div ref={mapContainer} className="h-[400px] rounded-lg" />
    </div>
  );
}
