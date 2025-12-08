#!/bin/bash
set -e

echo "Building Ashare.Api..."
dotnet publish Apps/Ashare.Api/Ashare.Api.csproj -c Release -o publish/api --self-contained true -r linux-x64

echo "Building Ashare.Web..."
dotnet publish Apps/Ashare.Web/Ashare.Web.csproj -c Release -o publish/web --self-contained true -r linux-x64

echo "Build completed successfully!"
