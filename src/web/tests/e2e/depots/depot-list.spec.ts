import { test, expect } from "@playwright/test";
import { DepotListPage } from "../../page-objects/DepotListPage";
import path from "path";

const adminAuthFile = path.join(__dirname, "..", "..", "fixtures", ".auth", "admin.json");

test.use({ storageState: adminAuthFile });

test.describe("Depot List", () => {
  test("should display loading state", async ({ page }) => {
    const depotListPage = new DepotListPage(page);
    await depotListPage.goto();
    await depotListPage.expectLoadingState();
  });

  test("should display depot table with correct columns", async ({ page }) => {
    const depotListPage = new DepotListPage(page);
    await depotListPage.goto();
    await page.waitForSelector("table", { state: "visible" });

    await depotListPage.expectTableHeaders(["Name", "Address", "Status", "Created", "Actions"]);
  });

  test("should navigate to create depot when clicking Add Depot", async ({ page }) => {
    const depotListPage = new DepotListPage(page);
    await depotListPage.goto();
    await page.waitForLoadState("networkidle");

    await depotListPage.clickAddDepot();
    await expect(page).toHaveURL("/depots/new");
  });

  test("should navigate to edit depot when clicking edit button", async ({ page }) => {
    const depotListPage = new DepotListPage(page);
    await depotListPage.goto();
    await page.waitForSelector("table", { state: "visible" });

    // This test assumes there's at least one depot - may need adjustment for empty state
    const editButton = page.getByRole("button", { name: /pencil/i }).first();
    if (await editButton.isVisible()) {
      await editButton.click();
      await page.waitForURL(/\/depots\/[a-z0-9-]+$/);
    }
  });
});
