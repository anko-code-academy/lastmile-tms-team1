import { apiFetch } from "@/lib/api";
import type { User, UserDto, UsersFilter, PageInfo } from "@/types/user";

const GET_USERS_QUERY = `
  query GetUsers($first: Int, $after: String) {
    users(first: $first, after: $after) {
      nodes {
        id
        firstName
        lastName
        email
        phoneNumber
        status
        roleId
        role {
          id
          name
        }
        zoneId
        zone {
          id
          name
        }
        depotId
        depot {
          id
          name
        }
        createdAt
      }
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor
        endCursor
      }
      totalCount
    }
  }
`;

const GET_USER_BY_ID_QUERY = `
  query GetUserById($id: UUID!) {
    userById(id: $id) {
      id
      firstName
      lastName
      email
      phoneNumber
      status
      roleId
      role {
        id
        name
      }
      zoneId
      zone {
        id
        name
      }
      depotId
      depot {
        id
        name
      }
      createdAt
    }
  }
`;

const GET_USER_MANAGEMENT_LOOKUPS_QUERY = `
  query GetUserManagementLookups {
    userManagementLookups {
      roles {
        id
        name
        description
      }
      depots {
        id
        name
      }
      zones {
        id
        name
        depotId
      }
    }
  }
`;

const CREATE_USER_MUTATION = `
  mutation CreateUser(
    $firstName: String!
    $lastName: String!
    $email: String!
    $phone: String
    $roleId: UUID!
    $zoneId: UUID
    $depotId: UUID
    $password: String!
  ) {
    createUser(
      firstName: $firstName
      lastName: $lastName
      email: $email
      phone: $phone
      roleId: $roleId
      zoneId: $zoneId
      depotId: $depotId
      password: $password
    ) {
      id
      firstName
      lastName
      email
      phoneNumber
      status
      roleId
      roleName
      zoneId
      zoneName
      depotId
      depotName
      createdAt
    }
  }
`;

const UPDATE_USER_MUTATION = `
  mutation UpdateUser(
    $userId: UUID!
    $firstName: String!
    $lastName: String!
    $email: String!
    $phone: String
    $roleId: UUID
    $zoneId: UUID
    $depotId: UUID
  ) {
    updateUser(
      userId: $userId
      firstName: $firstName
      lastName: $lastName
      email: $email
      phone: $phone
      roleId: $roleId
      zoneId: $zoneId
      depotId: $depotId
    ) {
      id
      firstName
      lastName
      email
      phoneNumber
      status
      roleId
      roleName
      zoneId
      zoneName
      depotId
      depotName
      createdAt
    }
  }
`;

const DEACTIVATE_USER_MUTATION = `
  mutation DeactivateUser($userId: UUID!) {
    deactivateUser(userId: $userId) {
      id
      firstName
      lastName
      email
      phoneNumber
      status
      roleId
      roleName
      zoneId
      zoneName
      depotId
      depotName
      createdAt
    }
  }
`;

const ACTIVATE_USER_MUTATION = `
  mutation ActivateUser($userId: UUID!) {
    activateUser(userId: $userId) {
      id
      firstName
      lastName
      email
      phoneNumber
      status
      roleId
      roleName
      zoneId
      zoneName
      depotId
      depotName
      createdAt
    }
  }
`;

const RESET_PASSWORD_MUTATION = `
  mutation ResetPassword($email: String!) {
    resetPassword(email: $email)
  }
`;

interface UsersResponse {
  users: {
    nodes: User[];
    pageInfo: PageInfo;
    totalCount: number;
  };
}

interface UserByIdResponse {
  userById: UserDto | null;
}

interface UserManagementLookupsResponse {
  userManagementLookups: {
    roles: Array<{ id: string; name: string; description: string | null }>;
    depots: Array<{ id: string; name: string }>;
    zones: Array<{ id: string; name: string; depotId: string }>;
  };
}

interface UserMutationResponse {
  createUser: UserDto;
  updateUser: UserDto;
  deactivateUser: UserDto;
  activateUser: UserDto;
}

export async function fetchUsers(
  token: string,
  filters?: UsersFilter,
  pageParams?: { first?: number; after?: string }
): Promise<{ users: User[]; pageInfo: PageInfo; totalCount: number }> {
  const where: Record<string, unknown> = {};

  if (filters?.where?.search) {
    where.or = [
      { firstName: { contains: filters.where.search } },
      { lastName: { contains: filters.where.search } },
      { email: { contains: filters.where.search } },
    ];
  }

  if (filters?.where?.status) {
    where.status = filters.where.status;
  }

  if (filters?.where?.roleId) {
    where.roleId = filters.where.roleId;
  }

  const response = await apiFetch<{ data: UsersResponse }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: GET_USERS_QUERY,
      variables: {
        where: Object.keys(where).length > 0 ? where : undefined,
        first: pageParams?.first ?? 10,
        after: pageParams?.after,
      },
    }),
  });

  return {
    users: response.data.users.nodes,
    pageInfo: response.data.users.pageInfo,
    totalCount: response.data.users.totalCount,
  };
}

export async function fetchUserById(
  token: string,
  id: string
): Promise<UserDto | null> {
  const response = await apiFetch<{ data: UserByIdResponse }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: GET_USER_BY_ID_QUERY,
      variables: { id },
    }),
  });
  return response.data.userById;
}

export async function fetchUserManagementLookups(
  token: string
): Promise<UserManagementLookupsResponse["userManagementLookups"]> {
  const response = await apiFetch<{
    data: UserManagementLookupsResponse;
  }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: GET_USER_MANAGEMENT_LOOKUPS_QUERY,
    }),
  });
  return response.data.userManagementLookups;
}

export async function createUser(
  token: string,
  input: {
    firstName: string;
    lastName: string;
    email: string;
    phone?: string;
    roleId: string;
    zoneId?: string;
    depotId?: string;
    password: string;
  }
): Promise<UserDto> {
  const response = await apiFetch<{ data: { createUser: UserDto } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: CREATE_USER_MUTATION,
        variables: input,
      }),
    }
  );
  return response.data.createUser;
}

export async function updateUser(
  token: string,
  userId: string,
  input: {
    firstName: string;
    lastName: string;
    email: string;
    phone?: string;
    roleId?: string;
    zoneId?: string;
    depotId?: string;
  }
): Promise<UserDto> {
  const response = await apiFetch<{ data: { updateUser: UserDto } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: UPDATE_USER_MUTATION,
        variables: { userId, ...input },
      }),
    }
  );
  return response.data.updateUser;
}

export async function deactivateUser(
  token: string,
  userId: string
): Promise<UserDto> {
  const response = await apiFetch<{ data: { deactivateUser: UserDto } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: DEACTIVATE_USER_MUTATION,
        variables: { userId },
      }),
    }
  );
  return response.data.deactivateUser;
}

export async function activateUser(
  token: string,
  userId: string
): Promise<UserDto> {
  const response = await apiFetch<{ data: { activateUser: UserDto } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: ACTIVATE_USER_MUTATION,
        variables: { userId },
      }),
    }
  );
  return response.data.activateUser;
}

export async function resetPassword(email: string): Promise<boolean> {
  const response = await apiFetch<{ data: { resetPassword: boolean } }>(
    "/api/graphql",
    {
      method: "POST",
      body: JSON.stringify({
        query: RESET_PASSWORD_MUTATION,
        variables: { email },
      }),
    }
  );
  return response.data.resetPassword;
}
