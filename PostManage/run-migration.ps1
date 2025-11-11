# PowerShell script to run database migrations
# Usage: .\run-migration.ps1

Write-Host "Running database migrations..." -ForegroundColor Green

# Navigate to project directory
Set-Location $PSScriptRoot

# Run migration
dotnet ef database update

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nMigration completed successfully!" -ForegroundColor Green
    Write-Host "Checking migration status..." -ForegroundColor Yellow
    dotnet ef migrations list
} else {
    Write-Host "`nMigration failed!" -ForegroundColor Red
    exit 1
}

