import { chromium, FullConfig } from "@playwright/test";
import path from "path";

async function globalSetup(config: FullConfig) {
  const baseURL = process.env.E2E_BASE_URL ?? "http://localhost";
  const adminUsername = process.env.E2E_ADMIN_USERNAME ?? "admin";
  const adminPassword = process.env.E2E_ADMIN_PASSWORD ?? "Admin@123";
  const storageStatePath = path.join(__dirname, "fixtures", ".auth", "admin.json");

  const browser = await chromium.launch();
  const context = await browser.newContext();
  const page = await context.newPage();

  await page.goto(`${baseURL}/login`);
  await page.waitForLoadState("networkidle");

  await page.getByPlaceholder("Enter your username").fill(adminUsername);
  await page.getByPlaceholder("Enter your password").fill(adminPassword);
  await page.getByRole("button", { name: "Sign In" }).click();

  await page.waitForURL(`${baseURL}/dashboard`, { timeout: 30000 });
  await page.waitForLoadState("networkidle");

  await context.storageState({ path: storageStatePath });
  await browser.close();
}

export default globalSetup;
