import { Page } from "@playwright/test";

export class ZoneFormPage {
  readonly page: Page;

  readonly nameInput = () => this.page.getByPlaceholder("Enter zone name");
  readonly depotSelect = () => this.page.getByRole("combobox", { name: /depot/i });
  readonly activeCheckbox = () => this.page.locator("input[type='checkbox']");
  readonly submitButton = () => this.page.getByRole("button", { name: /Create Zone|Update Zone/ });
  readonly cancelButton = () => this.page.getByRole("button", { name: "Cancel" });
  readonly mapContainer = () => this.page.locator(".mapboxgl-map, [class*='map']").first();

  constructor(page: Page) {
    this.page = page;
  }

  async gotoCreate() {
    await this.page.goto("/zones/new");
  }

  async gotoEdit(zoneId: string) {
    await this.page.goto(`/zones/${zoneId}`);
  }

  async fillName(name: string) {
    await this.nameInput().fill(name);
  }

  async selectDepot(depotName: string) {
    await this.depotSelect().click();
    await this.page.getByRole("option", { name: depotName }).click();
  }

  async toggleActive() {
    await this.activeCheckbox().click();
  }

  async submit() {
    await this.submitButton().click();
  }

  async cancel() {
    await this.cancelButton().click();
  }

  /**
   * Draw a triangle polygon by dispatching pointer events directly to the canvas.
   * Uses page.evaluate to send native pointer events, bypassing Playwright's mouse
   * simulation which can have issues with WebGL canvas event handling in MapboxGL v3.
   */
  async drawPolygon() {
    const map = this.mapContainer();
    await map.waitFor({ state: "visible" });

    // Wait for draw control to be fully loaded
    await this.page.waitForSelector(".mapbox-gl-draw_polygon", { state: "visible", timeout: 10000 });
    await this.page.waitForTimeout(1000);

    // Click the polygon tool to enter draw mode
    await this.page.locator(".mapbox-gl-draw_polygon").click();
    await this.page.waitForTimeout(500);

    // Get the canvas element
    const canvas = await this.page.$(".mapboxgl-map canvas");
    if (!canvas) throw new Error("Map canvas not found");

    const box = await canvas.boundingBox();
    if (!box) throw new Error("Canvas bounding box not found");

    const cx = box.x + box.width / 2;
    const cy = box.y + box.height / 2;
    const offset = box.width * 0.15;

    const p1 = { x: cx - offset, y: cy - offset };
    const p2 = { x: cx + offset, y: cy - offset };
    const p3 = { x: cx + offset, y: cy + offset };

    // Helper to send a complete pointer sequence (down + up) at a position
    const pointerClick = (x: number, y: number) =>
      this.page.evaluate(
        ([px, py]) => {
          const el = document.elementFromPoint(px, py) as HTMLElement;
          if (!el) return;
          const opts = { pointerX: px, pointerY: py, bubbles: true, cancelable: true };
          el.dispatchEvent(new PointerEvent("pointerdown", opts));
          el.dispatchEvent(new PointerEvent("pointerup", opts));
        },
        [x, y]
      );

    // Double-click helper
    const pointerDblClick = (x: number, y: number) =>
      this.page.evaluate(
        ([px, py]) => {
          const el = document.elementFromPoint(px, py) as HTMLElement;
          if (!el) return;
          const opts = { pointerX: px, pointerY: py, bubbles: true, cancelable: true };
          el.dispatchEvent(new PointerEvent("pointerdown", opts));
          el.dispatchEvent(new PointerEvent("pointerup", opts));
          el.dispatchEvent(new MouseEvent("dblclick", opts));
        },
        [x, y]
      );

    await pointerClick(p1.x, p1.y);
    await this.page.waitForTimeout(200);
    await pointerClick(p2.x, p2.y);
    await this.page.waitForTimeout(200);
    await pointerDblClick(p3.x, p3.y);
    await this.page.waitForTimeout(500);
  }

  async setGeoJsonDirectly() {
    await this.drawPolygon();
  }

  async expectValidationError(message: string) {
    await this.page.getByText(message).waitFor({ state: "visible" });
  }

  async expectDepotValidationError() {
    await this.expectValidationError("Depot is required");
  }

  async expectZoneBoundaryValidationError() {
    await this.expectValidationError("Please draw a zone boundary on the map");
  }
}
