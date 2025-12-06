#!/bin/bash

# =========================================================
# Ashare API - Google Cloud Platform Setup Script
# =========================================================
# This script helps set up the GCP environment for deploying
# the Ashare API to Google Cloud Run
#
# Prerequisites:
# - Google Cloud SDK (gcloud) installed
# - Authenticated with: gcloud auth login
# =========================================================

set -e

# Configuration
PROJECT_ID="${GCP_PROJECT_ID:-asharesa}"
REGION="${GCP_REGION:-me-central2}"  # Middle East (Dammam, Saudi Arabia)
SERVICE_NAME="ashare-api"
ARTIFACT_REGISTRY_REPO="ashare"

echo "========================================"
echo "Ashare API - GCP Setup"
echo "========================================"
echo "Project: $PROJECT_ID"
echo "Region: $REGION"
echo "========================================"

# Check if gcloud is installed
if ! command -v gcloud &> /dev/null; then
    echo "Error: gcloud CLI is not installed."
    echo "Please install it from: https://cloud.google.com/sdk/docs/install"
    exit 1
fi

# Set project
echo "[1/7] Setting GCP project..."
gcloud config set project $PROJECT_ID

# Enable required APIs
echo "[2/7] Enabling required APIs..."
gcloud services enable \
    cloudbuild.googleapis.com \
    run.googleapis.com \
    artifactregistry.googleapis.com \
    secretmanager.googleapis.com \
    cloudresourcemanager.googleapis.com

# Create Artifact Registry repository
echo "[3/7] Creating Artifact Registry repository..."
gcloud artifacts repositories create $ARTIFACT_REGISTRY_REPO \
    --repository-format=docker \
    --location=$REGION \
    --description="Ashare Docker images" \
    2>/dev/null || echo "Repository already exists"

# Configure Docker to use Artifact Registry
echo "[4/7] Configuring Docker authentication..."
gcloud auth configure-docker $REGION-docker.pkg.dev

# Create secrets in Secret Manager
echo "[5/7] Setting up Secret Manager secrets..."
echo "Please create the following secrets manually or use the commands below:"
echo ""
echo "  # JWT Secret Key"
echo "  echo -n 'YOUR_JWT_SECRET_KEY' | gcloud secrets create jwt-secret --data-file=-"
echo ""
echo "  # Noon Payment Authorization Key"
echo "  echo -n 'YOUR_NOON_AUTH_KEY' | gcloud secrets create noon-auth-key --data-file=-"
echo ""
echo "  # Nafath API Key"
echo "  echo -n 'YOUR_NAFATH_API_KEY' | gcloud secrets create nafath-api-key --data-file=-"
echo ""

# Grant Cloud Run access to secrets
echo "[6/7] Granting Cloud Run access to secrets..."
PROJECT_NUMBER=$(gcloud projects describe $PROJECT_ID --format='value(projectNumber)')
CLOUD_RUN_SA="${PROJECT_NUMBER}-compute@developer.gserviceaccount.com"

for secret in jwt-secret noon-auth-key nafath-api-key; do
    gcloud secrets add-iam-policy-binding $secret \
        --member="serviceAccount:$CLOUD_RUN_SA" \
        --role="roles/secretmanager.secretAccessor" \
        2>/dev/null || echo "Secret $secret doesn't exist yet - create it first"
done

echo "[7/7] Setup complete!"
echo ""
echo "========================================"
echo "Next Steps:"
echo "========================================"
echo ""
echo "1. Create secrets (if not done):"
echo "   gcloud secrets create jwt-secret --data-file=<(echo -n 'YOUR_SECRET')"
echo ""
echo "2. Build and deploy manually:"
echo "   gcloud builds submit --config=cloudbuild.yaml"
echo ""
echo "3. Or set up Cloud Build trigger for automatic deployments"
echo ""
echo "4. After deployment, get the service URL:"
echo "   gcloud run services describe $SERVICE_NAME --region=$REGION --format='value(status.url)'"
echo ""
echo "========================================"
