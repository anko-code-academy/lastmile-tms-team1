import { GraphQLClient } from "graphql-request";
import { getSession } from "next-auth/react";

const API_URL =
  process.env.NEXT_PUBLIC_API_URL
    ? `${process.env.NEXT_PUBLIC_API_URL}/api/graphql`
    : "http://localhost:5000/api/graphql";

export async function getGraphQLClient(): Promise<GraphQLClient> {
  const session = await getSession();

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };

  if (session?.user?.accessToken) {
    headers["Authorization"] = `Bearer ${session.user.accessToken}`;
  }

  return new GraphQLClient(API_URL, { headers });
}

export function getGraphQLClientFromToken(accessToken: string): GraphQLClient {
  return new GraphQLClient(API_URL, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
  });
}
