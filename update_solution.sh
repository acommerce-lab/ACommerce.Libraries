#!/bin/bash

# Create backup
cp ACommerce.Libraries.sln ACommerce.Libraries.sln.backup

# Update Core paths
sed -i 's|"Core\\ACommerce.SharedKernel.Abstractions\\|"libs\\backend\\core\\ACommerce.SharedKernel.Abstractions\\|g' ACommerce.Libraries.sln
sed -i 's|"Core\\ACommerce.SharedKernel.CQRS\\|"libs\\backend\\core\\ACommerce.SharedKernel.CQRS\\|g' ACommerce.Libraries.sln
sed -i 's|"Core\\ACommerce.SharedKernel.Infrastructure.EFCores\\|"libs\\backend\\core\\ACommerce.SharedKernel.Infrastructure.EFCores\\|g' ACommerce.Libraries.sln
sed -i 's|"Core\\ACommerce.Configuration\\|"libs\\backend\\core\\ACommerce.Configuration\\|g' ACommerce.Libraries.sln
sed -i 's|"Core\\ACommerce.Locations.Abstractions\\|"libs\\backend\\core\\ACommerce.Locations.Abstractions\\|g' ACommerce.Libraries.sln

# Update Core messaging paths
sed -i 's|"Core\\ACommerce.Realtime.Abstractions\\|"libs\\backend\\messaging\\ACommerce.Realtime.Abstractions\\|g' ACommerce.Libraries.sln
sed -i 's|"Core\\ACommerce.Notifications.Abstractions\\|"libs\\backend\\messaging\\ACommerce.Notifications.Abstractions\\|g' ACommerce.Libraries.sln
sed -i 's|"Core\\ACommerce.Chats.Abstractions\\|"libs\\backend\\messaging\\ACommerce.Chats.Abstractions\\|g' ACommerce.Libraries.sln

echo "Core paths updated"

# Update Authentication paths
sed -i 's|"Authentication\\ACommerce.|"libs\\backend\\auth\\ACommerce.|g' ACommerce.Libraries.sln

# Update Clients paths
sed -i 's|"Clients\\ACommerce.Client.Core\\|"libs\\frontend\\core\\ACommerce.Client.Core\\|g' ACommerce.Libraries.sln
sed -i 's|"Clients\\ACommerce.Client.Realtime\\|"libs\\frontend\\realtime\\ACommerce.Client.Realtime\\|g' ACommerce.Libraries.sln
sed -i 's|"Clients\\ACommerce.Client.|"libs\\frontend\\clients\\ACommerce.Client.|g' ACommerce.Libraries.sln

# Update Infrastructure paths
sed -i 's|"Infrastructure\\ACommerce.ServiceRegistry.Client\\|"libs\\frontend\\discovery\\ACommerce.ServiceRegistry.Client\\|g' ACommerce.Libraries.sln
sed -i 's|"Infrastructure\\ACommerce.|"libs\\backend\\integration\\ACommerce.|g' ACommerce.Libraries.sln

# Update AspNetCore paths
sed -i 's|"AspNetCore\\ACommerce.|"libs\\backend\\integration\\ACommerce.|g' ACommerce.Libraries.sln

# Update Files paths
sed -i 's|"Files\\ACommerce.|"libs\\backend\\files\\ACommerce.|g' ACommerce.Libraries.sln

# Update Marketplace paths
sed -i 's|"Marketplace\\ACommerce.|"libs\\backend\\marketplace\\ACommerce.|g' ACommerce.Libraries.sln

# Update Sales paths
sed -i 's|"Sales\\ACommerce.|"libs\\backend\\sales\\ACommerce.|g' ACommerce.Libraries.sln

# Update Payments paths
sed -i 's|"Payments\\ACommerce.|"libs\\backend\\sales\\ACommerce.|g' ACommerce.Libraries.sln

# Update Shipping paths
sed -i 's|"Shipping\\ACommerce.|"libs\\backend\\shipping\\ACommerce.|g' ACommerce.Libraries.sln

# Update Catalog paths
sed -i 's|"Catalog\\ACommerce.|"libs\\backend\\catalog\\ACommerce.|g' ACommerce.Libraries.sln

# Update Identity paths
sed -i 's|"Identity\\ACommerce.|"libs\\backend\\auth\\ACommerce.|g' ACommerce.Libraries.sln

echo "All paths updated"

# Update root-level Messaging paths
sed -i 's|"ACommerce.Messaging.Abstractions\\|"libs\\backend\\messaging\\ACommerce.Messaging.Abstractions\\|g' ACommerce.Libraries.sln
sed -i 's|"ACommerce.Messaging.InMemory\\|"libs\\backend\\messaging\\ACommerce.Messaging.InMemory\\|g' ACommerce.Libraries.sln
sed -i 's|"ACommerce.Messaging.SignalR\\|"libs\\backend\\messaging\\ACommerce.Messaging.SignalR\\|g' ACommerce.Libraries.sln
sed -i 's|"ACommerce.Messaging.SignalR.Hub\\|"libs\\backend\\messaging\\ACommerce.Messaging.SignalR.Hub\\|g' ACommerce.Libraries.sln
sed -i 's|"ACommerce.Authentication.Messaging\\|"libs\\backend\\messaging\\ACommerce.Authentication.Messaging\\|g' ACommerce.Libraries.sln
sed -i 's|"ACommerce.Notifications.Messaging\\|"libs\\backend\\messaging\\ACommerce.Notifications.Messaging\\|g' ACommerce.Libraries.sln

# Update root-level Profiles paths
sed -i 's|"ACommerce.Profiles\\|"libs\\backend\\auth\\ACommerce.Profiles\\|g' ACommerce.Libraries.sln
sed -i 's|"ACommerce.Profiles.Api\\|"libs\\backend\\auth\\ACommerce.Profiles.Api\\|g' ACommerce.Libraries.sln
sed -i 's|"ACommerce.Profiles.Core\\|"libs\\backend\\auth\\ACommerce.Profiles.Core\\|g' ACommerce.Libraries.sln
sed -i 's|"ACommerce.Profiles.Messaging\\|"libs\\backend\\auth\\ACommerce.Profiles.Messaging\\|g' ACommerce.Libraries.sln

# Update TwoFactor SessionStore paths
sed -i 's|"ACommerce.Authentication.TwoFactor.SessionStore.InMemory\\|"libs\\backend\\auth\\ACommerce.Authentication.TwoFactor.SessionStore.InMemory\\|g' ACommerce.Libraries.sln
sed -i 's|"ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework\\|"libs\\backend\\auth\\ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework\\|g' ACommerce.Libraries.sln

# Update Other folder paths
sed -i 's|"Other\\ACommerce.Catalog.|"libs\\backend\\catalog\\ACommerce.Catalog.|g' ACommerce.Libraries.sln
sed -i 's|"Other\\ACommerce.Chats.|"libs\\backend\\messaging\\ACommerce.Chats.|g' ACommerce.Libraries.sln
sed -i 's|"Other\\ACommerce.Realtime.|"libs\\backend\\messaging\\ACommerce.Realtime.|g' ACommerce.Libraries.sln
sed -i 's|"Other\\ACommerce.Notifications.|"libs\\backend\\messaging\\ACommerce.Notifications.|g' ACommerce.Libraries.sln
sed -i 's|"Other\\ACommerce.Accounting.|"libs\\backend\\other\\ACommerce.Accounting.|g' ACommerce.Libraries.sln
sed -i 's|"Other\\ACommerce.Transactions.|"libs\\backend\\other\\ACommerce.Transactions.|g' ACommerce.Libraries.sln

echo "Root paths updated"
