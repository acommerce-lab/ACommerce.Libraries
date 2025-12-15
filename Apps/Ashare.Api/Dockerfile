# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and all project files first for better caching
COPY ["ACommerce.Libraries.sln", "./"]

# Apps
COPY ["Apps/Ashare.Api/Ashare.Api.csproj", "Apps/Ashare.Api/"]
COPY ["Apps/Ashare.Shared/Ashare.Shared.csproj", "Apps/Ashare.Shared/"]

# libs/backend/core
COPY ["libs/backend/core/ACommerce.SharedKernel.Abstractions/ACommerce.SharedKernel.Abstractions.csproj", "libs/backend/core/ACommerce.SharedKernel.Abstractions/"]
COPY ["libs/backend/core/ACommerce.SharedKernel.CQRS/ACommerce.SharedKernel.CQRS.csproj", "libs/backend/core/ACommerce.SharedKernel.CQRS/"]
COPY ["libs/backend/core/ACommerce.SharedKernel.Infrastructure.EFCores/ACommerce.SharedKernel.Infrastructure.EFCores.csproj", "libs/backend/core/ACommerce.SharedKernel.Infrastructure.EFCores/"]
COPY ["libs/backend/core/ACommerce.Configuration/ACommerce.Configuration.csproj", "libs/backend/core/ACommerce.Configuration/"]
COPY ["libs/backend/core/ACommerce.Locations.Abstractions/ACommerce.Locations.Abstractions.csproj", "libs/backend/core/ACommerce.Locations.Abstractions/"]

# libs/backend/auth
COPY ["libs/backend/auth/ACommerce.Authentication.Abstractions/ACommerce.Authentication.Abstractions.csproj", "libs/backend/auth/ACommerce.Authentication.Abstractions/"]
COPY ["libs/backend/auth/ACommerce.Authentication.JWT/ACommerce.Authentication.JWT.csproj", "libs/backend/auth/ACommerce.Authentication.JWT/"]
COPY ["libs/backend/auth/ACommerce.Authentication.MicrosoftIdentity/ACommerce.Authentication.MicrosoftIdentity.csproj", "libs/backend/auth/ACommerce.Authentication.MicrosoftIdentity/"]
COPY ["libs/backend/auth/ACommerce.Authentication.OpenIddict/ACommerce.Authentication.OpenIddict.csproj", "libs/backend/auth/ACommerce.Authentication.OpenIddict/"]
COPY ["libs/backend/auth/ACommerce.Authentication.TwoFactor.Abstractions/ACommerce.Authentication.TwoFactor.Abstractions.csproj", "libs/backend/auth/ACommerce.Authentication.TwoFactor.Abstractions/"]
COPY ["libs/backend/auth/ACommerce.Authentication.TwoFactor.Email/ACommerce.Authentication.TwoFactor.Email.csproj", "libs/backend/auth/ACommerce.Authentication.TwoFactor.Email/"]
COPY ["libs/backend/auth/ACommerce.Authentication.TwoFactor.Nafath/ACommerce.Authentication.TwoFactor.Nafath.csproj", "libs/backend/auth/ACommerce.Authentication.TwoFactor.Nafath/"]
COPY ["libs/backend/auth/ACommerce.Authentication.TwoFactor.SMS/ACommerce.Authentication.TwoFactor.SMS.csproj", "libs/backend/auth/ACommerce.Authentication.TwoFactor.SMS/"]
COPY ["libs/backend/auth/ACommerce.Authentication.Users.Abstractions/ACommerce.Authentication.Users.Abstractions.csproj", "libs/backend/auth/ACommerce.Authentication.Users.Abstractions/"]
COPY ["libs/backend/auth/ACommerce.Profiles/ACommerce.Profiles.csproj", "libs/backend/auth/ACommerce.Profiles/"]
COPY ["libs/backend/auth/ACommerce.Profiles.Api/ACommerce.Profiles.Api.csproj", "libs/backend/auth/ACommerce.Profiles.Api/"]
COPY ["libs/backend/auth/ACommerce.Profiles.Core/ACommerce.Profiles.Core.csproj", "libs/backend/auth/ACommerce.Profiles.Core/"]
COPY ["libs/backend/auth/ACommerce.Profiles.Messaging/ACommerce.Profiles.Messaging.csproj", "libs/backend/auth/ACommerce.Profiles.Messaging/"]
COPY ["libs/backend/auth/ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework/ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework.csproj", "libs/backend/auth/ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework/"]
COPY ["libs/backend/auth/ACommerce.Authentication.TwoFactor.SessionStore.InMemory/ACommerce.Authentication.TwoFactor.SessionStore.InMemory.csproj", "libs/backend/auth/ACommerce.Authentication.TwoFactor.SessionStore.InMemory/"]

