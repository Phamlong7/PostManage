#!/bin/bash
set -e

# Run migrations
echo "Running database migrations..."
if [ -f "/app/src/PostManage.csproj" ]; then
    cd /app/src
    
    # Get connection string from environment variable
    # Render sets it as ConnectionStrings__DefaultConnection
    # Use printenv to safely read environment variable with special characters
    CONNECTION_STRING=$(printenv ConnectionStrings__DefaultConnection || printenv CONNECTIONSTRINGS__DEFAULTCONNECTION || echo "")
    
    if [ -n "$CONNECTION_STRING" ]; then
        echo "Running migrations with connection string..."
        export ConnectionStrings__DefaultConnection="$CONNECTION_STRING"
        dotnet ef database update --project PostManage.csproj --connection "$CONNECTION_STRING" || echo "Migration failed or already applied"
    else
        echo "WARNING: Connection string not found, skipping migrations"
        echo "This may cause issues if database is not already migrated"
        echo "Available environment variables with 'connection' in name:"
        env | grep -i connection || echo "None found"
    fi
else
    echo "Project file not found, skipping migrations"
fi

# Start the application
echo "Starting application..."
cd /app

# Verify DLL exists
if [ ! -f "PostManage.dll" ]; then
    echo "ERROR: PostManage.dll not found in /app"
    echo "Contents of /app:"
    ls -la /app || true
    exit 1
fi

# Use PORT from environment variable if set, otherwise use default 10000
PORT=${PORT:-10000}
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"

echo "Starting application on port ${PORT}..."
exec dotnet PostManage.dll

