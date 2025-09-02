# Redis Search API - Docker Configuration
# Multi-stage build for optimized production image
#
# ⚠️  WARNING: THIS IS REFERENCE CODE ONLY - NOT PRODUCTION READY
# ⚠️  Missing: Security hardening, non-root user configuration
# ⚠️  Missing: Proper secrets management, security scanning
# ⚠️  Missing: Resource limits, health check optimization

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy source code and build
COPY . ./
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 5001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5001/api/redis/health || exit 1

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5001

# Start application
ENTRYPOINT ["dotnet", "RedisApp.dll"]
