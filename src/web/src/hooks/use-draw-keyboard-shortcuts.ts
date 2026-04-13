import { useEffect, type RefObject } from "react";

interface KeyboardShortcutOptions {
  undo: () => void;
  redo: () => void;
  canUndo: boolean;
  canRedo: boolean;
  containerRef: RefObject<HTMLElement | null>;
}

export function useDrawKeyboardShortcuts({
  undo,
  redo,
  canUndo,
  canRedo,
  containerRef,
}: KeyboardShortcutOptions): void {
  useEffect(() => {
    const el = containerRef.current;
    if (!el) return;

    function handleKeyDown(e: KeyboardEvent) {
      if (!e.ctrlKey && !e.metaKey) return;

      if (e.key === "z" && !e.shiftKey && canUndo) {
        e.preventDefault();
        undo();
      } else if (
        ((e.key === "Z" && e.shiftKey) || e.key === "y") &&
        canRedo
      ) {
        e.preventDefault();
        redo();
      }
    }

    el.addEventListener("keydown", handleKeyDown);
    return () => el.removeEventListener("keydown", handleKeyDown);
  }, [undo, redo, canUndo, canRedo, containerRef]);
}
