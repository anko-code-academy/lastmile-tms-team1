import NextAuth from "next-auth";
import Credentials from "next-auth/providers/credentials";

// Browser uses NEXT_PUBLIC_API_URL (reaches Caddy at localhost)
// Server-side (NextAuth) uses API_INTERNAL_URL (reaches API directly in Docker, or localhost in dev)
const API_BASE_URL = process.env.API_INTERNAL_URL ?? process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

export const { handlers, auth, signIn, signOut } = NextAuth({
  providers: [
    Credentials({
      name: "credentials",
      credentials: {
        username: { label: "Username", type: "text" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        if (!credentials?.username || !credentials?.password) {
          return null;
        }

        const params = new URLSearchParams({
          grant_type: "password",
          username: credentials.username as string,
          password: credentials.password as string,
          scope: "offline_access",
        });

        const res = await fetch(`${API_BASE_URL}/connect/token`, {
          method: "POST",
          headers: {
            "Content-Type": "application/x-www-form-urlencoded",
          },
          body: params.toString(),
        });

        if (!res.ok) {
          return null;
        }

        const data: {
          access_token: string;
          refresh_token: string;
          token_type: string;
          expires_in: number;
        } = await res.json();

        return {
          id: credentials.username as string,
          name: credentials.username as string,
          accessToken: data.access_token,
          refreshToken: data.refresh_token,
        };
      },
    }),
  ],
  pages: {
    signIn: "/login",
    error: "/login",
  },
  trustHost: true,
  session: {
    strategy: "jwt",
    maxAge: 7 * 24 * 60 * 60, // 7 days
  },
  callbacks: {
    async jwt({ token, user }) {
      if (user) {
        token.accessToken = user.accessToken;
        token.refreshToken = user.refreshToken;
        // Decode JWT to get exp, email and roles (payload[1] is base64url)
        const payload = (user.accessToken as string).split('.')[1];
        const decoded = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
        token.accessTokenExp = decoded.exp as number;
        token.email = decoded.email as string;
        token.roles = decoded.role as string[] | undefined;
      }
      return token;
    },
    async session({ session, token }) {
      if (session.user) {
        session.user.accessToken = token.accessToken as string;
        session.user.refreshToken = token.refreshToken as string;
        session.user.accessTokenExp = token.accessTokenExp as number;
        session.user.email = token.email as string;
        session.user.roles = (token.roles as string[] | undefined) ?? [];
      }
      return session;
    },
  },
});
