#!/usr/bin/env bash
set -euo pipefail

# ── Wait until Postgres is ready ────────────────────────────────
echo " Waiting for Postgres at $DB_HOST:$DB_PORT ..."
until nc -z "$DB_HOST" "$DB_PORT"; do
  sleep 2
done
echo " Postgres is reachable"

# ── Start ASP.NET Core (it will apply migrations + seed) ────────
exec dotnet api.dll
