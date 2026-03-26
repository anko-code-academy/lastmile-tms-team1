import { auth } from "@/auth";
import { NextResponse } from "next/server";

export default auth((req) => {
  const { nextUrl, auth: session } = req;
  const isLoggedIn = !!session?.user;
  const isLoginPage = nextUrl.pathname === "/login";
  const isPublicPath =
    nextUrl.pathname === "/" ||
    nextUrl.pathname.startsWith("/login") ||
    nextUrl.pathname.startsWith("/_next") ||
    nextUrl.pathname.startsWith("/favicon");

  // Check if access token has expired
  if (session?.user?.accessTokenExp) {
    const now = Math.floor(Date.now() / 1000);
    if (session.user.accessTokenExp < now) {
      const callbackUrl = nextUrl.pathname;
      return NextResponse.redirect(
        new URL(`/login?callbackUrl=${callbackUrl}`, nextUrl.origin)
      );
    }
  }

  if (!isLoggedIn && !isPublicPath) {
    const callbackUrl = nextUrl.pathname;
    return NextResponse.redirect(
      new URL(`/login?callbackUrl=${callbackUrl}`, nextUrl.origin)
    );
  }

  if (isLoggedIn && isLoginPage) {
    const callbackUrl = nextUrl.searchParams.get("callbackUrl") ?? "/dashboard";
    return NextResponse.redirect(new URL(callbackUrl, nextUrl.origin));
  }

  return NextResponse.next();
});

export const config = {
  matcher: ["/((?!api/auth|login|_next/static|_next/image|favicon.ico|public).*)"],
};
