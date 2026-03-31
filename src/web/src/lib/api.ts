const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";
const USE_MOCK_DATA = process.env.NEXT_PUBLIC_USE_MOCK_DATA === "true";

export interface GraphQLResponse<T> {
  data?: T;
  errors?: Array<{ message: string }>;
}

export class GraphQLError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'GraphQLError';
  }
}

// Lazy-load mock data to avoid bundling when not needed
async function getMockData() {
  const { mockUsers, mockVehicles, mockDepots, mockZones, mockRoutes } = await import("./mock-data");
  return { mockUsers, mockVehicles, mockDepots, mockZones, mockRoutes };
}

function extractOperationName(body: string): string | null {
  const match = body.match(/query\s+(\w+)/);
  return match ? match[1] : null;
}

export async function apiFetch<T>(
  path: string,
  init?: RequestInit & { token?: string }
): Promise<T> {
  // Mock data mode - return fake responses based on operation name
  if (USE_MOCK_DATA && path === "/api/graphql") {
    const body = init?.body as string;
    const operationName = extractOperationName(body);

    const mocks = await getMockData();

    // Return appropriate mock data based on operation
    if (operationName === "GetUsers" || operationName === "Users") {
      return { data: mocks.mockUsers } as T;
    }
    if (operationName === "GetVehicles" || operationName === "Vehicles") {
      return { data: mocks.mockVehicles } as T;
    }
    if (operationName === "GetDepots" || operationName === "Depots") {
      return { data: mocks.mockDepots } as T;
    }
    if (operationName === "GetZones" || operationName === "Zones") {
      return { data: mocks.mockZones } as T;
    }
    if (operationName === "GetRoutes" || operationName === "Routes") {
      return { data: mocks.mockRoutes } as T;
    }
    if (operationName === "GetUser" || operationName === "User") {
      return { data: { user: mocks.mockUsers.users.nodes[0] } } as T;
    }
    if (operationName === "GetVehicle" || operationName === "Vehicle") {
      return { data: { vehicle: mocks.mockVehicles.vehicles.nodes[0] } } as T;
    }
    if (operationName === "GetDepot" || operationName === "Depot") {
      return { data: { depot: mocks.mockDepots.depots.nodes[0] } } as T;
    }
    if (operationName === "GetZone" || operationName === "Zone") {
      return { data: { zone: mocks.mockZones.zones.nodes[0] } } as T;
    }
    if (operationName === "GetRoute" || operationName === "Route") {
      return { data: { route: mocks.mockRoutes.routes.nodes[0] } } as T;
    }
    // Default: return empty data for mutations
    return { data: {} } as T;
  }

  const { token, ...rest } = init ?? {};

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(init?.headers as Record<string, string>),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...rest,
    headers,
  });

  if (!res.ok) {
    throw new Error(`API error: ${res.status}`);
  }

  if (res.status === 204) {
    return undefined as T;
  }

  const json = await res.json() as GraphQLResponse<T>;

  // Check for GraphQL errors - these return HTTP 200 but contain error messages
  if (json.errors && json.errors.length > 0) {
    throw new GraphQLError(json.errors[0].message);
  }

  return json as T;
}
