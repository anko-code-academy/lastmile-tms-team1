import { test as base, Page, expect } from "@playwright/test";
import path from "path";

const adminAuthFile = path.join(__dirname, ".auth", "admin.json");

export interface AuthenticatedPage {
  authenticatedPage: Page;
}

export const test = base.extend<AuthenticatedPage>({
  authenticatedPage: async ({ page }, use) => {
    await page.context().addInitScript(() => {
      // Mock window.confirm to always return true for delete operations
      window.confirm = () => true;
    });
    await use(page);
  },
});

export async function loginAsAdmin(page: Page) {
  const baseURL = process.env.E2E_BASE_URL ?? "http://localhost";
  const adminUsername = process.env.E2E_ADMIN_USERNAME ?? "admin";
  const adminPassword = process.env.E2E_ADMIN_PASSWORD ?? "Admin@123";

  await page.goto(`${baseURL}/login`);
  await page.getByPlaceholder("Enter your username").fill(adminUsername);
  await page.getByPlaceholder("Enter your password").fill(adminPassword);
  await page.getByRole("button", { name: "Sign In" }).click();
  await page.waitForURL(`${baseURL}/dashboard`);
}

export { adminAuthFile };
