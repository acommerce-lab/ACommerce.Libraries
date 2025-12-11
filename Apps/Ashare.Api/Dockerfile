# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY ["ACommerce.Libraries.sln", "./"]

# Copy ALL project files for restore (using find pattern)
COPY ["Apps/Ashare.Api/Ashare.Api.csproj", "Apps/Ashare.Api/"]
COPY ["Apps/Ashare.Shared/Ashare.Shared.csproj", "Apps/Ashare.Shared/"]

# Core
COPY ["Core/ACommerce.SharedKernel.Abstractions/ACommerce.SharedKernel.Abstractions.csproj", "Core/ACommerce.SharedKernel.Abstractions/"]
COPY ["Core/ACommerce.SharedKernel.CQRS/ACommerce.SharedKernel.CQRS.csproj", "Core/ACommerce.SharedKernel.CQRS/"]
COPY ["Core/ACommerce.SharedKernel.Infrastructure.EFCores/ACommerce.SharedKernel.Infrastructure.EFCores.csproj", "Core/ACommerce.SharedKernel.Infrastructure.EFCores/"]
COPY ["Core/ACommerce.Chats.Abstractions/ACommerce.Chats.Abstractions.csproj", "Core/ACommerce.Chats.Abstractions/"]
COPY ["Core/ACommerce.Configuration/ACommerce.Configuration.csproj", "Core/ACommerce.Configuration/"]
COPY ["Core/ACommerce.Locations.Abstractions/ACommerce.Locations.Abstractions.csproj", "Core/ACommerce.Locations.Abstractions/"]
COPY ["Core/ACommerce.Notifications.Abstractions/ACommerce.Notifications.Abstractions.csproj", "Core/ACommerce.Notifications.Abstractions/"]
COPY ["Core/ACommerce.Realtime.Abstractions/ACommerce.Realtime.Abstractions.csproj", "Core/ACommerce.Realtime.Abstractions/"]

# AspNetCore
COPY ["AspNetCore/ACommerce.SharedKernel.AspNetCore/ACommerce.SharedKernel.AspNetCore.csproj", "AspNetCore/ACommerce.SharedKernel.AspNetCore/"]
COPY ["AspNetCore/ACommerce.Authentication.AspNetCore/ACommerce.Authentication.AspNetCore.csproj", "AspNetCore/ACommerce.Authentication.AspNetCore/"]
COPY ["AspNetCore/ACommerce.Authentication.AspNetCore.Swagger/ACommerce.Authentication.AspNetCore.Swagger.csproj", "AspNetCore/ACommerce.Authentication.AspNetCore.Swagger/"]
COPY ["AspNetCore/ACommerce.Authentication.AspNetCore.NafathWH/ACommerce.Authentication.AspNetCore.NafathWH.csproj", "AspNetCore/ACommerce.Authentication.AspNetCore.NafathWH/"]
COPY ["AspNetCore/ACommerce.Files.AspNetCore/ACommerce.Files.AspNetCore.csproj", "AspNetCore/ACommerce.Files.AspNetCore/"]

# Authentication
COPY ["Authentication/ACommerce.Authentication.Abstractions/ACommerce.Authentication.Abstractions.csproj", "Authentication/ACommerce.Authentication.Abstractions/"]
COPY ["Authentication/ACommerce.Authentication.JWT/ACommerce.Authentication.JWT.csproj", "Authentication/ACommerce.Authentication.JWT/"]
COPY ["Authentication/ACommerce.Authentication.MicrosoftIdentity/ACommerce.Authentication.MicrosoftIdentity.csproj", "Authentication/ACommerce.Authentication.MicrosoftIdentity/"]
COPY ["Authentication/ACommerce.Authentication.OpenIddict/ACommerce.Authentication.OpenIddict.csproj", "Authentication/ACommerce.Authentication.OpenIddict/"]
COPY ["Authentication/ACommerce.Authentication.TwoFactor.Abstractions/ACommerce.Authentication.TwoFactor.Abstractions.csproj", "Authentication/ACommerce.Authentication.TwoFactor.Abstractions/"]
COPY ["Authentication/ACommerce.Authentication.TwoFactor.Email/ACommerce.Authentication.TwoFactor.Email.csproj", "Authentication/ACommerce.Authentication.TwoFactor.Email/"]
COPY ["Authentication/ACommerce.Authentication.TwoFactor.Nafath/ACommerce.Authentication.TwoFactor.Nafath.csproj", "Authentication/ACommerce.Authentication.TwoFactor.Nafath/"]
COPY ["Authentication/ACommerce.Authentication.TwoFactor.SMS/ACommerce.Authentication.TwoFactor.SMS.csproj", "Authentication/ACommerce.Authentication.TwoFactor.SMS/"]
COPY ["Authentication/ACommerce.Authentication.Users.Abstractions/ACommerce.Authentication.Users.Abstractions.csproj", "Authentication/ACommerce.Authentication.Users.Abstractions/"]
COPY ["ACommerce.Authentication.TwoFactor.SessionStore.InMemory/ACommerce.Authentication.TwoFactor.SessionStore.InMemory.csproj", "ACommerce.Authentication.TwoFactor.SessionStore.InMemory/"]
COPY ["ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework/ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework.csproj", "ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework/"]
COPY ["ACommerce.Authentication.Messaging/ACommerce.Authentication.Messaging.csproj", "ACommerce.Authentication.Messaging/"]

