#!/bin/bash

# Alibaba Cloud ECS Deployment Script for Ashare API
# Instance: i-l4vbjvebgid9bqmg2lp3 (ecs.g9i.large)
# Region: Saudi Arabia (me-central-1)

set -e

# Configuration - Update these values
ACR_REGISTRY="${ACR_REGISTRY:-registry.me-central-1.aliyuncs.com}"
ACR_NAMESPACE="${ACR_NAMESPACE:-ashare}"
IMAGE_NAME="${IMAGE_NAME:-ashare-api}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
ECS_HOST="${ECS_HOST:-}"  # ECS public IP or hostname
ECS_USER="${ECS_USER:-root}"
SSH_KEY="${SSH_KEY:-~/.ssh/id_rsa}"
CONTAINER_NAME="${CONTAINER_NAME:-ashare-api}"
CONTAINER_PORT="${CONTAINER_PORT:-8080}"
HOST_PORT="${HOST_PORT:-80}"

# Full image path
FULL_IMAGE="${ACR_REGISTRY}/${ACR_NAMESPACE}/${IMAGE_NAME}:${IMAGE_TAG}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check required tools
check_requirements() {
    log_info "Checking requirements..."

    if ! command -v docker &> /dev/null; then
        log_error "Docker is not installed"
        exit 1
    fi

    if ! command -v ssh &> /dev/null; then
        log_error "SSH is not installed"
        exit 1
    fi

    if [ -z "$ECS_HOST" ]; then
        log_error "ECS_HOST environment variable is not set"
        log_info "Usage: ECS_HOST=<your-ecs-ip> ./deploy-alibaba.sh"
        exit 1
    fi
}

# Login to Alibaba Container Registry
acr_login() {
    log_info "Logging in to Alibaba Container Registry..."

    if [ -z "$ACR_USERNAME" ] || [ -z "$ACR_PASSWORD" ]; then
        log_warn "ACR_USERNAME or ACR_PASSWORD not set"
        log_info "Please login manually: docker login ${ACR_REGISTRY}"
        read -p "Press Enter after logging in..."
    else
        echo "$ACR_PASSWORD" | docker login --username="$ACR_USERNAME" --password-stdin "$ACR_REGISTRY"
    fi
}

# Build Docker image
build_image() {
    log_info "Building Docker image..."

    # Navigate to repository root
    SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

    cd "$REPO_ROOT"

    docker build \
        --build-arg ACOMMERCE_BUILD_MODE=CLOUD \
        -t "$FULL_IMAGE" \
        -f Apps/Ashare.Api/Dockerfile \
        .

    log_info "Image built: $FULL_IMAGE"
}

# Push image to ACR
push_image() {
    log_info "Pushing image to Alibaba Container Registry..."
    docker push "$FULL_IMAGE"
    log_info "Image pushed successfully"
}

# Deploy to ECS via SSH
deploy_to_ecs() {
    log_info "Deploying to ECS instance: $ECS_HOST"

    # SSH commands to execute on ECS
    SSH_COMMANDS=$(cat <<EOF
set -e

echo "Logging into ACR..."
if [ -n "$ACR_USERNAME" ] && [ -n "$ACR_PASSWORD" ]; then
    echo "$ACR_PASSWORD" | docker login --username="$ACR_USERNAME" --password-stdin "$ACR_REGISTRY"
fi

echo "Pulling latest image..."
docker pull $FULL_IMAGE

echo "Stopping existing container (if any)..."
docker stop $CONTAINER_NAME 2>/dev/null || true
docker rm $CONTAINER_NAME 2>/dev/null || true

echo "Starting new container..."
docker run -d \
    --name $CONTAINER_NAME \
    --restart unless-stopped \
    -p $HOST_PORT:$CONTAINER_PORT \
    -e ASPNETCORE_ENVIRONMENT=Production \
    -e ASPNETCORE_URLS=http://+:$CONTAINER_PORT \
    -v /var/ashare/data:/app/data \
    -v /var/ashare/logs:/app/logs \
    -v /var/ashare/uploads:/app/uploads \
    $FULL_IMAGE

echo "Waiting for container to start..."
sleep 5

echo "Checking container health..."
docker ps | grep $CONTAINER_NAME

echo "Container logs (last 20 lines):"
docker logs --tail 20 $CONTAINER_NAME

echo "Deployment completed!"
EOF
)

    ssh -i "$SSH_KEY" -o StrictHostKeyChecking=no "$ECS_USER@$ECS_HOST" "$SSH_COMMANDS"
}

# Cleanup old images on ECS
cleanup_ecs() {
    log_info "Cleaning up old images on ECS..."

    ssh -i "$SSH_KEY" -o StrictHostKeyChecking=no "$ECS_USER@$ECS_HOST" \
        "docker image prune -f && docker container prune -f"
}

# Main deployment flow
main() {
    log_info "Starting Alibaba Cloud deployment..."
    log_info "Image: $FULL_IMAGE"
    log_info "Target: $ECS_HOST"

    check_requirements
    acr_login
    build_image
    push_image
    deploy_to_ecs
    cleanup_ecs

    log_info "==================================="
    log_info "Deployment completed successfully!"
    log_info "API URL: http://$ECS_HOST:$HOST_PORT"
    log_info "==================================="
}

# Parse arguments
case "${1:-deploy}" in
    build)
        check_requirements
        build_image
        ;;
    push)
        acr_login
        push_image
        ;;
    deploy)
        main
        ;;
    ssh)
        ssh -i "$SSH_KEY" "$ECS_USER@$ECS_HOST"
        ;;
    logs)
        ssh -i "$SSH_KEY" "$ECS_USER@$ECS_HOST" "docker logs -f $CONTAINER_NAME"
        ;;
    status)
        ssh -i "$SSH_KEY" "$ECS_USER@$ECS_HOST" "docker ps | grep $CONTAINER_NAME && docker logs --tail 50 $CONTAINER_NAME"
        ;;
    *)
        echo "Usage: $0 {build|push|deploy|ssh|logs|status}"
        echo ""
        echo "Commands:"
        echo "  build   - Build Docker image locally"
        echo "  push    - Push image to Alibaba Container Registry"
        echo "  deploy  - Full deployment (build, push, deploy to ECS)"
        echo "  ssh     - SSH into ECS instance"
        echo "  logs    - View container logs"
        echo "  status  - Check container status"
        echo ""
        echo "Environment variables:"
        echo "  ECS_HOST      - ECS public IP (required)"
        echo "  ACR_USERNAME  - ACR username"
        echo "  ACR_PASSWORD  - ACR password"
        echo "  ACR_REGISTRY  - ACR registry URL (default: registry.me-central-1.aliyuncs.com)"
        echo "  ACR_NAMESPACE - ACR namespace (default: ashare)"
        echo "  IMAGE_TAG     - Image tag (default: latest)"
        exit 1
        ;;
esac
