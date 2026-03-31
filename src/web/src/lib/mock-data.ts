// Mock data for development when NEXT_PUBLIC_USE_MOCK_DATA=true

export const mockUsers = {
  users: {
    nodes: [
      {
        id: "1",
        firstName: "John",
        lastName: "Doe",
        email: "john.doe@example.com",
        phoneNumber: "+1234567890",
        status: "ACTIVE",
        roleName: "Admin",
        roleId: "role-1",
        zoneId: null,
        zoneName: null,
        depotId: null,
        depotName: null,
        createdAt: "2024-01-15T10:00:00Z",
      },
      {
        id: "2",
        firstName: "Jane",
        lastName: "Smith",
        email: "jane.smith@example.com",
        phoneNumber: "+1234567891",
        status: "ACTIVE",
        roleName: "Driver",
        roleId: "role-2",
        zoneId: "zone-1",
        zoneName: "Downtown",
        depotId: "depot-1",
        depotName: "Central Depot",
        createdAt: "2024-01-20T14:30:00Z",
      },
    ],
    pageInfo: {
      hasNextPage: false,
      hasPreviousPage: false,
      startCursor: null,
      endCursor: null,
    },
    totalCount: 2,
  },
};

export const mockVehicles = {
  vehicles: {
    nodes: [
      {
        id: "v1",
        registrationPlate: "ABC-123",
        type: "VAN",
        parcelCapacity: 100,
        weightCapacityKg: 500,
        status: "ACTIVE",
        depotId: "depot-1",
        depotName: "Central Depot",
        createdAt: "2024-01-10T08:00:00Z",
      },
      {
        id: "v2",
        registrationPlate: "XYZ-789",
        type: "CAR",
        parcelCapacity: 20,
        weightCapacityKg: 100,
        status: "ACTIVE",
        depotId: "depot-1",
        depotName: "Central Depot",
        createdAt: "2024-01-12T09:00:00Z",
      },
    ],
    pageInfo: {
      hasNextPage: false,
      hasPreviousPage: false,
      startCursor: null,
      endCursor: null,
    },
    totalCount: 2,
  },
};

export const mockDepots = {
  depots: {
    nodes: [
      {
        id: "depot-1",
        name: "Central Depot",
        address: {
          street1: "123 Main St",
          city: "New York",
          state: "NY",
          postalCode: "10001",
          countryCode: "US",
        },
        isActive: true,
        createdAt: "2024-01-01T00:00:00Z",
      },
    ],
  },
};

export const mockZones = {
  zones: {
    nodes: [
      {
        id: "zone-1",
        name: "Downtown",
        boundaryGeometry: null,
        depotId: "depot-1",
        depot: { name: "Central Depot" },
        isActive: true,
        createdAt: "2024-01-05T00:00:00Z",
      },
    ],
  },
};

export const mockRoutes = {
  routes: {
    nodes: [
      {
        id: "route-1",
        name: "Morning Route",
        status: "COMPLETED",
        plannedStartTime: "2024-01-25T08:00:00Z",
        totalDistanceKm: 45.5,
        totalParcelCount: 25,
        vehicleId: "v1",
        vehicleRegistrationPlate: "ABC-123",
        assignedDriverId: "2",
        assignedDriverName: "Jane Smith",
        depotId: "depot-1",
        depotName: "Central Depot",
        createdAt: "2024-01-24T00:00:00Z",
      },
    ],
    pageInfo: {
      hasNextPage: false,
      hasPreviousPage: false,
      startCursor: null,
      endCursor: null,
    },
    totalCount: 1,
  },
};
