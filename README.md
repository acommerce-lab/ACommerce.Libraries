# ACommerce.Libraries

Reusable .NET libraries for e-commerce and shared spaces platform.

## Structure
```
ACommerce.Libraries/
+-- Core/                    # Core abstractions and patterns
¦   +-- ACommerce.Abstractions
¦   +-- ACommerce.CQRS
¦   +-- ACommerce.Infrastructure.EFCore
¦
+-- Authentication/          # Authentication & Authorization
¦   +-- ACommerce.Authentication.Abstractions
¦   +-- ACommerce.Authentication.JWT
¦   +-- ACommerce.Authentication.TwoFactor.Nafath
¦
+-- Files/                   # File & Media management
¦   +-- ACommerce.Files.Abstractions
¦   +-- ACommerce.Files.Storage.Local
¦   +-- ACommerce.Files.ImageProcessing
¦
+-- AspNetCore/             # ASP.NET Core utilities
    +-- ACommerce.AspNetCore
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 / JetBrains Rider / VS Code

### Build
```bash
dotnet restore
dotnet build
```

### Pack as NuGet
```bash
dotnet pack -c Release -o ./nupkg
```

## Libraries

### Core
- **ACommerce.Abstractions**: Base entities, repositories, and query patterns
- **ACommerce.CQRS**: CQRS implementation with MediatR
- **ACommerce.Infrastructure.EFCore**: Entity Framework Core repositories

### Authentication
- **ACommerce.Authentication.Abstractions**: Authentication provider contracts
- **ACommerce.Authentication.JWT**: JWT token generation and validation
- **ACommerce.Authentication.TwoFactor.Nafath**: Saudi Nafath 2FA integration

### Files & Media
- **ACommerce.Files.Abstractions**: File provider contracts
- **ACommerce.Files.Storage.Local**: Local file storage implementation
- **ACommerce.Files.ImageProcessing**: Image processing with ImageSharp

### ASP.NET Core
- **ACommerce.AspNetCore**: Base controllers and middleware

## License

MIT License - see LICENSE file for details
