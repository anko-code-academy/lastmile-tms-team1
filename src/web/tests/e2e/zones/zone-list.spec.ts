import { test, expect } from "@playwright/test";
import { ZoneListPage } from "../../page-objects/ZoneListPage";
import path from "path";

const adminAuthFile = path.join(__dirname, "..", "..", "fixtures", ".auth", "admin.json");

test.use({ storageState: adminAuthFile });

test.describe("Zone List", () => {
  test("should display loading state", async ({ page }) => {
    const zoneListPage = new ZoneListPage(page);
    await zoneListPage.goto();
    await zoneListPage.expectLoadingState();
  });

  test("should navigate to create zone when clicking Add Zone", async ({ page }) => {
    const zoneListPage = new ZoneListPage(page);
    await zoneListPage.goto();
    await page.waitForLoadState("networkidle");

    await zoneListPage.clickAddZone();
    await expect(page).toHaveURL("/zones/new");
  });
});
