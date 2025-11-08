#!/bin/bash
set -e

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
# Use PORT from environment variable if set, otherwise use default 10000
PORT=${PORT:-10000}
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"
exec dotnet PostManage.dll

