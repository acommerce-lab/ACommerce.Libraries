# ACommerce.Catalog.Currencies.Api

ASP.NET Core controllers for ACommerce currency management system.

## Features

✅ **Currencies Controller** - Manage currencies with ISO 4217 codes  
✅ **Exchange Rates Controller** - Manage historical exchange rates  
✅ **Currency Conversions Controller** - Convert between currencies  
✅ **Extends Units System** - Currencies are specialized units  
✅ **Automatic Formatting** - Format amounts by currency rules  
✅ **Rate History** - Track exchange rate changes over time  

## Installation
```bash
dotnet add package ACommerce.Catalog.Currencies.Api
```

## Usage

### In your Web API Program.cs:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Currency Conversion Service
builder.Services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();

// Add Controllers
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CurrenciesController).Assembly);

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.MapControllers();

app.Run();
```

## Available Endpoints

### Currencies
- `GET /api/catalog/currencies/{id}`
- `POST /api/catalog/currencies`
- `GET /api/catalog/currencies/by-code/{code}`
- `GET /api/catalog/currencies/base`
- `GET /api/catalog/currencies/active`
- `GET /api/catalog/currencies/by-country/{country}`
- `POST /api/catalog/currencies/{id}/set-as-base`
- `POST /api/catalog/currencies/{id}/format`

### Exchange Rates
- `GET /api/catalog/exchange-rates/{id}`
- `POST /api/catalog/exchange-rates`
- `GET /api/catalog/exchange-rates/current`
- `GET /api/catalog/exchange-rates/history`
- `GET /api/catalog/exchange-rates/from-currency/{currencyId}`
- `POST /api/catalog/exchange-rates/update-rate`

### Conversions
- `POST /api/catalog/currency-conversions/convert`
- `GET /api/catalog/currency-conversions/rate`
- `POST /api/catalog/currency-conversions/batch-convert`

## Example: Convert Currency
```bash
POST /api/catalog/currency-conversions/convert
{
  "amount": 1000,
  "fromCurrencyId": "sar-guid",
  "toCurrencyId": "usd-guid",
  "date": "2025-11-07T00:00:00Z",
  "rateType": "official"
}

Response:
{
  "originalAmount": 1000,
  "fromCurrencyCode": "SAR",
  "fromCurrencySymbol": "﷼",
  "formattedOriginalAmount": "1,000.00 ﷼",
  "convertedAmount": 266.67,
  "toCurrencyCode": "USD",
  "toCurrencySymbol": "$",
  "formattedConvertedAmount": "$266.67",
  "exchangeRate": 0.2667,
  "rateDate": "2025-11-07T00:00:00Z",
  "rateSource": "Saudi Central Bank"
}
```

## License

MIT