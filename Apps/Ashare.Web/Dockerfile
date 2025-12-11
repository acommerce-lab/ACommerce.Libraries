FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Apps/Ashare.Web/Ashare.Web.csproj", "Apps/Ashare.Web/"]
COPY ["Apps/Ashare.Shared/Ashare.Shared.csproj", "Apps/Ashare.Shared/"]
COPY ["Clients/ACommerce.Client.Auth/ACommerce.Client.Auth.csproj", "Clients/ACommerce.Client.Auth/"]
COPY ["Clients/ACommerce.Client.Core/ACommerce.Client.Core.csproj", "Clients/ACommerce.Client.Core/"]
COPY ["Clients/ACommerce.Client.Profiles/ACommerce.Client.Profiles.csproj", "Clients/ACommerce.Client.Profiles/"]
COPY ["Clients/ACommerce.Client.Products/ACommerce.Client.Products.csproj", "Clients/ACommerce.Client.Products/"]
COPY ["Clients/ACommerce.Client.ProductListings/ACommerce.Client.ProductListings.csproj", "Clients/ACommerce.Client.ProductListings/"]
COPY ["Clients/ACommerce.Client.Categories/ACommerce.Client.Categories.csproj", "Clients/ACommerce.Client.Categories/"]
COPY ["Clients/ACommerce.Client.Orders/ACommerce.Client.Orders.csproj", "Clients/ACommerce.Client.Orders/"]
COPY ["Clients/ACommerce.Client.Cart/ACommerce.Client.Cart.csproj", "Clients/ACommerce.Client.Cart/"]
COPY ["Clients/ACommerce.Client.Chats/ACommerce.Client.Chats.csproj", "Clients/ACommerce.Client.Chats/"]
COPY ["Clients/ACommerce.Client.Vendors/ACommerce.Client.Vendors.csproj", "Clients/ACommerce.Client.Vendors/"]
COPY ["Clients/ACommerce.Client.ContactPoints/ACommerce.Client.ContactPoints.csproj", "Clients/ACommerce.Client.ContactPoints/"]
COPY ["Clients/ACommerce.Client.Locations/ACommerce.Client.Locations.csproj", "Clients/ACommerce.Client.Locations/"]
COPY ["Clients/ACommerce.Client.Notifications/ACommerce.Client.Notifications.csproj", "Clients/ACommerce.Client.Notifications/"]
COPY ["Clients/ACommerce.Client.Payments/ACommerce.Client.Payments.csproj", "Clients/ACommerce.Client.Payments/"]
COPY ["Clients/ACommerce.Client.Subscriptions/ACommerce.Client.Subscriptions.csproj", "Clients/ACommerce.Client.Subscriptions/"]
COPY ["Locations/ACommerce.Locations.Abstractions/ACommerce.Locations.Abstractions.csproj", "Locations/ACommerce.Locations.Abstractions/"]

RUN dotnet restore "Apps/Ashare.Web/Ashare.Web.csproj"

COPY . .

WORKDIR "/src/Apps/Ashare.Web"
RUN dotnet build "Ashare.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ashare.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ashare.Web.dll"]