# libs/backend/catalog
COPY ["libs/backend/catalog/ACommerce.Catalog.Listings/ACommerce.Catalog.Listings.csproj", "libs/backend/catalog/ACommerce.Catalog.Listings/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Listings.Api/ACommerce.Catalog.Listings.Api.csproj", "libs/backend/catalog/ACommerce.Catalog.Listings.Api/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Attributes/ACommerce.Catalog.Attributes.csproj", "libs/backend/catalog/ACommerce.Catalog.Attributes/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Attributes.Api/ACommerce.Catalog.Attributes.Api.csproj", "libs/backend/catalog/ACommerce.Catalog.Attributes.Api/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Currencies/ACommerce.Catalog.Currencies.csproj", "libs/backend/catalog/ACommerce.Catalog.Currencies/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Currencies.Api/ACommerce.Catalog.Currencies.Api.csproj", "libs/backend/catalog/ACommerce.Catalog.Currencies.Api/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Products/ACommerce.Catalog.Products.csproj", "libs/backend/catalog/ACommerce.Catalog.Products/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Products.Api/ACommerce.Catalog.Products.Api.csproj", "libs/backend/catalog/ACommerce.Catalog.Products.Api/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Units/ACommerce.Catalog.Units.csproj", "libs/backend/catalog/ACommerce.Catalog.Units/"]
COPY ["libs/backend/catalog/ACommerce.Catalog.Units.Api/ACommerce.Catalog.Units.Api.csproj", "libs/backend/catalog/ACommerce.Catalog.Units.Api/"]

# libs/backend/sales
COPY ["libs/backend/sales/ACommerce.Cart/ACommerce.Cart.csproj", "libs/backend/sales/ACommerce.Cart/"]
COPY ["libs/backend/sales/ACommerce.Orders/ACommerce.Orders.csproj", "libs/backend/sales/ACommerce.Orders/"]
COPY ["libs/backend/sales/ACommerce.Orders.Api/ACommerce.Orders.Api.csproj", "libs/backend/sales/ACommerce.Orders.Api/"]
COPY ["libs/backend/sales/ACommerce.Payments.Abstractions/ACommerce.Payments.Abstractions.csproj", "libs/backend/sales/ACommerce.Payments.Abstractions/"]
COPY ["libs/backend/sales/ACommerce.Payments.Api/ACommerce.Payments.Api.csproj", "libs/backend/sales/ACommerce.Payments.Api/"]
COPY ["libs/backend/sales/ACommerce.Payments.Moyasar/ACommerce.Payments.Moyasar.csproj", "libs/backend/sales/ACommerce.Payments.Moyasar/"]
COPY ["libs/backend/sales/ACommerce.Payments.Noon/ACommerce.Payments.Noon.csproj", "libs/backend/sales/ACommerce.Payments.Noon/"]

