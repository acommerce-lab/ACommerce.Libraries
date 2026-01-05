# ACommerce Marketplace API - Google Cloud Platform Edition

A production-ready e-commerce marketplace API optimized for Google Cloud Marketplace deployment with usage-based billing support.

## Features

- **Multi-vendor Marketplace**: Complete vendor management, products, listings
- **Full Catalog System**: Products, categories, attributes, units, currencies
- **Sales Pipeline**: Cart, orders, payments integration points
- **Cloud-Native**: Designed for GCP Cloud Run, Cloud SQL, Redis
- **Usage-Based Billing**: Built-in hooks for GCP Marketplace billing integration

## Quick Start

### Local Development

```bash
# Run with SQLite (default for development)
dotnet run

# Run with PostgreSQL
DATABASE_PROVIDER=postgresql \
DATABASE_URL=postgres://user:pass@localhost:5432/acommerce \
dotnet run
```

### Docker

```bash
# Build
docker build -t acommerce-marketplace .

# Run
docker run -p 8080:8080 \
  -e DATABASE_URL=postgres://user:pass@host:5432/db \
  -e JWT__SecretKey=your-secret-key-min-32-characters \
  acommerce-marketplace
```

### Deploy to Cloud Run

```bash
# Build and push to GCR
gcloud builds submit --tag gcr.io/PROJECT_ID/acommerce-marketplace

# Deploy
gcloud run deploy acommerce-marketplace \
  --image gcr.io/PROJECT_ID/acommerce-marketplace \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --set-env-vars "DATABASE_URL=..." \
  --set-env-vars "JWT__SecretKey=..."
```

## Environment Variables

### Required

| Variable | Description | Example |
|----------|-------------|---------|
| `JWT__SecretKey` | JWT signing key (min 32 chars) | `YourSuperSecretKey...` |

### Database Configuration

| Variable | Description | Example |
|----------|-------------|---------|
| `DATABASE_URL` | Full connection string (Heroku style) | `postgres://user:pass@host:5432/db` |
| `DATABASE_CONNECTION_STRING` | Alternative to DATABASE_URL | Standard connection string |
| `DATABASE_PROVIDER` | Force provider: `postgresql`, `sqlserver`, `sqlite` | `postgresql` |
| `CLOUD_SQL_CONNECTION_NAME` | GCP Cloud SQL instance | `project:region:instance` |
| `DB_HOST` | Database host (component-based config) | `localhost` |
| `DB_PORT` | Database port | `5432` |
| `DB_NAME` | Database name | `acommerce` |
| `DB_USER` | Database user | `postgres` |
| `DB_PASSWORD` | Database password | `secret` |

### Redis (Session Management)

| Variable | Description | Example |
|----------|-------------|---------|
| `REDIS_CONNECTION_STRING` | Redis connection | `redis:6379` |

### Google Cloud Integration

| Variable | Description | Example |
|----------|-------------|---------|
| `GCP_PROJECT_ID` | Google Cloud project ID | `my-project` |
| `PORT` | HTTP port (Cloud Run sets this) | `8080` |

### GCP Marketplace Billing

| Variable | Description | Example |
|----------|-------------|---------|
| `GCP_ENABLE_USAGE_REPORTING` | Enable billing integration | `true` |
| `GCP_SERVICE_NAME` | Marketplace service name | `acommerce-marketplace` |
| `GCP_ENTITLEMENT_ID` | Customer entitlement (injected by GCP) | `entitlement-123` |
| `GCP_CONSUMER_ID` | Customer ID (injected by GCP) | `projects/customer-project` |

### Storage

| Variable | Description | Example |
|----------|-------------|---------|
| `Files__Storage__GoogleCloud__BucketName` | GCS bucket for files | `my-bucket` |
| `GOOGLE_APPLICATION_CREDENTIALS` | GCP service account key path | `/secrets/sa-key.json` |

### CORS & Security

| Variable | Description | Example |
|----------|-------------|---------|
| `ALLOWED_ORIGINS` | Comma-separated CORS origins | `https://app.example.com,https://admin.example.com` |

### Operational

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Production` |
| `RUN_MIGRATIONS` | Run DB migrations on startup | `false` |
| `Logging__LogLevel__Default` | Log level | `Information` |

## Health Endpoints

| Endpoint | Purpose | Use Case |
|----------|---------|----------|
| `/health` | Liveness probe | Is the service running? |
| `/health/ready` | Readiness probe | Can accept traffic? (includes DB check) |
| `/health/startup` | Startup probe | Has initialization completed? |
| `/healthz` | Simple health | Load balancer check |

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh token
- `GET /api/auth/me` - Current user info

### Profiles
- `GET/POST/PUT/DELETE /api/profiles` - User profile management

### Catalog
- `/api/products` - Product management
- `/api/productlistings` - Vendor product listings
- `/api/productcategories` - Category hierarchy
- `/api/attributedefinitions` - Dynamic product attributes
- `/api/units` - Units of measure
- `/api/currencies` - Currency management

### Marketplace
- `/api/vendors` - Vendor/seller management
- `/api/subscriptions` - Vendor subscriptions

### Sales
- `/api/orders` - Order management
- `/api/payments` - Payment processing

## Usage-Based Billing Integration

The service includes hooks for GCP Marketplace usage-based billing:

### Automatic Tracking
- **API Requests**: Every successful API call is tracked
- **Orders**: Order creation and transaction values

### Manual Integration Points

```csharp
// Inject IUsageReportingService where needed
public class MyService
{
    private readonly IUsageReportingService _usage;

    public async Task DoSomethingBillable()
    {
        // Report custom metrics
        await _usage.RecordUsageAsync(UsageMetricType.StorageUsedMB, fileSizeMB);
        await _usage.RecordUsageAsync("custom_metric", value);
    }
}
```

### Available Metrics
- `ApiRequests` - API call count
- `OrdersCreated` - Number of orders
- `TransactionValue` - Order totals (in cents)
- `ActiveVendors` - Vendor count
- `StorageUsedMB` - Storage consumption
- `ActiveUsers` - User count

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Cloud Run Service                         │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              ACommerce.Marketplace.GCP              │    │
│  │  ┌───────────┐ ┌───────────┐ ┌─────────────────┐   │    │
│  │  │   Auth    │ │  Catalog  │ │   Marketplace   │   │    │
│  │  └───────────┘ └───────────┘ └─────────────────┘   │    │
│  │  ┌───────────┐ ┌───────────┐ ┌─────────────────┐   │    │
│  │  │   Sales   │ │   Files   │ │ Usage Reporting │   │    │
│  │  └───────────┘ └───────────┘ └─────────────────┘   │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
         │                    │                    │
         ▼                    ▼                    ▼
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│  Cloud SQL  │      │    Redis    │      │     GCS     │
│ (PostgreSQL)│      │  (Memorystore) │   │  (Storage)  │
└─────────────┘      └─────────────┘      └─────────────┘
```

## License

Proprietary - ACommerce Platform
