#!/bin/bash
set -e

echo "Waiting for PostgreSQL to be ready..."
until pg_isready -h postgres -p 5432 -U postgres; do
  echo "PostgreSQL is unavailable - sleeping"
  sleep 1
done

echo "PostgreSQL is up - executing command"

# Run migrations
echo "Running database migrations..."
if [ -f "/app/src/PostManage.csproj" ]; then
    cd /app/src
    dotnet ef database update --project PostManage.csproj || echo "Migration failed or already applied"
else
    echo "Project file not found, skipping migrations"
fi

# Start the application
echo "Starting application..."
exec dotnet PostManage.dll

