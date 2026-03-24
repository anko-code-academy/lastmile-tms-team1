import { test, expect } from "@playwright/test";
import { ZoneFormPage } from "../../page-objects/ZoneFormPage";
import { ZoneListPage } from "../../page-objects/ZoneListPage";
import { DepotFormPage } from "../../page-objects/DepotFormPage";
import path from "path";

const adminAuthFile = path.join(__dirname, "..", "..", "fixtures", ".auth", "admin.json");

test.use({ storageState: adminAuthFile });

test.describe("Zone Form", () => {
  test("should show validation error if depot not selected", async ({ page }) => {
    const zoneFormPage = new ZoneFormPage(page);
    await zoneFormPage.gotoCreate();

    await zoneFormPage.fillName("Test Zone");
    await zoneFormPage.submit();

    await zoneFormPage.expectDepotValidationError();
  });

  test("should create zone when depot is selected", async ({ page }) => {
    const zoneFormPage = new ZoneFormPage(page);
    const depotFormPage = new DepotFormPage(page);
    const zoneListPage = new ZoneListPage(page);

    // First create a depot to assign the zone to
    const depotName = `Zone Test Depot ${Date.now()}`;
    await depotFormPage.gotoCreate();
    await depotFormPage.fillName(depotName);
    await depotFormPage.submit();
    await page.waitForURL("/depots");

    // Now create a zone
    const zoneName = `Test Zone ${Date.now()}`;
    await zoneFormPage.gotoCreate();
    await zoneFormPage.fillName(zoneName);
    await zoneFormPage.selectDepot(depotName);

    // Mock the GeoJSON since we can't interact with Mapbox in tests
    await zoneFormPage.setGeoJsonDirectly(
      '{"type":"Polygon","coordinates":[[[-74.006,40.7128],[-74.006,40.7129],[-74.005,40.7129],[-74.005,40.7128],[-74.006,40.7128]]]}'
    );

    await zoneFormPage.submit();

    await page.waitForURL("/zones");
    await zoneListPage.expectZoneInTable(zoneName, true, depotName);
  });

  test("should navigate back on cancel", async ({ page }) => {
    const zoneFormPage = new ZoneFormPage(page);
    await zoneFormPage.gotoCreate();

    await zoneFormPage.fillName("Test Zone");
    await zoneFormPage.cancel();

    await page.waitForURL("/zones");
  });
});
