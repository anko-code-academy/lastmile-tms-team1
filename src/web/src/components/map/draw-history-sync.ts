import type MapboxDraw from "@mapbox/mapbox-gl-draw";
import { parseZoneGeoJson, createPolygonFeature } from "./map-utils";

interface SyncOptions {
  draw: MapboxDraw;
  targetGeoJson: string;
}

/**
 * Synchronizes the MapboxDraw instance's visual state to match the given
 * GeoJSON snapshot. Used after undo/redo to update what the map shows.
 */
export function syncDrawToSnapshot({ draw, targetGeoJson }: SyncOptions): void {
  draw.deleteAll();

  if (!targetGeoJson) return;

  const parsed = parseZoneGeoJson(targetGeoJson);
  if (!parsed) return;

  const feature = createPolygonFeature(parsed);
  draw.add(feature as GeoJSON.Feature);
}
