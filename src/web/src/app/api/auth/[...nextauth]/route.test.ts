import { describe, it, expect, vi, beforeEach } from "vitest";
import { NextRequest } from "next/server";
import { GET, POST } from "./route";

// Mock next-auth
vi.mock("next-auth", async () => {
  const actual = await vi.importActual("next-auth");
  return {
    ...actual,
  };
});

vi.mock("next-auth/providers", () => ({
  credentials: vi.fn(() => ({
    id: "credentials",
    name: "Credentials",
    type: "credentials",
    credentials: {
      email: { label: "Email", type: "email" },
      password: { label: "Password", type: "password" },
    },
    authorize: vi.fn(),
  })),
}));

describe("NextAuth Route", () => {
  let mockRequest: NextRequest;

  beforeEach(() => {
    vi.clearAllMocks();
    mockRequest = new NextRequest("http://localhost:3000/api/auth/session");
  });

  it("should export GET handler", () => {
    expect(typeof GET).toBe("function");
  });

  it("should export POST handler", () => {
    expect(typeof POST).toBe("function");
  });
});