# libs/backend/marketplace
COPY ["libs/backend/marketplace/ACommerce.Subscriptions/ACommerce.Subscriptions.csproj", "libs/backend/marketplace/ACommerce.Subscriptions/"]
COPY ["libs/backend/marketplace/ACommerce.Subscriptions.Api/ACommerce.Subscriptions.Api.csproj", "libs/backend/marketplace/ACommerce.Subscriptions.Api/"]
COPY ["libs/backend/marketplace/ACommerce.Vendors/ACommerce.Vendors.csproj", "libs/backend/marketplace/ACommerce.Vendors/"]
COPY ["libs/backend/marketplace/ACommerce.Vendors.Api/ACommerce.Vendors.Api.csproj", "libs/backend/marketplace/ACommerce.Vendors.Api/"]

# libs/backend/messaging
COPY ["libs/backend/messaging/ACommerce.Realtime.Abstractions/ACommerce.Realtime.Abstractions.csproj", "libs/backend/messaging/ACommerce.Realtime.Abstractions/"]
COPY ["libs/backend/messaging/ACommerce.Notifications.Abstractions/ACommerce.Notifications.Abstractions.csproj", "libs/backend/messaging/ACommerce.Notifications.Abstractions/"]
COPY ["libs/backend/messaging/ACommerce.Chats.Abstractions/ACommerce.Chats.Abstractions.csproj", "libs/backend/messaging/ACommerce.Chats.Abstractions/"]
COPY ["libs/backend/messaging/ACommerce.Chats.Core/ACommerce.Chats.Core.csproj", "libs/backend/messaging/ACommerce.Chats.Core/"]
COPY ["libs/backend/messaging/ACommerce.Chats.Api/ACommerce.Chats.Api.csproj", "libs/backend/messaging/ACommerce.Chats.Api/"]
COPY ["libs/backend/messaging/ACommerce.Messaging.Abstractions/ACommerce.Messaging.Abstractions.csproj", "libs/backend/messaging/ACommerce.Messaging.Abstractions/"]
COPY ["libs/backend/messaging/ACommerce.Messaging.InMemory/ACommerce.Messaging.InMemory.csproj", "libs/backend/messaging/ACommerce.Messaging.InMemory/"]
COPY ["libs/backend/messaging/ACommerce.Messaging.SignalR/ACommerce.Messaging.SignalR.csproj", "libs/backend/messaging/ACommerce.Messaging.SignalR/"]
COPY ["libs/backend/messaging/ACommerce.Messaging.SignalR.Hub/ACommerce.Messaging.SignalR.Hub.csproj", "libs/backend/messaging/ACommerce.Messaging.SignalR.Hub/"]
COPY ["libs/backend/messaging/ACommerce.Notifications.Core/ACommerce.Notifications.Core.csproj", "libs/backend/messaging/ACommerce.Notifications.Core/"]
COPY ["libs/backend/messaging/ACommerce.Notifications.Channels.Email/ACommerce.Notifications.Channels.Email.csproj", "libs/backend/messaging/ACommerce.Notifications.Channels.Email/"]
COPY ["libs/backend/messaging/ACommerce.Notifications.Channels.Firebase/ACommerce.Notifications.Channels.Firebase.csproj", "libs/backend/messaging/ACommerce.Notifications.Channels.Firebase/"]
COPY ["libs/backend/messaging/ACommerce.Notifications.Channels.InApp/ACommerce.Notifications.Channels.InApp.csproj", "libs/backend/messaging/ACommerce.Notifications.Channels.InApp/"]
COPY ["libs/backend/messaging/ACommerce.Notifications.Recipients/ACommerce.Notifications.Recipients.csproj", "libs/backend/messaging/ACommerce.Notifications.Recipients/"]
COPY ["libs/backend/messaging/ACommerce.Notifications.Recipients.Api/ACommerce.Notifications.Recipients.Api.csproj", "libs/backend/messaging/ACommerce.Notifications.Recipients.Api/"]
COPY ["libs/backend/messaging/ACommerce.Notifications.Messaging/ACommerce.Notifications.Messaging.csproj", "libs/backend/messaging/ACommerce.Notifications.Messaging/"]
COPY ["libs/backend/messaging/ACommerce.Authentication.Messaging/ACommerce.Authentication.Messaging.csproj", "libs/backend/messaging/ACommerce.Authentication.Messaging/"]
COPY ["libs/backend/messaging/ACommerce.Realtime.SignalR/ACommerce.Realtime.SignalR.csproj", "libs/backend/messaging/ACommerce.Realtime.SignalR/"]

