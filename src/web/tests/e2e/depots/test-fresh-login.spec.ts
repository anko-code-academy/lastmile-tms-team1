import { test, expect } from "@playwright/test";

test.describe("Depot Form Fresh Login", () => {
  test("should create depot with minimal data (fresh login)", async ({ page }) => {
    const adminUsername = "admin";
    const adminPassword = "Admin@123";

    // Login first
    await page.goto("/login");
    await page.getByPlaceholder("Enter your username").fill(adminUsername);
    await page.getByPlaceholder("Enter your password").fill(adminPassword);
    await page.getByRole("button", { name: "Sign In" }).click();
    await page.waitForURL("/dashboard");

    // Now go to create depot
    await page.goto("/depots/new");
    await page.waitForLoadState("networkidle");

    // Fill name
    const depotName = `Test Depot ${Date.now()}`;
    await page.getByPlaceholder("Enter depot name").fill(depotName);

    // Submit
    await page.getByRole("button", { name: /Create Depot/ }).click();

    // Wait for navigation
    await page.waitForURL("/depots", { timeout: 15000 });
    console.log("SUCCESS: Navigated to /depots");
  });
});
