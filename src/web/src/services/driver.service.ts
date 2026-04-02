import { apiFetch } from "@/lib/api";
import type { DriverSummaryDto, DriverDto } from "@/lib/graphql/types";

const GET_DRIVERS_QUERY = `
  query GetDrivers {
    drivers {
      nodes {
        id
        licenseNumber
        licenseExpiryDate
        photo
        userId
        createdAt
        user {
          id
          firstName
          lastName
          email
          phoneNumber
          isActive
          zone {
            id
            name
          }
          depot {
            id
            name
          }
        }
      }
    }
  }
`;

const GET_DRIVER_QUERY = `
  query GetDriver($id: UUID!) {
    driver(id: $id) {
      id
      licenseNumber
      licenseExpiryDate
      photo
      userId
      createdAt
      lastModifiedAt
      user {
        id
        firstName
        lastName
        email
        phoneNumber
        isActive
        zone {
          id
          name
        }
        depot {
          id
          name
        }
      }
      shiftSchedules {
        dayOfWeek
        openTime
        closeTime
      }
      daysOff {
        date
      }
    }
  }
`;

const CREATE_DRIVER_MUTATION = `
  mutation CreateDriver($input: CreateDriverCommandInput!) {
    createDriver(input: $input) {
      id
      licenseNumber
      licenseExpiryDate
      photo
      userId
      createdAt
    }
  }
`;

const UPDATE_DRIVER_MUTATION = `
  mutation UpdateDriver($input: UpdateDriverCommandInput!) {
    updateDriver(input: $input) {
      id
      licenseNumber
      licenseExpiryDate
      photo
      userId
      createdAt
    }
  }
`;

const DELETE_DRIVER_MUTATION = `
  mutation DeleteDriver($id: UUID!) {
    deleteDriver(id: $id)
  }
`;

interface DriversResponse {
  drivers: {
    nodes: DriverSummaryDto[];
  };
}

interface DriverResponse {
  driver: DriverDto | null;
}

export async function fetchDrivers(token: string): Promise<DriverSummaryDto[]> {
  const response = await apiFetch<{ data: DriversResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({ query: GET_DRIVERS_QUERY }),
  });
  return response.data.drivers.nodes;
}

export async function fetchDriver(
  token: string,
  id: string
): Promise<DriverDto | null> {
  const response = await apiFetch<{ data: DriverResponse }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({ query: GET_DRIVER_QUERY, variables: { id } }),
  });
  return response.data.driver;
}

export async function createDriver(
  token: string,
  input: {
    email: string;
    licenseNumber: string;
    licenseExpiryDate: string;
    photo?: string;
    shiftSchedules?: Array<{
      dayOfWeek: string;
      openTime: string | null;
      closeTime: string | null;
    }>;
    daysOff?: Array<{ date: string }>;
  }
): Promise<DriverSummaryDto> {
  const response = await apiFetch<{
    data: { createDriver: DriverSummaryDto };
  }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({ query: CREATE_DRIVER_MUTATION, variables: { input } }),
  });
  return response.data.createDriver;
}

export async function updateDriver(
  token: string,
  input: {
    id: string;
    licenseNumber: string;
    licenseExpiryDate: string;
    photo?: string;
    shiftSchedules?: Array<{
      dayOfWeek: string;
      openTime: string | null;
      closeTime: string | null;
    }>;
    daysOff?: Array<{ date: string }>;
  }
): Promise<DriverSummaryDto> {
  const response = await apiFetch<{
    data: { updateDriver: DriverSummaryDto };
  }>("/api/graphql", {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body: JSON.stringify({ query: UPDATE_DRIVER_MUTATION, variables: { input } }),
  });
  return response.data.updateDriver;
}

export async function deleteDriver(token: string, id: string): Promise<boolean> {
  const response = await apiFetch<{ data: { deleteDriver: boolean } }>(
    "/api/graphql",
    {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: JSON.stringify({ query: DELETE_DRIVER_MUTATION, variables: { id } }),
    }
  );
  return response.data.deleteDriver;
}

export interface CreateDriverInput {
  email: string;
  licenseNumber: string;
  licenseExpiryDate: string;
  photo?: string;
  shiftSchedules?: Array<{
    dayOfWeek: string;
    openTime: string | null;
    closeTime: string | null;
  }>;
  daysOff?: Array<{ date: string }>;
}

export interface UpdateDriverInput {
  id: string;
  licenseNumber: string;
  licenseExpiryDate: string;
  photo?: string;
  shiftSchedules?: Array<{
    dayOfWeek: string;
    openTime: string | null;
    closeTime: string | null;
  }>;
  daysOff?: Array<{ date: string }>;
}
