#!/bin/bash
# Bash script to run database migrations
# Usage: ./run-migration.sh

echo "Running database migrations..."

# Navigate to script directory
cd "$(dirname "$0")"

# Run migration
dotnet ef database update

if [ $? -eq 0 ]; then
    echo ""
    echo "Migration completed successfully!"
    echo "Checking migration status..."
    dotnet ef migrations list
else
    echo ""
    echo "Migration failed!"
    exit 1
fi

