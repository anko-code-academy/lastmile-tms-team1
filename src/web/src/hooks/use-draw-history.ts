import { useCallback, useEffect, useRef, useState } from "react";

export interface DrawHistoryState {
  present: string;
  canUndo: boolean;
  canRedo: boolean;
  pushSnapshot: (geoJson: string) => void;
  undo: () => void;
  redo: () => void;
  reset: (initialSnapshot: string) => void;
}

interface DrawHistoryOptions {
  initialSnapshot?: string;
  maxHistorySize?: number;
}

interface HistoryEntry {
  past: string[];
  present: string;
  future: string[];
}

export function useDrawHistory(options: DrawHistoryOptions = {}): DrawHistoryState {
  const { initialSnapshot = "", maxHistorySize = 50 } = options;

  const [entry, setEntry] = useState<HistoryEntry>({
    past: [],
    present: initialSnapshot,
    future: [],
  });

  const maxRef = useRef(maxHistorySize);
  useEffect(() => {
    maxRef.current = maxHistorySize;
  }, [maxHistorySize]);

  const pushSnapshot = useCallback((geoJson: string) => {
    setEntry((prev) => {
      const newPast = [...prev.past, prev.present];
      const trimmed = newPast.length > maxRef.current
        ? newPast.slice(newPast.length - maxRef.current)
        : newPast;
      return { past: trimmed, present: geoJson, future: [] };
    });
  }, []);

  const undo = useCallback(() => {
    setEntry((prev) => {
      if (prev.past.length === 0) return prev;
      const newPast = [...prev.past];
      const previous = newPast.pop()!;
      return {
        past: newPast,
        present: previous,
        future: [...prev.future, prev.present],
      };
    });
  }, []);

  const redo = useCallback(() => {
    setEntry((prev) => {
      if (prev.future.length === 0) return prev;
      const newFuture = [...prev.future];
      const next = newFuture.pop()!;
      return {
        past: [...prev.past, prev.present],
        present: next,
        future: newFuture,
      };
    });
  }, []);

  const reset = useCallback((initial: string) => {
    setEntry({ past: [], present: initial, future: [] });
  }, []);

  return {
    present: entry.present,
    canUndo: entry.past.length > 0,
    canRedo: entry.future.length > 0,
    pushSnapshot,
    undo,
    redo,
    reset,
  };
}
