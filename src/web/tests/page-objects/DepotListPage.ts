import { Page, expect } from "@playwright/test";

export class DepotListPage {
  readonly page: Page;

  readonly addDepotButton = () => this.page.getByRole("button", { name: "Add Depot" });
  readonly loadingState = () => this.page.getByText("Loading depots...");
  readonly emptyState = () => this.page.getByText("No depots found. Create your first depot to get started.");
  readonly table = () => this.page.locator("table");
  readonly tableRows = () => this.table().locator("tbody tr");

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto("/depots");
  }

  async clickAddDepot() {
    await this.addDepotButton().click();
    await this.page.waitForURL("/depots/new");
  }

  async clickEditDepot(depotId: string) {
    await this.page.getByRole("button", { name: "Edit depot" }).first().click();
    await this.page.waitForURL(`/depots/${depotId}`);
  }

  getPencilButtonForDepot(name: string) {
    const row = this.table().locator("tr", { has: this.page.getByText(name) });
    return row.getByRole("button").first();
  }

  getDeleteButtonForDepot(name: string) {
    const row = this.table().locator("tr", { has: this.page.getByText(name) });
    return row.getByRole("button").nth(1);
  }

  async expectLoadingState() {
    await this.loadingState().waitFor({ state: "visible" });
  }

  async expectEmptyState() {
    await this.emptyState().waitFor({ state: "visible" });
  }

  async expectDepotInTable(name: string, isActive: boolean = true) {
    const row = this.table().locator("tr", { has: this.page.getByText(name) });
    await row.waitFor({ state: "visible" });
    const statusBadge = isActive ? "Active" : "Inactive";
    await row.getByText(statusBadge).waitFor({ state: "visible" });
  }

  async expectTableHeaders(columns: string[]) {
    const headerRow = this.table().locator("thead tr");
    for (const col of columns) {
      await headerRow.getByText(col).waitFor({ state: "visible" });
    }
  }
}
