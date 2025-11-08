# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PostManage/PostManage.csproj", "PostManage/"]
RUN dotnet restore "PostManage/PostManage.csproj"

# Copy everything else and build
COPY PostManage/ PostManage/
RUN dotnet build "PostManage/PostManage.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
WORKDIR /src
# Install .NET EF Core tools in build stage (has SDK)
RUN dotnet tool install --global dotnet-ef --version 8.0.0
RUN dotnet publish "PostManage/PostManage.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime (using SDK image to run migrations)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Install PostgreSQL client for health checks
RUN apt-get update && \
    apt-get install -y postgresql-client && \
    rm -rf /var/lib/apt/lists/*

# Install .NET EF Core tools
RUN dotnet tool install --global dotnet-ef --version 8.0.0
ENV PATH="$PATH:/root/.dotnet/tools"

EXPOSE 10000

# Copy published app
COPY --from=publish /app/publish .

# Copy source code for migrations (needed for EF Core migrations)
COPY --from=build /src/PostManage /app/src

# Copy entrypoint script
COPY PostManage/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Set environment variables (PORT will be set by Render)
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/app/entrypoint.sh"]

