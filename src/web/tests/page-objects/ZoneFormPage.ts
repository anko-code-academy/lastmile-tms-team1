import { Page, expect } from "@playwright/test";

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

  async mockGeoJson() {
    // Mock the GeoJSON state directly to bypass map drawing
    await this.page.evaluate(() => {
      // Find the state setters and call them directly
      const stateSetters = (window as unknown as { __zoneFormState?: { setGeoJson?: (geojson: string) => void } }).__zoneFormState;
      if (stateSetters?.setGeoJson) {
        stateSetters.setGeoJson('{"type":"Polygon","coordinates":[[[-74.006,40.7128],[-74.006,40.7129],[-74.005,40.7129],[-74.005,40.7128],[-74.006,40.7128]]]}');
      }
    });

    // Alternative approach - dispatch custom event that the map listens to
    await this.page.evaluate(() => {
      // Set GeoJSON directly on window for testing
      (window as unknown as { __testGeoJson?: string }).__testGeoJson = '{"type":"Polygon","coordinates":[[[-74.006,40.7128],[-74.006,40.7129],[-74.005,40.7129],[-74.005,40.7128],[-74.006,40.7128]]]}';
    });
  }

  async setGeoJsonDirectly(geoJson: string) {
    // This is a fallback method to set geoJSON when we can't interact with the map
    // We look for any state setter or event that might update the zone
    await this.page.evaluate((gj) => {
      // Try to find and call the state setter if exposed
      const stateSetters = (window as unknown as Record<string, unknown>).__zoneFormState;
      if (stateSetters && typeof (stateSetters as { setGeoJson?: (g: string) => void }).setGeoJson === "function") {
        (stateSetters as { setGeoJson: (g: string) => void }).setGeoJson(gj);
      }
    }, geoJson);
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
