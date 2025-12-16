# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Set environment variable to disable NuGet package generation
ENV ACOMMERCE_BUILD_MODE=CLOUD

# Copy everything
COPY . .

# Restore first with verbose output
WORKDIR "/src/Apps/Ashare.Api"
RUN dotnet restore "Ashare.Api.csproj" --verbosity normal

# Build with more verbose output to see actual errors
RUN dotnet build "Ashare.Api.csproj" -c Release --no-restore -v normal

# Publish
RUN dotnet publish "Ashare.Api.csproj" -c Release -o /app/publish --no-build

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install required packages for health checks
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published files
COPY --from=build /app/publish .

# Create directories for data and logs
RUN mkdir -p /app/data /app/logs /app/uploads && \
    chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV PORT=8080

# Expose port
EXPOSE 8080

# Health check (disabled - Cloud Run handles this)
# HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
#     CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Ashare.Api.dll"]
