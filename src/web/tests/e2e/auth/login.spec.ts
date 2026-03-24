import { test, expect } from "@playwright/test";
import path from "path";

const adminAuthFile = path.join(__dirname, "..", "..", "fixtures", ".auth", "admin.json");

test.describe("Authentication", () => {
  test("should display login form with username and password fields", async ({ page }) => {
    await page.goto("/login");

    await expect(page.getByPlaceholder("Enter your username")).toBeVisible();
    await expect(page.getByPlaceholder("Enter your password")).toBeVisible();
    await expect(page.getByRole("button", { name: "Sign In" })).toBeVisible();
  });

  test("should show validation errors for empty fields", async ({ page }) => {
    await page.goto("/login");

    // Try submitting with empty fields
    await page.getByRole("button", { name: "Sign In" }).click();

    await expect(page.getByText("Username is required")).toBeVisible();
    await expect(page.getByText("Password is required")).toBeVisible();
  });

  test("should redirect to dashboard on successful login", async ({ page }) => {
    const adminUsername = process.env.E2E_ADMIN_USERNAME ?? "admin";
    const adminPassword = process.env.E2E_ADMIN_PASSWORD ?? "Admin@123";

    await page.goto("/login");

    await page.getByPlaceholder("Enter your username").fill(adminUsername);
    await page.getByPlaceholder("Enter your password").fill(adminPassword);
    await page.getByRole("button", { name: "Sign In" }).click();

    await page.waitForURL("/dashboard");
    await expect(page).toHaveURL("/dashboard");
  });

  test("should show error on invalid credentials", async ({ page }) => {
    await page.goto("/login");

    await page.getByPlaceholder("Enter your username").fill("wronguser");
    await page.getByPlaceholder("Enter your password").fill("wrongpassword");
    await page.getByRole("button", { name: "Sign In" }).click();

    await expect(page.locator("p.text-red-500")).toBeVisible();
    await expect(page.locator("p.text-red-500")).toHaveText("Invalid username or password");
  });
});
