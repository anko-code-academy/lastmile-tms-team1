export const GET_ZONE_QUERY = /* GraphQL */ `
  query GetZone($id: UUID!) {
    zone(id: $id) {
      id
      name
      boundaryGeometry
      depotId
      depot {
        name
      }
      isActive
      createdAt
      lastModifiedAt
    }
  }
`;

export const GET_ZONES_QUERY = /* GraphQL */ `
  query GetZones {
    zones {
      nodes {
        id
        name
        boundaryGeometry
        depotId
        depot {
          name
        }
        isActive
        createdAt
      }
    }
  }
`;
