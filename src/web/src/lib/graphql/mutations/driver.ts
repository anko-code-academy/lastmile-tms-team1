export const CREATE_DRIVER_MUTATION = /* GraphQL */ `
  mutation CreateDriver($input: CreateDriverCommandInput!) {
    createDriver(input: $input) {
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

export const UPDATE_DRIVER_MUTATION = /* GraphQL */ `
  mutation UpdateDriver($input: UpdateDriverCommandInput!) {
    updateDriver(input: $input) {
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

export const DELETE_DRIVER_MUTATION = /* GraphQL */ `
  mutation DeleteDriver($id: UUID!) {
    deleteDriver(id: $id)
  }
`;
