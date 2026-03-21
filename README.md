# LastMile TMS

Monorepo containing three applications:

- **API** (`src/backend`) — .NET 10 backend with Clean Architecture
- **Web** (`src/web`) — Next.js 15 dispatcher UI with NextAuth v5
- **Mobile** (`src/mobile`) — Expo driver app

## Quick Start

```bash
# 1. Copy the environment template (required for docker-compose variable interpolation)
cp .env.example .env

# 2. Generate a NextAuth secret
# Run: openssl rand -base64 32
# Then paste the result as AUTH_SECRET in .env

# 3. Start all services
docker compose up --build
```

> **Note**: Docker Compose automatically loads `.env` for variable interpolation (`${VAR}`).
> The file is gitignored so secrets don't get committed. The template is `.env.example`.

All services will be available at:
- **Web UI**: http://localhost
- **API**: http://localhost/api
- **Swagger**: http://localhost/swagger
- **Hangfire**: http://localhost/hangfire
- **Seq Logs**: http://localhost/seq

## Environment Configuration

All environment variables are defined in `.env.example`. This file is a **template** — copy it to `.env` and fill in actual values.

### How it works

1. `.env.example` is the template (committed to git, contains placeholder values)
2. `.env` is your private config (NOT committed, contains real values)
3. Docker Compose automatically reads `.env` for `${VAR}` interpolation
4. The web service `env_file: .env` passes runtime vars to the container

### Important: `NEXT_PUBLIC_*` variables

Next.js bakes `NEXT_PUBLIC_*` variables into the JavaScript bundle at **build time**, not runtime. This means:

1. They must be present when `npm run build` runs inside the Docker build
2. Changes to these vars require a rebuild: `docker compose up --build`

For local development, `NEXT_PUBLIC_API_URL` should point to `http://localhost`. In production, update it to your public domain.

### API URLs: `NEXT_PUBLIC_API_URL` vs `API_INTERNAL_URL`

The web app uses two different URLs to reach the API:

| Variable | Used By | Purpose | Docker Value | Local Dev Value |
|----------|---------|---------|--------------|----------------|
| `NEXT_PUBLIC_API_URL` | Browser (client-side) | Browser makes API calls through Caddy | `http://localhost` | `http://localhost` |
| `API_INTERNAL_URL` | Next.js server (NextAuth authorize) | Server-side calls to OpenIddict token endpoint | `http://api:8080` | `http://localhost:8080` |

In Docker, the web container cannot use `localhost` to reach the API (it reaches itself). Use the Docker service name `api:8080` for internal communication.

### Updating environment variables

Modify `.env` (not `.env.example`). Rebuild to apply changes that affect the web app:

```bash
docker compose up --build
```

For API-only changes (connection strings, secrets, etc.), you can restart just the API container:

```bash
docker compose restart api
```

> **Note**: Some containers have init scripts that run only once (e.g., Seq sets its admin password on first run, PostgreSQL initializes data directory). For changes to affect these containers, you must delete their volumes and recreate:
>
> ```bash
> docker compose down -v && docker compose up --build
> ```
>
> The `-v` flag removes volumes (`pgdata`, `seqdata`). Use this when changing Seq password or resetting the database.

## Default Credentials

After first startup, the API seeds an admin user:

- **Username**: `admin`
- **Email**: `admin@lastmile.com`
- **Password**: `Admin@123`

## Development

### Backend

```bash
cd src/backend
dotnet restore
dotnet build --no-restore
dotnet run --project src/LastMile.TMS.Api
```

### Web

```bash
cd src/web
# For local dev, create .env.local with API_INTERNAL_URL pointing to API on host:
echo "API_INTERNAL_URL=http://localhost:8080" > .env.local
npm ci
npm run dev
```

> **Note**: When running `npm run dev` locally, set `API_INTERNAL_URL=http://localhost:8080` in `.env.local` so NextAuth can reach the API directly (bypassing Caddy).

### pgadmin

pgadmin is available at http://localhost:5050 for now it should be used only for development.

initial setup:
1. right click on `Servers`
2. choose `Register`
3. choose `Server`
4. fill out `Name`
5. select `Connection` tab
6. fill host name/address `postgres` and password `postgres` (default values from .env.example)

done, now you can
1. go to `Servers`
2. select your server name
3. select `Databases`
4. select `lastmile_tms`
5. select `Schemas`
6. select `public`
7. select `Tables`

or just click `Tools`, `PSQL tool` and write `\dt` to list all tables.

## Architecture

See [CLAUDE.md](./CLAUDE.md) for detailed architecture documentation.
