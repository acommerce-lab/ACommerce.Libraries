# ACommerce.Transactions.Core

Core transaction and document management system with operations and notifications.

## Overview

Enterprise transaction management system that allows admins to configure document types, operations, and notifications. Provides the foundation for building complex business workflows with document lifecycle management.

## Packages

- **ACommerce.Transactions.Core** (Domain)
- **ACommerce.Transactions.Core.Api** (Controllers)

## Key Features

✅ **Document Types** - Invoice, Receipt, Order, Quote, etc.  
✅ **Operations** - Create, Edit, Approve, Post, Cancel, etc.  
✅ **Notifications** - Email, SMS, InApp, Push per operation  
✅ **Document Relations** - Sequential and Compositional  
✅ **Workflow Engine** - Condition-based operations  
✅ **Custom Attributes** - Extend documents dynamically  
✅ **Audit Trail** - Track all document changes  

## Domain Entities

### DocumentType
Define document types

**Properties:**
- Name, Code, Category
- Description, Icon, ColorHex
- NumberPrefix, NumberLength
- RequiresApproval
- AffectsInventory, AffectsAccounting
- IsActive, SortOrder

**Categories:**
- Sales, Purchases, Inventory
- Financial, HumanResources
- Operations, General

### DocumentOperation
Operations available on documents

**Properties:**
- DocumentTypeId
- Operation (Create, Edit, Delete, Approve, Post, etc.)
- CustomName, Description
- RequiresApproval
- AllowedRoles, ApprovalRoles
- Conditions (JSON)
- IsActive, SortOrder

### OperationNotification
Notifications per operation

**Properties:**
- OperationId
- Type (Email, SMS, InApp, Push, WhatsApp, Webhook)
- Template, Subject
- Recipients, CcRecipients
- DelayMinutes, Priority
- IsActive

### DocumentTypeRelation
Relations between document types

**Properties:**
- SourceDocumentTypeId
- TargetDocumentTypeId
- RelationType (Sequential, Compositional, Reference, Amendment)
- IsRequired, AllowMultiple
- Conditions, Priority

### DocumentTypeAttribute
Custom attributes per document type

**Properties:**
- DocumentTypeId
- AttributeDefinitionId
- IsRequired, SortOrder

## Relation Types

### Sequential
Document flows (Order → Invoice → Payment)
```
Sales Order → Invoice → Receipt
Purchase Request → Purchase Order → Receipt
```

### Compositional
Document composition (Multiple quotes → Single order)
```
Quote A ┐
Quote B ├─→ Purchase Order
Quote C ┘
```

### Reference
Document linking (Return → Original invoice)
```
Sales Invoice ← Return Invoice
```

## API Endpoints

### Document Types
- `GET /api/transactions/document-types`
- `POST /api/transactions/document-types`
- `GET /api/transactions/document-types/{id}`
- `GET /api/transactions/document-types/by-code/{code}`
- `GET /api/transactions/document-types/by-category/{category}`
- `GET /api/transactions/document-types/inventory-affecting`
- `GET /api/transactions/document-types/accounting-affecting`
- `GET /api/transactions/document-types/{id}/operations`
- `GET /api/transactions/document-types/{id}/attributes`
- `GET /api/transactions/document-types/{id}/next-types`

### Document Operations
- `GET /api/transactions/document-operations`
- `POST /api/transactions/document-operations`
- `GET /api/transactions/document-operations/by-document-type/{id}`
- `GET /api/transactions/document-operations/by-operation-type/{type}`
- `GET /api/transactions/document-operations/requiring-approval`
- `GET /api/transactions/document-operations/{id}/notifications`

### Operation Notifications
- `GET /api/transactions/operation-notifications`
- `POST /api/transactions/operation-notifications`
- `GET /api/transactions/operation-notifications/by-type/{type}`
- `GET /api/transactions/operation-notifications/by-operation/{id}`

### Document Type Relations
- `GET /api/transactions/document-type-relations`
- `POST /api/transactions/document-type-relations`
- `GET /api/transactions/document-type-relations/by-source/{id}`
- `GET /api/transactions/document-type-relations/by-target/{id}`
- `GET /api/transactions/document-type-relations/by-relation-type/{type}`
- `GET /api/transactions/document-type-relations/required`

## Usage Example
```csharp
// 1. Create document type
POST /api/transactions/document-types
{
  "name": "Sales Invoice",
  "code": "sales_invoice",
  "category": "Sales",
  "numberPrefix": "INV",
  "requiresApproval": true,
  "affectsInventory": true,
  "affectsAccounting": true
}

// 2. Add operations
POST /api/transactions/document-operations
{
  "documentTypeId": "doc-type-id",
  "operation": "Approve",
  "requiresApproval": true,
  "allowedRoles": ["manager", "admin"],
  "approvalRoles": ["finance_manager"]
}

// 3. Add notification
POST /api/transactions/operation-notifications
{
  "operationId": "operation-id",
  "type": "Email",
  "template": "Invoice {DocumentNumber} has been approved",
  "subject": "Invoice Approved",
  "recipients": ["creator", "finance_team"]
}

// 4. Create relation
POST /api/transactions/document-type-relations
{
  "sourceDocumentTypeId": "sales-order-id",
  "targetDocumentTypeId": "sales-invoice-id",
  "relationType": "Sequential",
  "isRequired": true,
  "allowMultiple": true
}
```

## Workflow Examples

### Sales Flow
```
Quote → Sales Order → Invoice → Receipt → Shipment
```

### Purchase Flow
```
Purchase Request → Purchase Order → Receipt → Invoice → Payment
```

### Return Flow
```
Sales Invoice ← Return Request ← Return Invoice ← Refund
```

## Dependencies

- ACommerce.Catalog.Attributes (for custom attributes)
- ACommerce.Catalog.Units (for quantities)
- ACommerce.Catalog.Currencies (for amounts)
- SharedKernel.Abstractions
- SharedKernel.CQRS

## Installation
```bash
dotnet add package ACommerce.Transactions.Core
dotnet add package ACommerce.Transactions.Core.Api
```

## License

MIT