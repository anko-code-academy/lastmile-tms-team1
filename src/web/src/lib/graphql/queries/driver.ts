export const GET_DRIVER_QUERY = /* GraphQL */ `
  query GetDriver($id: UUID!) {
    driver(id: $id) {
      id
      firstName
      lastName
      phone
      email
      licenseNumber
      licenseExpiryDate
      photo
      zoneId
      depotId
      userId
      isActive
      createdAt
      lastModifiedAt
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
`;

export const GET_DRIVERS_QUERY = /* GraphQL */ `
  query GetDrivers {
    drivers {
      nodes {
        id
        firstName
        lastName
        phone
        email
        isActive
        createdAt
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
`;
