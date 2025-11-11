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

        # Normalize connection string (trim whitespace around keys/values)
        NORMALIZED_CONNECTION_STRING=""
        IFS=';' read -ra SEGMENTS <<< "$CONNECTION_STRING"
        for SEGMENT in "${SEGMENTS[@]}"; do
            TRIMMED_SEGMENT=$(echo "$SEGMENT" | xargs)
            if [ -z "$TRIMMED_SEGMENT" ]; then
                continue
            fi

            KEY_PART=${TRIMMED_SEGMENT%%=*}
            VALUE_PART=${TRIMMED_SEGMENT#*=}
            KEY_PART=$(echo "$KEY_PART" | xargs)
            VALUE_PART=$(echo "$VALUE_PART" | xargs)

            if [ -z "$KEY_PART" ] || [ -z "$VALUE_PART" ]; then
                continue
            fi

            if [ -n "$NORMALIZED_CONNECTION_STRING" ]; then
                NORMALIZED_CONNECTION_STRING="$NORMALIZED_CONNECTION_STRING;"
            fi

            NORMALIZED_CONNECTION_STRING="$NORMALIZED_CONNECTION_STRING$KEY_PART=$VALUE_PART"
        done
        unset IFS

        if [ -z "$NORMALIZED_CONNECTION_STRING" ]; then
            echo "WARNING: Failed to normalize connection string, skipping migrations"
        else
            export ConnectionStrings__DefaultConnection="$NORMALIZED_CONNECTION_STRING"
            dotnet ef database update --project PostManage.csproj --connection "$NORMALIZED_CONNECTION_STRING" || echo "Migration failed or already applied"
        fi
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

