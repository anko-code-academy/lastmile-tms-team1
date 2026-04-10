import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook } from "@testing-library/react";
import { useDrawKeyboardShortcuts } from "./use-draw-keyboard-shortcuts";

describe("useDrawKeyboardShortcuts", () => {
  let container: HTMLDivElement;

  beforeEach(() => {
    container = document.createElement("div");
    document.body.appendChild(container);
  });

  function fireKeyDown(
    target: HTMLElement,
    key: string,
    ctrlKey = false,
    shiftKey = false,
  ) {
    target.dispatchEvent(
      new KeyboardEvent("keydown", {
        key,
        ctrlKey,
        shiftKey,
        bubbles: true,
        cancelable: true,
      }),
    );
  }

  it("calls undo on Ctrl+Z", () => {
    const undo = vi.fn();
    const redo = vi.fn();

    renderHook(() =>
      useDrawKeyboardShortcuts({
        undo,
        redo,
        canUndo: true,
        canRedo: false,
        containerRef: { current: container },
      }),
    );

    fireKeyDown(container, "z", true);

    expect(undo).toHaveBeenCalledTimes(1);
    expect(redo).not.toHaveBeenCalled();
  });

  it("does not call undo when canUndo is false", () => {
    const undo = vi.fn();

    renderHook(() =>
      useDrawKeyboardShortcuts({
        undo,
        redo: vi.fn(),
        canUndo: false,
        canRedo: false,
        containerRef: { current: container },
      }),
    );

    fireKeyDown(container, "z", true);

    expect(undo).not.toHaveBeenCalled();
  });

  it("calls redo on Ctrl+Shift+Z", () => {
    const undo = vi.fn();
    const redo = vi.fn();

    renderHook(() =>
      useDrawKeyboardShortcuts({
        undo,
        redo,
        canUndo: false,
        canRedo: true,
        containerRef: { current: container },
      }),
    );

    fireKeyDown(container, "Z", true, true);

    expect(redo).toHaveBeenCalledTimes(1);
    expect(undo).not.toHaveBeenCalled();
  });

  it("calls redo on Ctrl+Y", () => {
    const redo = vi.fn();

    renderHook(() =>
      useDrawKeyboardShortcuts({
        undo: vi.fn(),
        redo,
        canUndo: false,
        canRedo: true,
        containerRef: { current: container },
      }),
    );

    fireKeyDown(container, "y", true);

    expect(redo).toHaveBeenCalledTimes(1);
  });

  it("does not call redo when canRedo is false", () => {
    const redo = vi.fn();

    renderHook(() =>
      useDrawKeyboardShortcuts({
        undo: vi.fn(),
        redo,
        canUndo: false,
        canRedo: false,
        containerRef: { current: container },
      }),
    );

    fireKeyDown(container, "y", true);
    fireKeyDown(container, "Z", true, true);

    expect(redo).not.toHaveBeenCalled();
  });

  it("does not trigger on plain key presses without Ctrl", () => {
    const undo = vi.fn();
    const redo = vi.fn();

    renderHook(() =>
      useDrawKeyboardShortcuts({
        undo,
        redo,
        canUndo: true,
        canRedo: true,
        containerRef: { current: container },
      }),
    );

    fireKeyDown(container, "z");
    fireKeyDown(container, "y");

    expect(undo).not.toHaveBeenCalled();
    expect(redo).not.toHaveBeenCalled();
  });

  it("cleans up listener on unmount", () => {
    const undo = vi.fn();

    const { unmount } = renderHook(() =>
      useDrawKeyboardShortcuts({
        undo,
        redo: vi.fn(),
        canUndo: true,
        canRedo: false,
        containerRef: { current: container },
      }),
    );

    unmount();

    fireKeyDown(container, "z", true);

    expect(undo).not.toHaveBeenCalled();
  });
});
