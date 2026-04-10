import { describe, it, expect, vi } from "vitest";
import type MapboxDraw from "@mapbox/mapbox-gl-draw";
import { syncDrawToSnapshot } from "./draw-history-sync";

function createMockDraw() {
  return {
    deleteAll: vi.fn(),
    add: vi.fn(() => ["mock-feature-id"]),
    getAll: vi.fn(() => ({ features: [] })),
    changeMode: vi.fn(),
  };
}

describe("syncDrawToSnapshot", () => {
  it("deletes existing features and adds parsed feature for non-empty target", () => {
    const draw = createMockDraw();
    const targetGeoJson = '{"type":"Polygon","coordinates":[[[0,0],[1,0],[1,1],[0,0]]]}';

    syncDrawToSnapshot({ draw: draw as unknown as MapboxDraw, targetGeoJson });

    expect(draw.deleteAll).toHaveBeenCalledOnce();
    expect(draw.add).toHaveBeenCalledOnce();
    // The added feature should have the parsed geometry
    const addedFeature = draw.add.mock.calls[0][0];
    expect(addedFeature.type).toBe("Feature");
    expect(addedFeature.geometry.type).toBe("Polygon");
  });

  it("handles empty target by deleting all features without adding", () => {
    const draw = createMockDraw();

    syncDrawToSnapshot({ draw: draw as unknown as MapboxDraw, targetGeoJson: "" });

    expect(draw.deleteAll).toHaveBeenCalledOnce();
    expect(draw.add).not.toHaveBeenCalled();
  });

  it("handles invalid GeoJSON gracefully by deleting without adding", () => {
    const draw = createMockDraw();

    syncDrawToSnapshot({ draw: draw as unknown as MapboxDraw, targetGeoJson: "not-json" });

    expect(draw.deleteAll).toHaveBeenCalledOnce();
    expect(draw.add).not.toHaveBeenCalled();
  });
});
