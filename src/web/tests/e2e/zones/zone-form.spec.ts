import { test } from "@playwright/test";
import { ZoneFormPage } from "../../page-objects/ZoneFormPage";
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

  test("should navigate back on cancel", async ({ page }) => {
    const zoneFormPage = new ZoneFormPage(page);
    await zoneFormPage.gotoCreate();

    await zoneFormPage.fillName("Test Zone");
    await zoneFormPage.cancel();

    await page.waitForURL("/zones");
  });
});
