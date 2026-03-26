export const GET_DEPOT_QUERY = /* GraphQL */ `
  query GetDepot($id: UUID!) {
    depot(id: $id) {
      id
      name
      address {
        street1
        street2
        city
        state
        postalCode
        countryCode
        isResidential
        contactName
        companyName
        phone
        email
      }
      shiftSchedules {
        dayOfWeek
        openTime
        closeTime
      }
      isActive
      zones {
        id
      }
    }
  }
`;

export const GET_DEPOTS_QUERY = /* GraphQL */ `
  query GetDepots {
    depots {
      nodes {
        id
        name
        address {
          street1
          street2
          city
          state
          postalCode
          countryCode
          isResidential
          contactName
          companyName
          phone
          email
        }
        isActive
        createdAt
      }
    }
  }
`;