# libs/backend/files
COPY ["libs/backend/files/ACommerce.Files.Abstractions/ACommerce.Files.Abstractions.csproj", "libs/backend/files/ACommerce.Files.Abstractions/"]
COPY ["libs/backend/files/ACommerce.Files.Storage.Local/ACommerce.Files.Storage.Local.csproj", "libs/backend/files/ACommerce.Files.Storage.Local/"]
COPY ["libs/backend/files/ACommerce.Files.Storage.GoogleCloud/ACommerce.Files.Storage.GoogleCloud.csproj", "libs/backend/files/ACommerce.Files.Storage.GoogleCloud/"]
COPY ["libs/backend/files/ACommerce.Files.Storage.AliyunOSS/ACommerce.Files.Storage.AliyunOSS.csproj", "libs/backend/files/ACommerce.Files.Storage.AliyunOSS/"]

# libs/backend/shipping
COPY ["libs/backend/shipping/ACommerce.Shipping.Abstractions/ACommerce.Shipping.Abstractions.csproj", "libs/backend/shipping/ACommerce.Shipping.Abstractions/"]
COPY ["libs/backend/shipping/ACommerce.Shipping.Api/ACommerce.Shipping.Api.csproj", "libs/backend/shipping/ACommerce.Shipping.Api/"]
COPY ["libs/backend/shipping/ACommerce.Shipping.Mock/ACommerce.Shipping.Mock.csproj", "libs/backend/shipping/ACommerce.Shipping.Mock/"]

# libs/backend/integration
COPY ["libs/backend/integration/ACommerce.Authentication.AspNetCore/ACommerce.Authentication.AspNetCore.csproj", "libs/backend/integration/ACommerce.Authentication.AspNetCore/"]
COPY ["libs/backend/integration/ACommerce.Authentication.AspNetCore.NafathWH/ACommerce.Authentication.AspNetCore.NafathWH.csproj", "libs/backend/integration/ACommerce.Authentication.AspNetCore.NafathWH/"]
COPY ["libs/backend/integration/ACommerce.Authentication.AspNetCore.Swagger/ACommerce.Authentication.AspNetCore.Swagger.csproj", "libs/backend/integration/ACommerce.Authentication.AspNetCore.Swagger/"]
COPY ["libs/backend/integration/ACommerce.Files.AspNetCore/ACommerce.Files.AspNetCore.csproj", "libs/backend/integration/ACommerce.Files.AspNetCore/"]
COPY ["libs/backend/integration/ACommerce.SharedKernel.AspNetCore/ACommerce.SharedKernel.AspNetCore.csproj", "libs/backend/integration/ACommerce.SharedKernel.AspNetCore/"]
COPY ["libs/backend/integration/ACommerce.ServiceRegistry.Abstractions/ACommerce.ServiceRegistry.Abstractions.csproj", "libs/backend/integration/ACommerce.ServiceRegistry.Abstractions/"]
COPY ["libs/backend/integration/ACommerce.ServiceRegistry.Core/ACommerce.ServiceRegistry.Core.csproj", "libs/backend/integration/ACommerce.ServiceRegistry.Core/"]
COPY ["libs/backend/integration/ACommerce.ServiceRegistry.Server/ACommerce.ServiceRegistry.Server.csproj", "libs/backend/integration/ACommerce.ServiceRegistry.Server/"]

