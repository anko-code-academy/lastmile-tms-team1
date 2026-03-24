const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost";

export async function graphqlFetch<T>(
  query: string,
  variables?: Record<string, unknown>,
  token?: string
): Promise<T> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const body = { query, variables };
  console.log("GraphQL Request:", JSON.stringify(body, null, 2));

  const res = await fetch(`${API_BASE_URL}/api/graphql`, {
    method: "POST",
    headers,
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    const text = await res.text();
    console.error("GraphQL HTTP Error:", res.status, text);
    throw new Error(`GraphQL error: ${res.status}`);
  }

  const json = await res.json();
  console.log("GraphQL Response:", JSON.stringify(json, null, 2));

  if (json.errors && json.errors.length > 0) {
    console.error("GraphQL Errors:", json.errors);
    throw new Error(json.errors[0].message);
  }

  return json.data as T;
}
