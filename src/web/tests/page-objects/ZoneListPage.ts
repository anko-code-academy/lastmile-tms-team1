import { Page } from "@playwright/test";

export class ZoneListPage {
  readonly page: Page;

  readonly addZoneButton = () => this.page.getByRole("button", { name: "Add Zone" });
  readonly loadingState = () => this.page.getByText("Loading zones...");
  readonly emptyState = () => this.page.getByText("No zones found. Create your first zone to get started.");
  readonly table = () => this.page.locator("table");
  readonly tableRows = () => this.table().locator("tbody tr");

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto("/zones");
  }

  async clickAddZone() {
    await this.addZoneButton().click();
    await this.page.waitForURL("/zones/new");
  }

  async clickEditZone(zoneId: string) {
    await this.page.getByRole("button", { name: "Edit zone" }).first().click();
    await this.page.waitForURL(`/zones/${zoneId}`);
  }

  getPencilButtonForZone(name: string) {
    const row = this.table().locator("tr", { has: this.page.getByText(name) });
    return row.getByRole("button").first();
  }

  getDeleteButtonForZone(name: string) {
    const row = this.table().locator("tr", { has: this.page.getByText(name) });
    return row.getByRole("button").nth(1);
  }

  async expectLoadingState() {
    await this.loadingState().waitFor({ state: "visible" });
  }

  async expectEmptyState() {
    await this.emptyState().waitFor({ state: "visible" });
  }

  async expectZoneInTable(name: string, isActive: boolean = true, depotName?: string) {
    if (depotName) {
      const depotSection = this.page.getByText(depotName).first();
      await depotSection.waitFor({ state: "visible" });
    }
    const row = this.table().locator("tr", { has: this.page.getByText(name) });
    await row.waitFor({ state: "visible" });
    const statusBadge = isActive ? "Active" : "Inactive";
    await row.getByText(statusBadge).waitFor({ state: "visible" });
  }

  async expectZonesGroupedByDepot(depotName: string) {
    await this.page.getByText(depotName).first().waitFor({ state: "visible" });
  }
}
