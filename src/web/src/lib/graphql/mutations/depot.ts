export const CREATE_DEPOT_MUTATION = /* GraphQL */ `
  mutation CreateDepot($input: CreateDepotCommandInput!) {
    createDepot(input: $input) {
      id
      name
      isActive
      createdAt
    }
  }
`;

export const UPDATE_DEPOT_MUTATION = /* GraphQL */ `
  mutation UpdateDepot($input: UpdateDepotCommandInput!) {
    updateDepot(input: $input) {
      id
      name
      isActive
      createdAt
    }
  }
`;

export const DELETE_DEPOT_MUTATION = /* GraphQL */ `
  mutation DeleteDepot($id: UUID!) {
    deleteDepot(id: $id)
  }
`;
