# ACommerce.Accounting.Core

Revolutionary 4-dimensional accounting system: Account + Currency + Unit + CostCenter.

## Overview

An innovative accounting system that extends traditional double-entry bookkeeping by adding quantity (units) and cost center dimensions. Perfect for businesses dealing with physical inventory, multiple currencies, and departmental accounting.

## Packages

- **ACommerce.Accounting.Core** (Domain)
- **ACommerce.Accounting.Core.Api** (Controllers)

## Revolutionary Concept

### Traditional Accounting (2D)
```
Customer Account    1000 SAR (Debit)
Revenue Account     1000 SAR (Credit)
```

### ACommerce Accounting (4D)
```
Side A: Customer Account
  - Debit:  1000 SAR   (Currency dimension)
  - Credit: 10 Units   (Unit dimension)
  - Cost Center: Sales Dept

Side B: Revenue Account
  - Credit: 1000 SAR   (Currency dimension)
  - Debit:  10 Units   (Unit dimension)
  - Cost Center: Sales Dept
```

## Key Features

🔥 **4-Dimensional Entries** - Track money AND quantities  
✅ **Chart of Accounts** - Hierarchical account structure  
✅ **Cost Centers** - Departmental accounting  
✅ **Multi-Currency** - Built-in currency support  
✅ **Multi-Unit** - Track inventory quantities  
✅ **Entry Lifecycle** - Draft → Approved → Posted → Reversed  
✅ **Audit Trail** - Complete entry history  

## Domain Entities

### ChartOfAccounts
Account hierarchy definition

**Properties:**
- Name, Code, Description
- CompanyId, BranchId, FiscalYear
- IsDefault, IsActive

### Account
Individual account

**Properties:**
- ChartOfAccountsId
- Name, Code
- Type (Asset, Liability, Equity, Revenue, Expense)
- Nature (Debit, Credit)
- ParentAccountId (for hierarchy)
- Level, IsLeaf, AllowPosting
- DefaultCostCenterId
- RequiresCostCenter

### CostCenter
Departmental cost tracking

**Properties:**
- Name, Code, Description
- ParentCostCenterId (for hierarchy)
- Level, IsActive

### AccountingEntry
The revolutionary 4D entry

**Properties:**
- Number, Type, Date, Description
- SourceDocumentId (link to transaction)
- Status (Draft, Approved, Posted, Reversed)
- FiscalYear, FiscalPeriod
- Sides (list of entry sides)

### EntrySide
One side of the entry (4 dimensions!)

**Properties:**
- AccountId, LineNumber

**Currency Dimension:**
- CurrencyId
- DebitAmount, CreditAmount
- ExchangeRate
- BaseDebitAmount, BaseCreditAmount

**Unit Dimension:**
- UnitId
- DebitQuantity, CreditQuantity

**Cost Center Dimension:**
- CostCenterId

**Additional Dimensions:**
- ProjectId, DepartmentId
- Description, Metadata

## Entry Types

- **Manual** - Created manually
- **Automatic** - From transactions
- **Opening** - Opening balances
- **Closing** - Period closing
- **Adjustment** - Corrections
- **Reversal** - Reverse existing entry

## API Endpoints

### Chart of Accounts
- `GET /api/accounting/chart-of-accounts`
- `POST /api/accounting/chart-of-accounts`
- `GET /api/accounting/chart-of-accounts/default`
- `GET /api/accounting/chart-of-accounts/by-company/{id}`
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

## Usage Example

### Create 4D Entry
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

### Entry Lifecycle
```
Draft → Approve → Post → [Reverse if needed]
```

## Why 4D Accounting?

### Traditional Problem
```
Sold 10 products for 1000 SAR
Entry only tracks: Money moved
Missing: How many products? Which department?
```

### 4D Solution
```
Entry tracks:
✓ Money: 1000 SAR moved from customer to revenue
✓ Quantity: 10 products moved from inventory to customer
✓ Department: Sales department performance
✓ Currency: Multi-currency support
```

## Use Cases

✅ **Retail** - Track money and inventory together  
✅ **Manufacturing** - Cost per unit with departmental tracking  
✅ **Distribution** - Multi-warehouse inventory accounting  
✅ **Multi-Currency** - International business  
✅ **Project Accounting** - Track costs per project/department  

## Dependencies

- ACommerce.Catalog.Units (for quantities)
- ACommerce.Catalog.Currencies (for multi-currency)
- ACommerce.Transactions.Core (for document linking)
- SharedKernel.Abstractions
- SharedKernel.CQRS

## Installation
```bash
dotnet add package ACommerce.Accounting.Core
dotnet add package ACommerce.Accounting.Core.Api
```

## License

MIT