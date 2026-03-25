import { apiFetch } from '@/lib/api';
import type {
	UsersQueryResponse,
	FlatUsersQueryResponse,
	UserByIdQueryResponse,
	UsersFilter,
	UserDto,
} from '@/types/user';

const GET_USERS_QUERY = `
	query GetUsers {
		users {
			nodes {
				id
				firstName
				lastName
				email
				phoneNumber
				status
				roleName
				roleId
				zoneId
				depotId
				createdAt
			}
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
			roleName
			roleId
			zoneId
			depotId
			createdAt
		}
	}
`;

export async function fetchUsers(
	token: string,
	filters?: UsersFilter
): Promise<FlatUsersQueryResponse> {
	// Build filter variables for HotChocolate
	const where: Record<string, unknown> = {};

	if (filters?.where?.search) {
		// HotChocolate string filters use contains, startsWith, endsWith
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

	const orderBy =
		filters?.order?.field && filters?.order?.direction
			? { [filters.order.field]: filters.order.direction }
			: undefined;

	const response = await apiFetch<{ data: { users: { nodes: UserDto[] } } }>('/api/graphql', {
		method: 'POST',
		token,
		body: JSON.stringify({
			query: GET_USERS_QUERY,
			variables: {
				where: Object.keys(where).length > 0 ? where : undefined,
				orderBy,
			},
		}),
	});

	// Extract nodes from cursor connection, unwrapping GraphQL data wrapper
	return { users: response.data.users.nodes };
}

export async function fetchUserById(
	token: string,
	id: string
): Promise<UserByIdQueryResponse> {
	return apiFetch<UserByIdQueryResponse>('/api/graphql', {
		method: 'POST',
		token,
		body: JSON.stringify({
			query: GET_USER_BY_ID_QUERY,
			variables: { id },
		}),
	});
}

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

export async function fetchUserManagementLookups(
	token: string
): Promise<{ userManagementLookups: {
	roles: Array<{ id: string; name: string; description: string | null }>;
	depots: Array<{ id: string; name: string }>;
	zones: Array<{ id: string; name: string; depotId: string }>;
}}> {
	const response = await apiFetch<{ data: { userManagementLookups: {
		roles: Array<{ id: string; name: string; description: string | null }>;
		depots: Array<{ id: string; name: string }>;
		zones: Array<{ id: string; name: string; depotId: string }>;
	}}}>('/api/graphql', {
		method: 'POST',
		token,
		body: JSON.stringify({
			query: GET_USER_MANAGEMENT_LOOKUPS_QUERY,
		}),
	});

	return response.data;
}
