# =============================================================================
# ACommerce Marketplace API - Google Cloud Platform Edition
# Optimized multi-stage Dockerfile for production deployment
# =============================================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Install dependencies for native compilation (if needed)
RUN apk add --no-cache icu-libs

# Copy solution and project files first (for layer caching)
COPY ["ACommerce.Libraries.sln", "./"]
COPY ["Apps/ACommerce.Marketplace.GCP/ACommerce.Marketplace.GCP.csproj", "Apps/ACommerce.Marketplace.GCP/"]

# Copy all library project files
COPY ["libs/backend/core/ACommerce.SharedKernel.Abstractions/ACommerce.SharedKernel.Abstractions.csproj", "libs/backend/core/ACommerce.SharedKernel.Abstractions/"]
COPY ["libs/backend/core/ACommerce.SharedKernel.CQRS/ACommerce.SharedKernel.CQRS.csproj", "libs/backend/core/ACommerce.SharedKernel.CQRS/"]
COPY ["libs/backend/core/ACommerce.SharedKernel.Infrastructure.EFCores/ACommerce.SharedKernel.Infrastructure.EFCores.csproj", "libs/backend/core/ACommerce.SharedKernel.Infrastructure.EFCores/"]
COPY ["libs/backend/core/ACommerce.Configuration/ACommerce.Configuration.csproj", "libs/backend/core/ACommerce.Configuration/"]
COPY ["libs/backend/integration/ACommerce.SharedKernel.AspNetCore/ACommerce.SharedKernel.AspNetCore.csproj", "libs/backend/integration/ACommerce.SharedKernel.AspNetCore/"]
COPY ["libs/backend/integration/ACommerce.Authentication.AspNetCore/ACommerce.Authentication.AspNetCore.csproj", "libs/backend/integration/ACommerce.Authentication.AspNetCore/"]
COPY ["libs/backend/integration/ACommerce.Authentication.AspNetCore.Swagger/ACommerce.Authentication.AspNetCore.Swagger.csproj", "libs/backend/integration/ACommerce.Authentication.AspNetCore.Swagger/"]
COPY ["libs/backend/auth/ACommerce.Authentication.Abstractions/ACommerce.Authentication.Abstractions.csproj", "libs/backend/auth/ACommerce.Authentication.Abstractions/"]
COPY ["libs/backend/auth/ACommerce.Authentication.JWT/ACommerce.Authentication.JWT.csproj", "libs/backend/auth/ACommerce.Authentication.JWT/"]
COPY ["libs/backend/auth/ACommerce.Authentication.Users.Abstractions/ACommerce.Authentication.Users.Abstractions.csproj", "libs/backend/auth/ACommerce.Authentication.Users.Abstractions/"]
COPY ["libs/backend/auth/ACommerce.Authentication.TwoFactor.Abstractions/ACommerce.Authentication.TwoFactor.Abstractions.csproj", "libs/backend/auth/ACommerce.Authentication.TwoFactor.Abstractions/"]
COPY ["libs/backend/auth/ACommerce.Authentication.TwoFactor.SessionStore.Redis/ACommerce.Authentication.TwoFactor.SessionStore.Redis.csproj", "libs/backend/auth/ACommerce.Authentication.TwoFactor.SessionStore.Redis/"]
COPY ["libs/backend/auth/ACommerce.Profiles/ACommerce.Profiles.csproj", "libs/backend/auth/ACommerce.Profiles/"]
COPY ["libs/backend/auth/ACommerce.Profiles.Api/ACommerce.Profiles.Api.csproj", "libs/backend/auth/ACommerce.Profiles.Api/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Attributes/ACommerce.Catalog.Attributes.csproj", "libs/backend/catalog/ACommerce.Catalog.Attributes/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Attributes.Api/ACommerce.Catalog.Attributes.Api.csproj", "libs/backend/catalog/ACommerce.Catalog.Attributes.Api/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Products/ACommerce.Catalog.Products.csproj", "libs/backend/catalog/ACommerce.Catalog.Products/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Products.Api/ACommerce.Catalog.Products.Api.csproj", "libs/backend/catalog/ACommerce.Catalog.Products.Api/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Listings/ACommerce.Catalog.Listings.csproj", "libs/backend/catalog/ACommerce.Catalog.Listings/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Listings.Api/ACommerce.Catalog.Listings.Api.csproj", "libs/backend/catalog/ACommerce.Catalog.Listings.Api/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Units/ACommerce.Catalog.Units.csproj", "libs/backend/catalog/ACommerce.Catalog.Units/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Units.Api/ACommerce.Catalog.Units.Api.csproj", "libs/backend/catalog/ACommerce.Catalog.Units.Api/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Currencies/ACommerce.Catalog.Currencies.csproj", "libs/backend/catalog/ACommerce.Catalog.Currencies/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Currencies.Api/ACommerce.Catalog.Currencies.Api.csproj", "libs/backend/catalog/ACommerce.Catalog.Currencies.Api/"]
COPY ["libs/backend/marketplace/ACommerce.Vendors/ACommerce.Vendors.csproj", "libs/backend/marketplace/ACommerce.Vendors/"]
COPY ["libs/backend/marketplace/ACommerce.Vendors.Api/ACommerce.Vendors.Api.csproj", "libs/backend/marketplace/ACommerce.Vendors.Api/"]
COPY ["libs/backend/marketplace/ACommerce.Subscriptions/ACommerce.Subscriptions.csproj", "libs/backend/marketplace/ACommerce.Subscriptions/"]
COPY ["libs/backend/marketplace/ACommerce.Subscriptions.Api/ACommerce.Subscriptions.Api.csproj", "libs/backend/marketplace/ACommerce.Subscriptions.Api/"]
COPY ["libs/backend/sales/ACommerce.Cart/ACommerce.Cart.csproj", "libs/backend/sales/ACommerce.Cart/"]
COPY ["libs/backend/sales/ACommerce.Orders/ACommerce.Orders.csproj", "libs/backend/sales/ACommerce.Orders/"]
COPY ["libs/backend/sales/ACommerce.Orders.Api/ACommerce.Orders.Api.csproj", "libs/backend/sales/ACommerce.Orders.Api/"]
COPY ["libs/backend/sales/ACommerce.Payments.Abstractions/ACommerce.Payments.Abstractions.csproj", "libs/backend/sales/ACommerce.Payments.Abstractions/"]
COPY ["libs/backend/sales/ACommerce.Payments.Api/ACommerce.Payments.Api.csproj", "libs/backend/sales/ACommerce.Payments.Api/"]
COPY ["libs/backend/other/ACommerce.Transactions.Core/ACommerce.Transactions.Core.csproj", "libs/backend/other/ACommerce.Transactions.Core/"]
COPY ["libs/backend/other/ACommerce.Transactions.Core.Api/ACommerce.Transactions.Core.Api.csproj", "libs/backend/other/ACommerce.Transactions.Core.Api/"]
COPY ["libs/backend/files/ACommerce.Files.Abstractions/ACommerce.Files.Abstractions.csproj", "libs/backend/files/ACommerce.Files.Abstractions/"]
COPY ["libs/backend/files/ACommerce.Files.Storage.GoogleCloud/ACommerce.Files.Storage.GoogleCloud.csproj", "libs/backend/files/ACommerce.Files.Storage.GoogleCloud/"]
COPY ["libs/backend/integration/ACommerce.Files.AspNetCore/ACommerce.Files.AspNetCore.csproj", "libs/backend/integration/ACommerce.Files.AspNetCore/"]
COPY ["libs/backend/messaging/ACommerce.Messaging.Abstractions/ACommerce.Messaging.Abstractions.csproj", "libs/backend/messaging/ACommerce.Messaging.Abstractions/"]
COPY ["libs/backend/messaging/ACommerce.Messaging.InMemory/ACommerce.Messaging.InMemory.csproj", "libs/backend/messaging/ACommerce.Messaging.InMemory/"]

# Restore dependencies
RUN dotnet restore "Apps/ACommerce.Marketplace.GCP/ACommerce.Marketplace.GCP.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/Apps/ACommerce.Marketplace.GCP"
RUN dotnet publish "ACommerce.Marketplace.GCP.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false

# =============================================================================
# Stage 2: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime

# Security: Run as non-root user
RUN addgroup -g 1000 acommerce && \
    adduser -u 1000 -G acommerce -s /bin/sh -D acommerce

WORKDIR /app

# Install runtime dependencies
RUN apk add --no-cache \
    icu-libs \
    tzdata \
    ca-certificates \
    && update-ca-certificates

# Copy published app
COPY --from=build --chown=acommerce:acommerce /app/publish .

# Switch to non-root user
USER acommerce

# Environment defaults
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=UTC

# Cloud Run uses PORT environment variable
ENV PORT=8080
EXPOSE 8080

# Health check for container orchestrators
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:${PORT}/healthz || exit 1

# Entry point
ENTRYPOINT ["dotnet", "ACommerce.Marketplace.GCP.dll"]
