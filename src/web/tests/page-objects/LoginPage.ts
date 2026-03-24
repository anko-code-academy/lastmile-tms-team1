import { Page, expect } from "@playwright/test";

export class LoginPage {
  readonly page: Page;

  readonly usernameInput = () => this.page.getByPlaceholder("Enter your username");
  readonly passwordInput = () => this.page.getByPlaceholder("Enter your password");
  readonly signInButton = () => this.page.getByRole("button", { name: "Sign In" });
  readonly errorMessage = () => this.page.locator("p.text-red-500");

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto("/login");
  }

  async login(username: string, password: string) {
    await this.usernameInput().fill(username);
    await this.passwordInput().fill(password);
    await this.signInButton().click();
  }

  async expectValidationError(field: "username" | "password") {
    const errorText = field === "username" ? "Username is required" : "Password is required";
    await this.page.getByText(errorText).waitFor({ state: "visible" });
  }

  async expectInvalidCredentialsError() {
    await this.errorMessage().waitFor({ state: "visible" });
    await expect(this.errorMessage()).toHaveText("Invalid username or password");
  }
}