# libs/backend/other
COPY ["libs/backend/other/ACommerce.Accounting.Core/ACommerce.Accounting.Core.csproj", "libs/backend/other/ACommerce.Accounting.Core/"]
COPY ["libs/backend/other/ACommerce.Accounting.Core.Api/ACommerce.Accounting.Core.Api.csproj", "libs/backend/other/ACommerce.Accounting.Core.Api/"]
COPY ["libs/backend/other/ACommerce.Transactions.Core/ACommerce.Transactions.Core.csproj", "libs/backend/other/ACommerce.Transactions.Core/"]
COPY ["libs/backend/other/ACommerce.Transactions.Core.Api/ACommerce.Transactions.Core.Api.csproj", "libs/backend/other/ACommerce.Transactions.Core.Api/"]

# libs/backend/admin
COPY ["libs/backend/admin/ACommerce.Admin.Dashboard/ACommerce.Admin.Dashboard.csproj", "libs/backend/admin/ACommerce.Admin.Dashboard/"]
COPY ["libs/backend/admin/ACommerce.Admin.Dashboard.Api/ACommerce.Admin.Dashboard.Api.csproj", "libs/backend/admin/ACommerce.Admin.Dashboard.Api/"]
COPY ["libs/backend/admin/ACommerce.Admin.Orders/ACommerce.Admin.Orders.csproj", "libs/backend/admin/ACommerce.Admin.Orders/"]
COPY ["libs/backend/admin/ACommerce.Admin.Orders.Api/ACommerce.Admin.Orders.Api.csproj", "libs/backend/admin/ACommerce.Admin.Orders.Api/"]
COPY ["libs/backend/admin/ACommerce.Admin.Listings/ACommerce.Admin.Listings.csproj", "libs/backend/admin/ACommerce.Admin.Listings/"]
COPY ["libs/backend/admin/ACommerce.Admin.Listings.Api/ACommerce.Admin.Listings.Api.csproj", "libs/backend/admin/ACommerce.Admin.Listings.Api/"]
COPY ["libs/backend/admin/ACommerce.Admin.Reports/ACommerce.Admin.Reports.csproj", "libs/backend/admin/ACommerce.Admin.Reports/"]
COPY ["libs/backend/admin/ACommerce.Admin.Reports.Api/ACommerce.Admin.Reports.Api.csproj", "libs/backend/admin/ACommerce.Admin.Reports.Api/"]
COPY ["libs/backend/admin/ACommerce.Admin.AuditLog/ACommerce.Admin.AuditLog.csproj", "libs/backend/admin/ACommerce.Admin.AuditLog/"]
COPY ["libs/backend/admin/ACommerce.Admin.AuditLog.Api/ACommerce.Admin.AuditLog.Api.csproj", "libs/backend/admin/ACommerce.Admin.AuditLog.Api/"]

