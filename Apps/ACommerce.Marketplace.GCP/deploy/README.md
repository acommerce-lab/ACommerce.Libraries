# ACommerce Marketplace - Deployment Guide

This directory contains deployment manifests for the ACommerce Marketplace API on Google Cloud Platform.

## Directory Structure

```
deploy/
├── kubernetes/           # Standard Kubernetes manifests
│   ├── configmap.yaml    # Non-sensitive configuration
│   ├── secrets.yaml      # Sensitive configuration (template)
│   ├── deployment.yaml   # Pod deployment with health checks
│   ├── service.yaml      # Service, Ingress, Network Policies
│   └── kustomization.yaml
│
└── marketplace/          # GCP Marketplace specific
    ├── schema.yaml       # Customer configuration form
    └── application.yaml  # Application resource
```

## Quick Start

### Prerequisites

1. **GKE Cluster**: A running GKE cluster with Workload Identity enabled
2. **Cloud SQL**: A PostgreSQL instance (recommended)
3. **Container Image**: Built and pushed to GCR

### Build and Push Image

```bash
# From the repository root
docker build -t gcr.io/PROJECT_ID/acommerce-marketplace:1.0.0 \
  -f Apps/ACommerce.Marketplace.GCP/Dockerfile .

docker push gcr.io/PROJECT_ID/acommerce-marketplace:1.0.0
```

### Deploy to GKE

```bash
# Create namespace
kubectl create namespace acommerce

# Update secrets with real values
# Edit deploy/kubernetes/secrets.yaml with base64-encoded values

# Apply all manifests
kubectl apply -k deploy/kubernetes/

# Or apply individually
kubectl apply -f deploy/kubernetes/configmap.yaml
kubectl apply -f deploy/kubernetes/secrets.yaml
kubectl apply -f deploy/kubernetes/deployment.yaml
kubectl apply -f deploy/kubernetes/service.yaml
```

### Verify Deployment

```bash
# Check pods
kubectl get pods -n acommerce -l app=acommerce-marketplace

# Check service
kubectl get svc -n acommerce acommerce-marketplace

# Check ingress
kubectl get ingress -n acommerce

# View logs
kubectl logs -f deployment/acommerce-marketplace -n acommerce

# Test health endpoint
kubectl port-forward svc/acommerce-marketplace 8080:80 -n acommerce
curl http://localhost:8080/healthz
```

## Configuration

### Environment Variables

All configuration is via environment variables. See `configmap.yaml` and `secrets.yaml` for the complete list.

#### Required Secrets

| Variable | Description |
|----------|-------------|
| `DB_HOST` | Database host/socket path |
| `DB_USER` | Database username |
| `DB_PASSWORD` | Database password |
| `JWT__SecretKey` | JWT signing key (min 32 chars) |
| `REDIS_CONNECTION_STRING` | Redis connection string |

#### GCP Marketplace Billing

| Variable | Description |
|----------|-------------|
| `GCP_PROJECT_ID` | Your GCP project ID |
| `GCP_ENABLE_USAGE_REPORTING` | Set to `true` for billing |
| `GCP_ENTITLEMENT_ID` | Injected by Marketplace |
| `GCP_CONSUMER_ID` | Injected by Marketplace |

### Using GCP Secret Manager

For production, use GCP Secret Manager instead of Kubernetes Secrets:

```bash
# Create secrets in Secret Manager
gcloud secrets create acommerce-db-password --data-file=-
gcloud secrets create acommerce-jwt-key --data-file=-

# Grant access to the service account
gcloud secrets add-iam-policy-binding acommerce-db-password \
  --member="serviceAccount:acommerce-marketplace@PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/secretmanager.secretAccessor"
```

Then modify the deployment to use Secret Manager volumes or the CSI driver.

## Cloud SQL Connectivity

### Option 1: Cloud SQL Proxy Sidecar

Uncomment the Cloud SQL Proxy sidecar in `deployment.yaml`:

```yaml
containers:
  - name: cloud-sql-proxy
    image: gcr.io/cloud-sql-connectors/cloud-sql-proxy:2.8.0
    args:
      - "--structured-logs"
      - "--port=5432"
      - "PROJECT_ID:REGION:INSTANCE_NAME"
```

Set `DB_HOST` to `localhost`.

### Option 2: Private IP

Use the Cloud SQL private IP directly:

```yaml
env:
  - name: DB_HOST
    value: "10.x.x.x"  # Cloud SQL private IP
```

### Option 3: Cloud SQL Auth Proxy (Unix Socket)

For Workload Identity:

```yaml
env:
  - name: DB_HOST
    value: "/cloudsql/PROJECT:REGION:INSTANCE"
```

## Scaling

### Manual Scaling

```bash
kubectl scale deployment acommerce-marketplace --replicas=5 -n acommerce
```

### Autoscaling

The HPA is configured in `deployment.yaml`:

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
spec:
  minReplicas: 2
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          averageUtilization: 70
```

## Health Checks

| Endpoint | Purpose | Probe Type |
|----------|---------|------------|
| `/health` | Is the app running? | Liveness |
| `/health/ready` | Can accept traffic? (DB connected) | Readiness |
| `/health/startup` | Has initialization completed? | Startup |
| `/healthz` | Simple check for LB | - |

## GCP Marketplace Deployment

### Schema.yaml

The `marketplace/schema.yaml` defines the customer-facing configuration form in GCP Marketplace. It includes:

- Store configuration (name, URL)
- Database settings
- Redis configuration
- Authentication options
- Scaling parameters

### Submitting to Marketplace

1. Build the deployer image
2. Create the Marketplace listing
3. Submit for review

See [GCP Marketplace Partner Documentation](https://cloud.google.com/marketplace/docs/partners/kubernetes) for details.

## Troubleshooting

### Pod not starting

```bash
# Check pod events
kubectl describe pod -l app=acommerce-marketplace -n acommerce

# Check container logs
kubectl logs -l app=acommerce-marketplace -n acommerce --previous
```

### Database connection issues

```bash
# Test from a debug pod
kubectl run -it --rm debug --image=postgres:15 --restart=Never -- \
  psql "host=DB_HOST dbname=acommerce user=DB_USER"
```

### Health check failures

```bash
# Port forward and test manually
kubectl port-forward deployment/acommerce-marketplace 8080:8080 -n acommerce
curl -v http://localhost:8080/health/ready
```

## Security Considerations

1. **Never commit real secrets** - Use placeholders in `secrets.yaml`
2. **Use Workload Identity** - Avoid service account keys
3. **Enable Network Policies** - Restrict pod-to-pod traffic
4. **Use Cloud Armor** - Protect against DDoS and web attacks
5. **Enable audit logging** - Track API access
