export type ColumnKey =
  | "trackingNumber"
  | "status"
  | "serviceType"
  | "description"
  | "parcelType"
  | "weight"
  | "length"
  | "width"
  | "height"
  | "dimensionUnit"
  | "declaredValue"
  | "currency"
  | "estimatedDeliveryDate"
  | "actualDeliveryDate"
  | "deliveryAttempts"
  | "createdAt"
  | "recipientContactName"
  | "recipientPhone"
  | "recipientEmail"
  | "recipientStreet"
  | "recipientCity"
  | "recipientState"
  | "recipientPostalCode"
  | "recipientCountryCode"
  | "shipperContactName"
  | "shipperCity"
  | "zone"
  | "depot"
  | "link";

export interface ColumnDef {
  key: ColumnKey;
  label: string;
  group: "parcel" | "dimensions" | "financials" | "dates" | "recipient" | "shipper" | "zone" | "depot" | "actions";
  sortable: boolean;
  alwaysOn?: boolean;
  graphqlFields: string[];
}

export const COLUMN_REGISTRY: ColumnDef[] = [
  // Parcel
  {
    key: "trackingNumber",
    label: "Tracking #",
    group: "parcel",
    sortable: true,
    alwaysOn: true,
    graphqlFields: ["trackingNumber"],
  },
  {
    key: "status",
    label: "Status",
    group: "parcel",
    sortable: true,
    graphqlFields: ["status"],
  },
  {
    key: "serviceType",
    label: "Service Type",
    group: "parcel",
    sortable: true,
    graphqlFields: ["serviceType"],
  },
  {
    key: "description",
    label: "Description",
    group: "parcel",
    sortable: true,
    graphqlFields: ["description"],
  },
  {
    key: "parcelType",
    label: "Parcel Type",
    group: "parcel",
    sortable: true,
    graphqlFields: ["parcelType"],
  },
  // Dimensions
  {
    key: "weight",
    label: "Weight",
    group: "dimensions",
    sortable: true,
    graphqlFields: ["weight", "weightUnit"],
  },
  {
    key: "length",
    label: "Length",
    group: "dimensions",
    sortable: true,
    graphqlFields: ["length", "dimensionUnit"],
  },
  {
    key: "width",
    label: "Width",
    group: "dimensions",
    sortable: true,
    graphqlFields: ["width"],
  },
  {
    key: "height",
    label: "Height",
    group: "dimensions",
    sortable: true,
    graphqlFields: ["height"],
  },
  {
    key: "dimensionUnit",
    label: "Dim. Unit",
    group: "dimensions",
    sortable: true,
    graphqlFields: ["dimensionUnit"],
  },
  // Financials
  {
    key: "declaredValue",
    label: "Declared Value",
    group: "financials",
    sortable: true,
    graphqlFields: ["declaredValue"],
  },
  {
    key: "currency",
    label: "Currency",
    group: "financials",
    sortable: true,
    graphqlFields: ["currency"],
  },
  // Dates
  {
    key: "estimatedDeliveryDate",
    label: "Est. Delivery",
    group: "dates",
    sortable: true,
    graphqlFields: ["estimatedDeliveryDate"],
  },
  {
    key: "actualDeliveryDate",
    label: "Actual Delivery",
    group: "dates",
    sortable: true,
    graphqlFields: ["actualDeliveryDate"],
  },
  {
    key: "deliveryAttempts",
    label: "Delivery Attempts",
    group: "parcel",
    sortable: true,
    graphqlFields: ["deliveryAttempts"],
  },
  {
    key: "createdAt",
    label: "Created",
    group: "dates",
    sortable: true,
    graphqlFields: ["createdAt"],
  },
  // Recipient
  {
    key: "recipientContactName",
    label: "Recipient Name",
    group: "recipient",
    sortable: false,
    graphqlFields: ["recipientAddress {\n        contactName\n      }"],
  },
  {
    key: "recipientPhone",
    label: "Recipient Phone",
    group: "recipient",
    sortable: false,
    graphqlFields: ["recipientAddress {\n        phone\n      }"],
  },
  {
    key: "recipientEmail",
    label: "Recipient Email",
    group: "recipient",
    sortable: false,
    graphqlFields: ["recipientAddress {\n        email\n      }"],
  },
  {
    key: "recipientStreet",
    label: "Recipient Street",
    group: "recipient",
    sortable: false,
    graphqlFields: ["recipientAddress {\n        street1\n      }"],
  },
  {
    key: "recipientCity",
    label: "Recipient City",
    group: "recipient",
    sortable: false,
    graphqlFields: ["recipientAddress {\n        city\n      }"],
  },
  {
    key: "recipientState",
    label: "Recipient State",
    group: "recipient",
    sortable: false,
    graphqlFields: ["recipientAddress {\n        state\n      }"],
  },
  {
    key: "recipientPostalCode",
    label: "Recipient Postal Code",
    group: "recipient",
    sortable: false,
    graphqlFields: ["recipientAddress {\n        postalCode\n      }"],
  },
  {
    key: "recipientCountryCode",
    label: "Recipient Country",
    group: "recipient",
    sortable: false,
    graphqlFields: ["recipientAddress {\n        countryCode\n      }"],
  },
  // Shipper
  {
    key: "shipperContactName",
    label: "Shipper Name",
    group: "shipper",
    sortable: false,
    graphqlFields: ["shipperAddress {\n        contactName\n      }"],
  },
  {
    key: "shipperCity",
    label: "Shipper City",
    group: "shipper",
    sortable: false,
    graphqlFields: ["shipperAddress {\n        city\n      }"],
  },
  // Zone
  {
    key: "zone",
    label: "Zone",
    group: "zone",
    sortable: false,
    graphqlFields: ["zone {\n        id\n        name\n      }"],
  },
  // Depot
  {
    key: "depot",
    label: "Depot",
    group: "depot",
    sortable: false,
    graphqlFields: ["depot {\n        id\n        name\n      }"],
  },
  // Actions
  {
    key: "link",
    label: "Link",
    group: "actions",
    sortable: false,
    graphqlFields: [],
  },
];

export const DEFAULT_COLUMNS: ColumnKey[] = [
  "trackingNumber",
  "status",
  "serviceType",
  "parcelType",
  "weight",
  "estimatedDeliveryDate",
  "createdAt",
  "recipientContactName",
  "recipientStreet",
  "recipientCity",
  "recipientState",
  "recipientPostalCode",
  "recipientCountryCode",
  "zone",
  "depot",
  "link",
];