# Identity
COPY ["Identity/ACommerce.Profiles/ACommerce.Profiles.csproj", "Identity/ACommerce.Profiles/"]
COPY ["Identity/ACommerce.Profiles.Api/ACommerce.Profiles.Api.csproj", "Identity/ACommerce.Profiles.Api/"]
COPY ["ACommerce.Profiles.Core/ACommerce.Profiles.Core.csproj", "ACommerce.Profiles.Core/"]
COPY ["ACommerce.Profiles.Messaging/ACommerce.Profiles.Messaging.csproj", "ACommerce.Profiles.Messaging/"]

# Catalog
COPY ["Catalog/ACommerce.Catalog.Listings/ACommerce.Catalog.Listings.csproj", "Catalog/ACommerce.Catalog.Listings/"]
COPY ["Catalog/ACommerce.Catalog.Listings.Api/ACommerce.Catalog.Listings.Api.csproj", "Catalog/ACommerce.Catalog.Listings.Api/"]

# Marketplace
COPY ["Marketplace/ACommerce.Vendors/ACommerce.Vendors.csproj", "Marketplace/ACommerce.Vendors/"]
COPY ["Marketplace/ACommerce.Vendors.Api/ACommerce.Vendors.Api.csproj", "Marketplace/ACommerce.Vendors.Api/"]
COPY ["Marketplace/ACommerce.Subscriptions/ACommerce.Subscriptions.csproj", "Marketplace/ACommerce.Subscriptions/"]
COPY ["Marketplace/ACommerce.Subscriptions.Api/ACommerce.Subscriptions.Api.csproj", "Marketplace/ACommerce.Subscriptions.Api/"]

# Sales
COPY ["Sales/ACommerce.Cart/ACommerce.Cart.csproj", "Sales/ACommerce.Cart/"]
COPY ["Sales/ACommerce.Orders/ACommerce.Orders.csproj", "Sales/ACommerce.Orders/"]
COPY ["Sales/ACommerce.Orders.Api/ACommerce.Orders.Api.csproj", "Sales/ACommerce.Orders.Api/"]

# Payments
COPY ["Payments/ACommerce.Payments.Abstractions/ACommerce.Payments.Abstractions.csproj", "Payments/ACommerce.Payments.Abstractions/"]
COPY ["Payments/ACommerce.Payments.Api/ACommerce.Payments.Api.csproj", "Payments/ACommerce.Payments.Api/"]
COPY ["Payments/ACommerce.Payments.Moyasar/ACommerce.Payments.Moyasar.csproj", "Payments/ACommerce.Payments.Moyasar/"]
COPY ["Payments/ACommerce.Payments.Noon/ACommerce.Payments.Noon.csproj", "Payments/ACommerce.Payments.Noon/"]

# Shipping
COPY ["Shipping/ACommerce.Shipping.Abstractions/ACommerce.Shipping.Abstractions.csproj", "Shipping/ACommerce.Shipping.Abstractions/"]
COPY ["Shipping/ACommerce.Shipping.Api/ACommerce.Shipping.Api.csproj", "Shipping/ACommerce.Shipping.Api/"]
COPY ["Shipping/ACommerce.Shipping.Mock/ACommerce.Shipping.Mock.csproj", "Shipping/ACommerce.Shipping.Mock/"]

# Infrastructure
COPY ["Infrastructure/ACommerce.ServiceRegistry.Abstractions/ACommerce.ServiceRegistry.Abstractions.csproj", "Infrastructure/ACommerce.ServiceRegistry.Abstractions/"]
COPY ["Infrastructure/ACommerce.ServiceRegistry.Client/ACommerce.ServiceRegistry.Client.csproj", "Infrastructure/ACommerce.ServiceRegistry.Client/"]
COPY ["Infrastructure/ACommerce.ServiceRegistry.Core/ACommerce.ServiceRegistry.Core.csproj", "Infrastructure/ACommerce.ServiceRegistry.Core/"]
COPY ["Infrastructure/ACommerce.ServiceRegistry.Server/ACommerce.ServiceRegistry.Server.csproj", "Infrastructure/ACommerce.ServiceRegistry.Server/"]

