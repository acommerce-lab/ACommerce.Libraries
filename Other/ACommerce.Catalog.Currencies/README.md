# ACommerce.Catalog.Currencies

Multi-currency support with exchange rates and conversion.

## Overview

Complete currency management system with support for multiple currencies, exchange rates, and automatic currency conversion. Built on ACommerce.Catalog.Units for shared measurement concepts.

## Packages

- **ACommerce.Catalog.Currencies** (Domain)
- **ACommerce.Catalog.Currencies.Api** (Controllers)

## Key Features

✅ **Multi-Currency** - Support unlimited currencies  
✅ **Exchange Rates** - Historical and current rates  
✅ **Base Currency** - Define your accounting currency  
✅ **Auto Conversion** - Convert amounts between currencies  
✅ **Rate History** - Track exchange rate changes  
✅ **Symbol Formatting** - Locale-aware formatting  

## Domain Entities

### Currency
Currency definition

**Properties:**
- Name, CurrencyCode (ISO 4217)
- Symbol
- DecimalPlaces
- IsBaseCurrency
- SymbolBeforeAmount
- ThousandsSeparator
- DecimalSeparator

### ExchangeRate
Exchange rate between currencies

**Properties:**
- FromCurrencyId, ToCurrencyId
- Rate
- EffectiveDate
- Source (manual, API, etc.)
- IsActive

## API Endpoints

### Currencies
- `GET /api/catalog/currencies`
- `POST /api/catalog/currencies`
- `GET /api/catalog/currencies/{id}`
- `GET /api/catalog/currencies/base`
- `GET /api/catalog/currencies/by-code/{code}`

### Exchange Rates
- `GET /api/catalog/exchange-rates`
- `POST /api/catalog/exchange-rates`
- `GET /api/catalog/exchange-rates/current`
- `GET /api/catalog/exchange-rates/by-currencies`
- `GET /api/catalog/currencies/convert` - Convert amount

## Usage Example
```csharp
// 1. Create base currency
POST /api/catalog/currencies
{
  "name": "Saudi Riyal",
  "currencyCode": "SAR",
  "symbol": "﷼",
  "isBaseCurrency": true,
  "decimalPlaces": 2
}

// 2. Create other currency
POST /api/catalog/currencies
{
  "name": "US Dollar",
  "currencyCode": "USD",
  "symbol": "$",
  "decimalPlaces": 2
}

// 3. Set exchange rate
POST /api/catalog/exchange-rates
{
  "fromCurrencyId": "sar-id",
  "toCurrencyId": "usd-id",
  "rate": 0.27,
  "effectiveDate": "2024-01-01",
  "source": "Manual"
}

// 4. Convert
GET /api/catalog/currencies/convert?amount=100&fromCode=SAR&toCode=USD
// Returns: 27.00 USD
```

## Dependencies

- ACommerce.Catalog.Units
- SharedKernel.Abstractions
- SharedKernel.CQRS

## Installation
```bash
dotnet add package ACommerce.Catalog.Currencies
dotnet add package ACommerce.Catalog.Currencies.Api
```

## License

MIT