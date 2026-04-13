import { describe, it, expect } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useDrawHistory } from "./use-draw-history";

describe("useDrawHistory", () => {
  it("starts with empty present and no undo/redo", () => {
    const { result } = renderHook(() => useDrawHistory());

    expect(result.current.present).toBe("");
    expect(result.current.canUndo).toBe(false);
    expect(result.current.canRedo).toBe(false);
  });

  it("starts with initial snapshot when provided", () => {
    const initial = '{"type":"Polygon","coordinates":[[[0,0],[1,0],[1,1],[0,0]]]}';
    const { result } = renderHook(() =>
      useDrawHistory({ initialSnapshot: initial }),
    );

    expect(result.current.present).toBe(initial);
    expect(result.current.canUndo).toBe(false);
  });

  describe("pushSnapshot", () => {
    it("pushes new snapshot and enables undo", () => {
      const { result } = renderHook(() => useDrawHistory());

      act(() => {
        result.current.pushSnapshot('{"type":"Polygon","coordinates":[]}');
      });

      expect(result.current.present).toBe('{"type":"Polygon","coordinates":[]}');
      expect(result.current.canUndo).toBe(true);
      expect(result.current.canRedo).toBe(false);
    });

    it("clears redo stack on new push", () => {
      const { result } = renderHook(() => useDrawHistory());

      act(() => {
        result.current.pushSnapshot("a");
        result.current.pushSnapshot("b");
      });

      act(() => {
        result.current.undo();
      });

      expect(result.current.canRedo).toBe(true);

      act(() => {
        result.current.pushSnapshot("c");
      });

      expect(result.current.present).toBe("c");
      expect(result.current.canRedo).toBe(false);
    });

    it("caps history at maxHistorySize", () => {
      const { result } = renderHook(() =>
        useDrawHistory({ maxHistorySize: 3 }),
      );

      act(() => {
        result.current.pushSnapshot("1");
        result.current.pushSnapshot("2");
        result.current.pushSnapshot("3");
        result.current.pushSnapshot("4");
      });

      // After pushes: past=["", "1", "2", "3"] trimmed to ["1","2","3"], present="4"
      expect(result.current.present).toBe("4");

      act(() => { result.current.undo(); });
      expect(result.current.present).toBe("3");

      act(() => { result.current.undo(); });
      expect(result.current.present).toBe("2");

      act(() => { result.current.undo(); });
      expect(result.current.present).toBe("1");

      // "" was trimmed from past, can't undo further
      expect(result.current.canUndo).toBe(false);
    });
  });

  describe("undo", () => {
    it("restores previous snapshot and enables redo", () => {
      const { result } = renderHook(() => useDrawHistory());

      act(() => {
        result.current.pushSnapshot("a");
        result.current.pushSnapshot("b");
      });

      act(() => {
        result.current.undo();
      });

      expect(result.current.present).toBe("a");
      expect(result.current.canUndo).toBe(true);
      expect(result.current.canRedo).toBe(true);
    });

    it("is a no-op when past is empty", () => {
      const { result } = renderHook(() => useDrawHistory());

      act(() => {
        result.current.undo();
      });

      expect(result.current.present).toBe("");
      expect(result.current.canUndo).toBe(false);
    });

    it("can undo multiple times in sequence", () => {
      const { result } = renderHook(() => useDrawHistory());

      act(() => {
        result.current.pushSnapshot("a");
        result.current.pushSnapshot("b");
        result.current.pushSnapshot("c");
      });

      act(() => { result.current.undo(); });
      expect(result.current.present).toBe("b");

      act(() => { result.current.undo(); });
      expect(result.current.present).toBe("a");

      act(() => { result.current.undo(); });
      expect(result.current.present).toBe("");
      expect(result.current.canUndo).toBe(false);
    });
  });

  describe("redo", () => {
    it("restores next snapshot after undo", () => {
      const { result } = renderHook(() => useDrawHistory());

      act(() => {
        result.current.pushSnapshot("a");
        result.current.pushSnapshot("b");
      });

      act(() => { result.current.undo(); });
      expect(result.current.present).toBe("a");

      act(() => { result.current.redo(); });
      expect(result.current.present).toBe("b");
      expect(result.current.canRedo).toBe(false);
    });

    it("is a no-op when future is empty", () => {
      const { result } = renderHook(() => useDrawHistory());

      act(() => {
        result.current.redo();
      });

      expect(result.current.present).toBe("");
    });

    it("undo then redo returns to the same state", () => {
      const { result } = renderHook(() => useDrawHistory());

      act(() => {
        result.current.pushSnapshot("a");
        result.current.pushSnapshot("b");
        result.current.pushSnapshot("c");
      });

      act(() => { result.current.undo(); });
      act(() => { result.current.undo(); });
      expect(result.current.present).toBe("a");

      act(() => { result.current.redo(); });
      expect(result.current.present).toBe("b");

      act(() => { result.current.redo(); });
      expect(result.current.present).toBe("c");
    });
  });

  describe("reset", () => {
    it("replaces all state", () => {
      const { result } = renderHook(() => useDrawHistory());

      act(() => {
        result.current.pushSnapshot("a");
        result.current.pushSnapshot("b");
      });

      act(() => { result.current.undo(); });
      expect(result.current.canRedo).toBe(true);

      act(() => {
        result.current.reset("fresh");
      });

      expect(result.current.present).toBe("fresh");
      expect(result.current.canUndo).toBe(false);
      expect(result.current.canRedo).toBe(false);
    });
  });
});