# Clients
COPY ["Clients/ACommerce.Client.Auth/ACommerce.Client.Auth.csproj", "Clients/ACommerce.Client.Auth/"]
COPY ["Clients/ACommerce.Client.Cart/ACommerce.Client.Cart.csproj", "Clients/ACommerce.Client.Cart/"]
COPY ["Clients/ACommerce.Client.Categories/ACommerce.Client.Categories.csproj", "Clients/ACommerce.Client.Categories/"]
COPY ["Clients/ACommerce.Client.Chats/ACommerce.Client.Chats.csproj", "Clients/ACommerce.Client.Chats/"]
COPY ["Clients/ACommerce.Client.ContactPoints/ACommerce.Client.ContactPoints.csproj", "Clients/ACommerce.Client.ContactPoints/"]
COPY ["Clients/ACommerce.Client.Core/ACommerce.Client.Core.csproj", "Clients/ACommerce.Client.Core/"]
COPY ["Clients/ACommerce.Client.Files/ACommerce.Client.Files.csproj", "Clients/ACommerce.Client.Files/"]
COPY ["Clients/ACommerce.Client.Locations/ACommerce.Client.Locations.csproj", "Clients/ACommerce.Client.Locations/"]
COPY ["Clients/ACommerce.Client.Nafath/ACommerce.Client.Nafath.csproj", "Clients/ACommerce.Client.Nafath/"]
COPY ["Clients/ACommerce.Client.Notifications/ACommerce.Client.Notifications.csproj", "Clients/ACommerce.Client.Notifications/"]
COPY ["Clients/ACommerce.Client.Orders/ACommerce.Client.Orders.csproj", "Clients/ACommerce.Client.Orders/"]
COPY ["Clients/ACommerce.Client.Payments/ACommerce.Client.Payments.csproj", "Clients/ACommerce.Client.Payments/"]
COPY ["Clients/ACommerce.Client.ProductListings/ACommerce.Client.ProductListings.csproj", "Clients/ACommerce.Client.ProductListings/"]
COPY ["Clients/ACommerce.Client.Products/ACommerce.Client.Products.csproj", "Clients/ACommerce.Client.Products/"]
COPY ["Clients/ACommerce.Client.Profiles/ACommerce.Client.Profiles.csproj", "Clients/ACommerce.Client.Profiles/"]
COPY ["Clients/ACommerce.Client.Realtime/ACommerce.Client.Realtime.csproj", "Clients/ACommerce.Client.Realtime/"]
COPY ["Clients/ACommerce.Client.Shipping/ACommerce.Client.Shipping.csproj", "Clients/ACommerce.Client.Shipping/"]
COPY ["Clients/ACommerce.Client.Subscriptions/ACommerce.Client.Subscriptions.csproj", "Clients/ACommerce.Client.Subscriptions/"]
COPY ["Clients/ACommerce.Client.Vendors/ACommerce.Client.Vendors.csproj", "Clients/ACommerce.Client.Vendors/"]

