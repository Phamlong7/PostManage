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
RUN dotnet publish "PostManage/PostManage.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install PostgreSQL client for health checks and EF Core tools for migrations
RUN apt-get update && \
    apt-get install -y postgresql-client && \
    rm -rf /var/lib/apt/lists/*

# Install .NET EF Core tools
RUN dotnet tool install --global dotnet-ef --version 8.0.0
ENV PATH="$PATH:/root/.dotnet/tools"

EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=publish /app/publish .

# Copy source code for migrations (needed for EF Core migrations)
COPY --from=build /src/PostManage /app/src

# Copy entrypoint script
COPY PostManage/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/app/entrypoint.sh"]

