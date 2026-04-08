import {
  type ColumnKey,
  COLUMN_REGISTRY,
} from "@/components/parcel/column-registry";

/**
 * Parse a graphqlFields entry like "recipientAddress {\n  contactName\n}"
 * into { alias: "recipientAddress", subFields: ["contactName"] }.
 * Returns null for simple top-level fields like "status".
 */
function parseNestedField(
  raw: string,
): { alias: string; subFields: string[] } | null {
  const match = raw.match(/^(\w+)\s*\{([\s\S]+)\}$/);
  if (!match) return null;
  return {
    alias: match[1],
    subFields: match[2]
      .split("\n")
      .map((s) => s.trim())
      .filter(Boolean),
  };
}

export function buildParcelsQuery(columns: ColumnKey[]): string {
  const topFields = new Set<string>(["id"]);
  const nested = new Map<string, Set<string>>();

  for (const key of columns) {
    const def = COLUMN_REGISTRY.find((c) => c.key === key);
    if (!def) continue;
    for (const raw of def.graphqlFields) {
      const parsed = parseNestedField(raw);
      if (parsed) {
        let set = nested.get(parsed.alias);
        if (!set) {
          set = new Set();
          nested.set(parsed.alias, set);
        }
        for (const sf of parsed.subFields) set.add(sf);
      } else {
        topFields.add(raw);
      }
    }
  }

  // Collect dimensionUnit alongside any dimension column that needs it
  // (already handled by graphqlFields including it)

  const parts: string[] = [];

  for (const f of topFields) parts.push(f);

  for (const [alias, subFields] of nested) {
    const fields = Array.from(subFields)
      .map((f) => `        ${f}`)
      .join("\n");
    parts.push(`${alias} {\n${fields}\n      }`);
  }

  const nodesFields = parts.join("\n      ");

  return `query GetParcels($recipientSearch: String, $addressSearch: String, $where: ParcelFilterInput, $order: [ParcelSortInput!], $first: Int, $after: String) {
  parcels(recipientSearch: $recipientSearch, addressSearch: $addressSearch, where: $where, order: $order, first: $first, after: $after) {
    nodes {
      ${nodesFields}
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
      startCursor
      endCursor
    }
    totalCount
  }
}`;
}