# Other modules
COPY ["Other/ACommerce.Accounting.Core/ACommerce.Accounting.Core.csproj", "Other/ACommerce.Accounting.Core/"]
COPY ["Other/ACommerce.Accounting.Core.Api/ACommerce.Accounting.Core.Api.csproj", "Other/ACommerce.Accounting.Core.Api/"]
COPY ["Other/ACommerce.Catalog.Attributes/ACommerce.Catalog.Attributes.csproj", "Other/ACommerce.Catalog.Attributes/"]
COPY ["Other/ACommerce.Catalog.Attributes.Api/ACommerce.Catalog.Attributes.Api.csproj", "Other/ACommerce.Catalog.Attributes.Api/"]
COPY ["Other/ACommerce.Catalog.Products/ACommerce.Catalog.Products.csproj", "Other/ACommerce.Catalog.Products/"]
COPY ["Other/ACommerce.Catalog.Products.Api/ACommerce.Catalog.Products.Api.csproj", "Other/ACommerce.Catalog.Products.Api/"]
COPY ["Other/ACommerce.Catalog.Simple.Api/ACommerce.Catalog.Simple.Api.csproj", "Other/ACommerce.Catalog.Simple.Api/"]
COPY ["Other/ACommerce.Catalog.Units/ACommerce.Catalog.Units.csproj", "Other/ACommerce.Catalog.Units/"]
COPY ["Other/ACommerce.Catalog.Units.Api/ACommerce.Catalog.Units.Api.csproj", "Other/ACommerce.Catalog.Units.Api/"]
COPY ["Other/ACommerce.Catalog.Currencies/ACommerce.Catalog.Currencies.csproj", "Other/ACommerce.Catalog.Currencies/"]
COPY ["Other/ACommerce.Catalog.Currencies.Api/ACommerce.Catalog.Currencies.Api.csproj", "Other/ACommerce.Catalog.Currencies.Api/"]
COPY ["Other/ACommerce.Transactions.Core/ACommerce.Transactions.Core.csproj", "Other/ACommerce.Transactions.Core/"]
COPY ["Other/ACommerce.Transactions.Core.Api/ACommerce.Transactions.Core.Api.csproj", "Other/ACommerce.Transactions.Core.Api/"]
COPY ["Other/ACommerce.Locations/ACommerce.Locations.csproj", "Other/ACommerce.Locations/"]
COPY ["Other/ACommerce.Locations.Api/ACommerce.Locations.Api.csproj", "Other/ACommerce.Locations.Api/"]
COPY ["Other/ACommerce.Notifications.Core/ACommerce.Notifications.Core.csproj", "Other/ACommerce.Notifications.Core/"]
COPY ["Other/ACommerce.Notifications.Channels.Email/ACommerce.Notifications.Channels.Email.csproj", "Other/ACommerce.Notifications.Channels.Email/"]
COPY ["Other/ACommerce.Notifications.Channels.Firebase/ACommerce.Notifications.Channels.Firebase.csproj", "Other/ACommerce.Notifications.Channels.Firebase/"]
COPY ["Other/ACommerce.Notifications.Channels.InApp/ACommerce.Notifications.Channels.InApp.csproj", "Other/ACommerce.Notifications.Channels.InApp/"]
COPY ["Other/ACommerce.Notifications.Recipients/ACommerce.Notifications.Recipients.csproj", "Other/ACommerce.Notifications.Recipients/"]
COPY ["Other/ACommerce.Notifications.Recipients.Api/ACommerce.Notifications.Recipients.Api.csproj", "Other/ACommerce.Notifications.Recipients.Api/"]
COPY ["Other/ACommerce.Chats.Core/ACommerce.Chats.Core.csproj", "Other/ACommerce.Chats.Core/"]
COPY ["Other/ACommerce.Chats.Api/ACommerce.Chats.Api.csproj", "Other/ACommerce.Chats.Api/"]
COPY ["Other/ACommerce.Realtime.SignalR/ACommerce.Realtime.SignalR.csproj", "Other/ACommerce.Realtime.SignalR/"]

# Modules
COPY ["Modules/ACommerce.Reviews/ACommerce.Reviews.csproj", "Modules/ACommerce.Reviews/"]
COPY ["Modules/ACommerce.Localization/ACommerce.Localization.csproj", "Modules/ACommerce.Localization/"]

# Files
COPY ["Files/ACommerce.Files.Abstractions/ACommerce.Files.Abstractions.csproj", "Files/ACommerce.Files.Abstractions/"]
COPY ["Files/ACommerce.Files.Storage.Local/ACommerce.Files.Storage.Local.csproj", "Files/ACommerce.Files.Storage.Local/"]
COPY ["Files/ACommerce.Files.Storage.AliyunOSS/ACommerce.Files.Storage.AliyunOSS.csproj", "Files/ACommerce.Files.Storage.AliyunOSS/"]
COPY ["Files/ACommerce.Files.Storage.GoogleCloud/ACommerce.Files.Storage.GoogleCloud.csproj", "Files/ACommerce.Files.Storage.GoogleCloud/"]

# Templates
COPY ["Templates/ACommerce.Templates.Customer/ACommerce.Templates.Customer.csproj", "Templates/ACommerce.Templates.Customer/"]
COPY ["Templates/ACommerce.Blazor.Shop/ACommerce.Blazor.Shop.csproj", "Templates/ACommerce.Blazor.Shop/"]

# Messaging
COPY ["ACommerce.Messaging.Abstractions/ACommerce.Messaging.Abstractions.csproj", "ACommerce.Messaging.Abstractions/"]
COPY ["ACommerce.Messaging.InMemory/ACommerce.Messaging.InMemory.csproj", "ACommerce.Messaging.InMemory/"]
COPY ["ACommerce.Messaging.SignalR/ACommerce.Messaging.SignalR.csproj", "ACommerce.Messaging.SignalR/"]
COPY ["ACommerce.Messaging.SignalR.Hub/ACommerce.Messaging.SignalR.Hub.csproj", "ACommerce.Messaging.SignalR.Hub/"]
COPY ["ACommerce.Notifications.Messaging/ACommerce.Notifications.Messaging.csproj", "ACommerce.Notifications.Messaging/"]

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
