import { test, expect } from "@playwright/test";
import { DepotFormPage } from "../../page-objects/DepotFormPage";
import { DepotListPage } from "../../page-objects/DepotListPage";
import path from "path";

const adminAuthFile = path.join(__dirname, "..", "..", "fixtures", ".auth", "admin.json");

test.use({ storageState: adminAuthFile });

test.describe("Depot Delete", () => {
  test("should show confirmation dialog and delete depot", async ({ page }) => {
    const depotFormPage = new DepotFormPage(page);
    const depotListPage = new DepotListPage(page);

    // First create a depot to delete
    const depotName = `Delete Test Depot ${Date.now()}`;
    await depotFormPage.gotoCreate();
    await depotFormPage.fillName(depotName);
    await depotFormPage.submit();
    await page.waitForURL("/depots");

    // Now delete it
    page.on("dialog", (dialog) => dialog.accept());
    await depotListPage.getDeleteButtonForDepot(depotName).click();

    // Wait for the deletion to process
    await page.waitForTimeout(1000);

    // Depot should no longer be visible
    const row = depotListPage.table().locator("tr", { has: page.getByText(depotName) });
    await expect(row).not.toBeVisible();
  });
});
