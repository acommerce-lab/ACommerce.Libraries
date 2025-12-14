# ACommerce.Accounting.Core.Api

ASP.NET Core controllers for revolutionary 4-dimensional accounting system.

## Features

🔥 **4-Dimensional Entries** - Account + Currency + Unit + CostCenter  
✅ **Chart of Accounts** - Multi-level hierarchical structure  
✅ **Cost Centers** - Multi-level hierarchical structure  
✅ **Entry Management** - Draft, Approve, Post, Reverse  
✅ **Multi-Currency** - Built-in currency support  
✅ **Multi-Unit** - Built-in unit support  

## Installation
```bash
dotnet add package ACommerce.Accounting.Core.Api
```

## Usage

### In your Web API Program.cs:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(AccountingEntriesController).Assembly);

var app = builder.Build();

app.MapControllers();
app.Run();
```

## Available Endpoints

### Chart of Accounts
- `GET /api/accounting/chart-of-accounts`
- `POST /api/accounting/chart-of-accounts`
- `GET /api/accounting/chart-of-accounts/default`
- `GET /api/accounting/chart-of-accounts/{id}/root-accounts`

### Accounts
- `GET /api/accounting/accounts`
- `POST /api/accounting/accounts`
- `GET /api/accounting/accounts/by-code/{code}`
- `GET /api/accounting/accounts/by-type/{type}`
- `GET /api/accounting/accounts/{id}/sub-accounts`
- `GET /api/accounting/accounts/leaf`

### Cost Centers
- `GET /api/accounting/cost-centers`
- `POST /api/accounting/cost-centers`
- `GET /api/accounting/cost-centers/by-code/{code}`
- `GET /api/accounting/cost-centers/root`
- `GET /api/accounting/cost-centers/{id}/sub-centers`

### Accounting Entries (The Heart! 💰)
- `GET /api/accounting/entries`
- `POST /api/accounting/entries`
- `GET /api/accounting/entries/by-number/{number}`
- `GET /api/accounting/entries/by-status/{status}`
- `GET /api/accounting/entries/by-period?fiscalYear=2024&fiscalPeriod=12`
- `POST /api/accounting/entries/{id}/approve`
- `POST /api/accounting/entries/{id}/post`
- `POST /api/accounting/entries/{id}/reverse`

## Example: Creating 4D Entry
```json
POST /api/accounting/entries
{
  "number": "JV-2024-001",
  "type": "Manual",
  "date": "2024-01-15",
  "description": "Sale of 10 units of Product X",
  "fiscalYear": 2024,
  "fiscalPeriod": 1,
  "sides": [
    {
      "lineNumber": 1,
      "accountId": "customer-account-guid",
      "debitAmount": 1000,
      "currencyId": "sar-currency-guid",
      "creditQuantity": 10,
      "unitId": "piece-unit-guid",
      "costCenterId": "sales-dept-guid"
    },
    {
      "lineNumber": 2,
      "accountId": "revenue-account-guid",
      "creditAmount": 1000,
      "currencyId": "sar-currency-guid",
      "debitQuantity": 10,
      "unitId": "piece-unit-guid",
      "costCenterId": "sales-dept-guid"
    }
  ]
}
```

## License

MIT