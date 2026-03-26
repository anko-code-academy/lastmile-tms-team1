#!/bin/bash
set -e
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres -tc "SELECT 1 FROM pg_database WHERE datname = 'lastmile_tms_hangfire'" | grep -q 1 || \
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres -c "CREATE DATABASE lastmile_tms_hangfire;"
