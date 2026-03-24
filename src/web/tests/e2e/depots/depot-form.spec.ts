import { test, expect } from "@playwright/test";
import { DepotFormPage } from "../../page-objects/DepotFormPage";
import { DepotListPage } from "../../page-objects/DepotListPage";
import path from "path";

const adminAuthFile = path.join(__dirname, "..", "..", "fixtures", ".auth", "admin.json");

test.use({ storageState: adminAuthFile });

test.describe("Depot Form", () => {
  test("should create depot with minimal data (name only)", async ({ page }) => {
    const depotFormPage = new DepotFormPage(page);
    const depotListPage = new DepotListPage(page);

    const depotName = `Test Depot ${Date.now()}`;

    await depotFormPage.gotoCreate();
    await depotFormPage.fillName(depotName);
    await depotFormPage.submit();

    await page.waitForURL("/depots");
    await depotListPage.expectDepotInTable(depotName);
  });

  test("should create depot with full address data", async ({ page }) => {
    const depotFormPage = new DepotFormPage(page);
    const depotListPage = new DepotListPage(page);

    const depotName = `Full Address Depot ${Date.now()}`;

    await depotFormPage.gotoCreate();
    await depotFormPage.fillName(depotName);
    await depotFormPage.fillAddress({
      street: "123 Main St",
      city: "New York",
      state: "NY",
      postal: "10001",
      country: "US",
    });
    await depotFormPage.submit();

    await page.waitForURL("/depots");
    await depotListPage.expectDepotInTable(depotName);
  });

  test("should toggle active status", async ({ page }) => {
    const depotFormPage = new DepotFormPage(page);
    const depotListPage = new DepotListPage(page);

    const depotName = `Toggle Test Depot ${Date.now()}`;

    await depotFormPage.gotoCreate();
    await depotFormPage.fillName(depotName);
    // Default is active, toggle off
    await depotFormPage.toggleActive();
    await depotFormPage.submit();

    await page.waitForURL("/depots");
    await depotListPage.expectDepotInTable(depotName, false);
  });

  test("should show validation errors for empty required fields", async ({ page }) => {
    const depotFormPage = new DepotFormPage(page);
    await depotFormPage.gotoCreate();

    // Submit without filling any fields
    await depotFormPage.submit();

    await depotFormPage.expectFieldValidationError("name", "Depot name is required");
  });

  test("should navigate back on cancel", async ({ page }) => {
    const depotFormPage = new DepotFormPage(page);
    await depotFormPage.gotoCreate();

    await depotFormPage.fillName("Test Depot");
    await depotFormPage.cancel();

    await page.waitForURL("/depots");
  });
});