# libs/frontend
COPY ["libs/frontend/core/ACommerce.Client.Core/ACommerce.Client.Core.csproj", "libs/frontend/core/ACommerce.Client.Core/"]
COPY ["libs/frontend/clients/ACommerce.Client.Auth/ACommerce.Client.Auth.csproj", "libs/frontend/clients/ACommerce.Client.Auth/"]
COPY ["libs/frontend/clients/ACommerce.Client.Cart/ACommerce.Client.Cart.csproj", "libs/frontend/clients/ACommerce.Client.Cart/"]
COPY ["libs/frontend/clients/ACommerce.Client.Orders/ACommerce.Client.Orders.csproj", "libs/frontend/clients/ACommerce.Client.Orders/"]
COPY ["libs/frontend/clients/ACommerce.Client.Products/ACommerce.Client.Products.csproj", "libs/frontend/clients/ACommerce.Client.Products/"]
COPY ["libs/frontend/clients/ACommerce.Client.Categories/ACommerce.Client.Categories.csproj", "libs/frontend/clients/ACommerce.Client.Categories/"]
COPY ["libs/frontend/clients/ACommerce.Client.Profiles/ACommerce.Client.Profiles.csproj", "libs/frontend/clients/ACommerce.Client.Profiles/"]
COPY ["libs/frontend/clients/ACommerce.Client.Payments/ACommerce.Client.Payments.csproj", "libs/frontend/clients/ACommerce.Client.Payments/"]
COPY ["libs/frontend/clients/ACommerce.Client.Chats/ACommerce.Client.Chats.csproj", "libs/frontend/clients/ACommerce.Client.Chats/"]
COPY ["libs/frontend/clients/ACommerce.Client.ContactPoints/ACommerce.Client.ContactPoints.csproj", "libs/frontend/clients/ACommerce.Client.ContactPoints/"]
COPY ["libs/frontend/clients/ACommerce.Client.Files/ACommerce.Client.Files.csproj", "libs/frontend/clients/ACommerce.Client.Files/"]
COPY ["libs/frontend/clients/ACommerce.Client.Locations/ACommerce.Client.Locations.csproj", "libs/frontend/clients/ACommerce.Client.Locations/"]
COPY ["libs/frontend/clients/ACommerce.Client.Nafath/ACommerce.Client.Nafath.csproj", "libs/frontend/clients/ACommerce.Client.Nafath/"]
COPY ["libs/frontend/clients/ACommerce.Client.Notifications/ACommerce.Client.Notifications.csproj", "libs/frontend/clients/ACommerce.Client.Notifications/"]
COPY ["libs/frontend/clients/ACommerce.Client.ProductListings/ACommerce.Client.ProductListings.csproj", "libs/frontend/clients/ACommerce.Client.ProductListings/"]
COPY ["libs/frontend/clients/ACommerce.Client.Realtime/ACommerce.Client.Realtime.csproj", "libs/frontend/clients/ACommerce.Client.Realtime/"]
COPY ["libs/frontend/clients/ACommerce.Client.Shipping/ACommerce.Client.Shipping.csproj", "libs/frontend/clients/ACommerce.Client.Shipping/"]
COPY ["libs/frontend/clients/ACommerce.Client.Subscriptions/ACommerce.Client.Subscriptions.csproj", "libs/frontend/clients/ACommerce.Client.Subscriptions/"]
COPY ["libs/frontend/clients/ACommerce.Client.Vendors/ACommerce.Client.Vendors.csproj", "libs/frontend/clients/ACommerce.Client.Vendors/"]

# Other folder
COPY ["Other/ACommerce.Transactions.Core/ACommerce.Transactions.Core.csproj", "Other/ACommerce.Transactions.Core/"]
COPY ["Other/ACommerce.Transactions.Core.Api/ACommerce.Transactions.Core.Api.csproj", "Other/ACommerce.Transactions.Core.Api/"]
COPY ["Other/ACommerce.Locations/ACommerce.Locations.csproj", "Other/ACommerce.Locations/"]
COPY ["Other/ACommerce.Locations.Api/ACommerce.Locations.Api.csproj", "Other/ACommerce.Locations.Api/"]

# Modules
COPY ["Modules/ACommerce.Reviews/ACommerce.Reviews.csproj", "Modules/ACommerce.Reviews/"]
COPY ["Modules/ACommerce.Localization/ACommerce.Localization.csproj", "Modules/ACommerce.Localization/"]

# Templates
COPY ["Templates/ACommerce.Templates.Customer/ACommerce.Templates.Customer.csproj", "Templates/ACommerce.Templates.Customer/"]
COPY ["Templates/ACommerce.Blazor.Shop/ACommerce.Blazor.Shop.csproj", "Templates/ACommerce.Blazor.Shop/"]

# Restore dependencies
RUN dotnet restore "Apps/Ashare.Api/Ashare.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/Apps/Ashare.Api"
RUN dotnet publish "Ashare.Api.csproj" -c Release -o /app/publish --no-restore

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
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Ashare.Api.dll"]
