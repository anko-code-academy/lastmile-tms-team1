import { Page } from "@playwright/test";

export class DepotFormPage {
  readonly page: Page;

  readonly nameInput = () => this.page.getByPlaceholder("Enter depot name");
  readonly streetInput = () => this.page.getByPlaceholder("123 Main St");
  readonly street2Input = () => this.page.getByPlaceholder("Apt 4B");
  readonly cityInput = () => this.page.getByPlaceholder("New York");
  readonly stateInput = () => this.page.getByPlaceholder("NY");
  readonly postalInput = () => this.page.getByPlaceholder("10001");
  readonly countryInput = () => this.page.getByPlaceholder("US");
  readonly activeCheckbox = () => this.page.locator("input[type='checkbox']");
  readonly submitButton = () => this.page.getByRole("button", { name: /Create Depot|Update Depot/ });
  readonly cancelButton = () => this.page.getByRole("button", { name: "Cancel" });

  constructor(page: Page) {
    this.page = page;
  }

  async gotoCreate() {
    await this.page.goto("/depots/new");
  }

  async gotoEdit(depotId: string) {
    await this.page.goto(`/depots/${depotId}`);
  }

  async fillName(name: string) {
    await this.nameInput().fill(name);
  }

  async fillAddress(address: {
    street?: string;
    street2?: string;
    city?: string;
    state?: string;
    postal?: string;
    country?: string;
  }) {
    if (address.street) await this.streetInput().fill(address.street);
    if (address.street2) await this.street2Input().fill(address.street2);
    if (address.city) await this.cityInput().fill(address.city);
    if (address.state) await this.stateInput().fill(address.state);
    if (address.postal) await this.postalInput().fill(address.postal);
    if (address.country) await this.countryInput().fill(address.country);
  }

  async toggleActive() {
    await this.activeCheckbox().click();
  }

  async submit() {
    await this.submitButton().click();
  }

  async cancel() {
    await this.cancelButton().click();
  }

  async expectValidationError(message: string) {
    await this.page.getByText(message).waitFor({ state: "visible" });
  }

  async expectFieldValidationError(field: string, message: string) {
    // Find the form field with the label and then find the error message
    const fieldText = field === "name" ? "Depot Name" : field === "city" ? "City" : field;
    await this.page.getByText(fieldText).first().waitFor({ state: "visible" });
    // Look for the error message near the field
    await this.page.locator(`text="${message}"`).first().waitFor({ state: "visible" });
  }
}
