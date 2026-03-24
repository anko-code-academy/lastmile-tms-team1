import type { Feature, Polygon, MultiPolygon, Geometry } from "geojson";

export interface ZoneGeoJson {
  type: "Polygon" | "MultiPolygon";
  coordinates: number[][][] | number[][][][];
}

export function parseZoneGeoJson(geoJson: string): ZoneGeoJson | null {
  try {
    const parsed = JSON.parse(geoJson);
    if (parsed.type === "Polygon" || parsed.type === "MultiPolygon") {
      return parsed as ZoneGeoJson;
    }
    return null;
  } catch {
    return null;
  }
}

export function formatGeoJson(geoJson: ZoneGeoJson): string {
  return JSON.stringify(geoJson);
}

export function getZoneCenter(geoJson: string): [number, number] | null {
  const parsed = parseZoneGeoJson(geoJson);
  if (!parsed) return null;

  let coords: number[][][];
  if (parsed.type === "Polygon") {
    coords = parsed.coordinates as number[][][];
  } else {
    coords = (parsed.coordinates as number[][][][])[0];
  }

  const ring = coords[0];
  if (!ring || ring.length === 0) return null;

  let sumLat = 0;
  let sumLng = 0;
  ring.forEach((coord) => {
    sumLng += coord[0];
    sumLat += coord[1];
  });

  return [sumLng / ring.length, sumLat / ring.length];
}

export function createPolygonFeature(geoJson: ZoneGeoJson): Feature<Polygon | MultiPolygon> {
  return {
    type: "Feature",
    properties: {},
    geometry: geoJson as Geometry,
  } as Feature<Polygon | MultiPolygon>;
}
