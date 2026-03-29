export const CREATE_ZONE_MUTATION = /* GraphQL */ `
  mutation CreateZone($input: CreateZoneCommandInput!) {
    createZone(input: $input) {
      id
      name
      depotId
      isActive
      createdAt
    }
  }
`;

export const UPDATE_ZONE_MUTATION = /* GraphQL */ `
  mutation UpdateZone($input: UpdateZoneCommandInput!) {
    updateZone(input: $input) {
      id
      name
      depotId
      isActive
      createdAt
    }
  }
`;

export const DELETE_ZONE_MUTATION = /* GraphQL */ `
  mutation DeleteZone($id: UUID!) {
    deleteZone(id: $id)
  }
`;
