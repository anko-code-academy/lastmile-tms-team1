import { auth } from "@/auth";

export default async function DashboardPage() {
  const session = await auth();

  return (
    <div className="p-6">
      <h1 className="text-3xl font-bold mb-4">
        Welcome, {session?.user?.name}
      </h1>
      <p className="text-muted-foreground">
        You are now logged in to Last Mile TMS
      </p>
    </div>
  );
}
