export type UserStatus = 'ACTIVE' | 'INACTIVE' | 'SUSPENDED';

export interface RoleInfo {
  id: string;
  name: string | null;
}

export interface ZoneInfo {
  id: string;
  name: string;
}

export interface DepotInfo {
  id: string;
  name: string;
}

// User type returned by queries (uses navigation properties)
export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  status: UserStatus;
  roleId: string | null;
  role: RoleInfo | null;
  zoneId: string | null;
  zone: ZoneInfo | null;
  depotId: string | null;
  depot: DepotInfo | null;
  createdAt: string;
}

// UserDto type returned by mutations (uses scalar name fields)
export interface UserDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  status: UserStatus;
  roleId: string | null;
  roleName: string | null;
  zoneId: string | null;
  zoneName: string | null;
  depotId: string | null;
  depotName: string | null;
  createdAt: string;
}

export interface CreateUserInput {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  roleId: string;
  zoneId?: string;
  depotId?: string;
  password: string;
}

export interface UpdateUserInput {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  roleId?: string;
  zoneId?: string;
  depotId?: string;
}

export interface UsersFilter {
  where?: {
    search?: string;
    status?: UserStatus;
    roleId?: string;
  };
  order?: {
    field?: string;
    direction?: 'ASC' | 'DESC';
  };
}

export interface CreateUserMutationResponse {
  createUser: UserDto;
}

export interface UpdateUserMutationResponse {
  updateUser: UserDto;
}

export interface DeactivateUserMutationResponse {
  deactivateUser: UserDto;
}

export interface ActivateUserMutationResponse {
  activateUser: UserDto;
}

export interface ResetPasswordMutationResponse {
  resetPassword: boolean;
}

export interface UsersQueryResponse {
  users: {
    nodes: User[];
  };
}

// Flattened version returned by fetchUsers
export interface FlatUsersQueryResponse {
  users: User[];
}

export interface PageInfo {
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  startCursor: string | null;
  endCursor: string | null;
}

export interface PaginatedUsersQueryResponse {
  users: {
    nodes: User[];
    pageInfo: PageInfo;
    totalCount: number;
  };
}

export interface UserByIdQueryResponse {
  user: User | null;
}
