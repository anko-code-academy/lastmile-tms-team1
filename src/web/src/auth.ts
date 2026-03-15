import NextAuth from "next-auth";
import type { NextAuthConfig } from "next-auth";
import Credentials from "next-auth/providers/credentials";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:80";

export const authConfig: NextAuthConfig = {
  providers: [
    Credentials({
      name: "Credentials",
      credentials: {
        email: { label: "Email", type: "email" },
        password: { label: "Password", type: "password" },
      },
      authorize: async (credentials) => {
        if (!credentials?.email || !credentials?.password) {
          return null;
        }

        const params = new URLSearchParams();
        params.append("grant_type", "password");
        params.append("username", credentials.email as string); // accepts email or username
        params.append("password", credentials.password as string);
        params.append("client_id", "tms-client");
        params.append("client_secret", "tms-secret");

        const response = await fetch(`${API_URL}/connect/token`, {
          method: "POST",
          headers: {
            "Content-Type": "application/x-www-form-urlencoded",
          },
          body: params.toString(),
        });

        if (!response.ok) {
          const errorText = await response.text();
          if (errorText.includes("username and/or password")) {
            throw new Error("username and/or password parameters are missing");
          }
          throw new Error("Invalid credentials");
        }

        const data = await response.json();

        return {
          id: credentials.email as string,
          email: credentials.email as string,
          accessToken: data.access_token,
        };
      },
    }),
  ],
  pages: {
    signIn: "/login",
  },
  callbacks: {
    authorized({ auth, request: { nextUrl } }) {
      const isLoggedIn = !!auth?.user;
      const isOnProtectedPage =
        nextUrl.pathname.startsWith("/dashboard") ||
        nextUrl.pathname.startsWith("/admin");

      if (isOnProtectedPage) {
        if (isLoggedIn) return true;
        return false; // Redirect to login
      }
      return true;
    },
    jwt({ token, user }) {
      if (user) {
        token.accessToken = (user as { accessToken: string }).accessToken;
      }
      return token;
    },
    session({ session, token }) {
      if (token && session.user) {
        (session.user as { accessToken?: string }).accessToken =
          token.accessToken as string;
      }
      return session;
    },
  },
  session: {
    strategy: "jwt",
  },
};

export const { handlers, auth, signIn, signOut } = NextAuth(authConfig);
