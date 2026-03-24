import { test, expect } from "@playwright/test";
import { ZoneFormPage } from "../../page-objects/ZoneFormPage";
import { ZoneListPage } from "../../page-objects/ZoneListPage";
import { DepotFormPage } from "../../page-objects/DepotFormPage";
import path from "path";

const adminAuthFile = path.join(__dirname, "..", "..", "fixtures", ".auth", "admin.json");

test.use({ storageState: adminAuthFile });

test.describe("Zone Delete", () => {
  test("should show confirmation dialog and delete zone", async ({ page }) => {
    const zoneFormPage = new ZoneFormPage(page);
    const depotFormPage = new DepotFormPage(page);
    const zoneListPage = new ZoneListPage(page);

    // First create a depot
    const depotName = `Delete Zone Test Depot ${Date.now()}`;
    await depotFormPage.gotoCreate();
    await depotFormPage.fillName(depotName);
    await depotFormPage.submit();
    await page.waitForURL("/depots");

    // Create a zone to delete
    const zoneName = `Delete Test Zone ${Date.now()}`;
    await zoneFormPage.gotoCreate();
    await zoneFormPage.fillName(zoneName);
    await zoneFormPage.selectDepot(depotName);
    await zoneFormPage.setGeoJsonDirectly(
      '{"type":"Polygon","coordinates":[[[-74.006,40.7128],[-74.006,40.7129],[-74.005,40.7129],[-74.005,40.7128],[-74.006,40.7128]]]}'
    );
    await zoneFormPage.submit();
    await page.waitForURL("/zones");

    // Delete the zone
    page.on("dialog", (dialog) => dialog.accept());
    await zoneListPage.getDeleteButtonForZone(zoneName).click();

    // Wait for the deletion to process
    await page.waitForTimeout(1000);

    // Zone should no longer be visible
    const row = zoneListPage.table().locator("tr", { has: page.getByText(zoneName) });
    await expect(row).not.toBeVisible();
  });
});
