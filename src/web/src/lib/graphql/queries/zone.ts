export const GET_ZONE_QUERY = /* GraphQL */ `
  query GetZone($id: UUID!) {
    zone(id: $id) {
      id
      name
      geoJson
      depotId
      depotName
      isActive
      createdAt
      lastModifiedAt
    }
  }
`;

export const GET_ZONES_QUERY = /* GraphQL */ `
  query GetZones {
    zones {
      id
      name
      depotId
      depotName
      isActive
      createdAt
    }
  }
`;
