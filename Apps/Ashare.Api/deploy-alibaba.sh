#!/bin/bash
# Alibaba Cloud ECS + ACR Deployment Script for Ashare API
# Target: ecs-user@<ECS_HOST> running Docker, nginx reverse-proxy → :8080

set -euo pipefail

# === Git-based version detection ===
GIT_SHA="$(git rev-parse --short=8 HEAD 2>/dev/null || echo unknown)"
GIT_BRANCH="$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo unknown)"
GIT_DIRTY=""
[ -n "$(git status --porcelain 2>/dev/null)" ] && GIT_DIRTY="-dirty"

# === Registry config ===
ACR_REGISTRY="${ACR_REGISTRY:-registry.me-central-1.aliyuncs.com}"
ACR_REGISTRY_INTRANET="${ACR_REGISTRY_INTRANET:-registry-vpc.me-central-1.aliyuncs.com}"
ACR_NAMESPACE="${ACR_NAMESPACE:-ashare}"
IMAGE_NAME="${IMAGE_NAME:-ashare-api}"
IMAGE_TAG="${IMAGE_TAG:-${GIT_SHA}${GIT_DIRTY}}"
IMAGE_REPO_PUBLIC="${ACR_REGISTRY}/${ACR_NAMESPACE}/${IMAGE_NAME}"
IMAGE_REPO_INTRANET="${ACR_REGISTRY_INTRANET}/${ACR_NAMESPACE}/${IMAGE_NAME}"

# === ECS / SSH ===
ECS_HOST="${ECS_HOST:-}"
ECS_USER="${ECS_USER:-ecs-user}"
SSH_KEY="${SSH_KEY:-$HOME/.ssh/id_rsa}"

# === Container runtime ===
CONTAINER_NAME="${CONTAINER_NAME:-ashare-api}"
CONTAINER_PORT="${CONTAINER_PORT:-8080}"
HOST_PORT="${HOST_PORT:-8080}"
REMOTE_ENV_FILE="${REMOTE_ENV_FILE:-/home/ecs-user/ashare.env}"
REMOTE_SECRETS_DIR="${REMOTE_SECRETS_DIR:-/app/secrets}"
HEALTH_PATH="${HEALTH_PATH:-/health}"

# === Logging ===
R='\033[0;31m'; G='\033[0;32m'; Y='\033[1;33m'; B='\033[0;34m'; N='\033[0m'
log_info()  { echo -e "${G}[INFO]${N}  $1"; }
log_warn()  { echo -e "${Y}[WARN]${N}  $1"; }
log_error() { echo -e "${R}[ERROR]${N} $1" >&2; }
log_step()  { echo; echo -e "${B}━━━ $1 ━━━${N}"; }

# === SSH helper (handles missing key file gracefully) ===
ssh_opts() {
    local opts="-o ServerAliveInterval=30 -o ConnectTimeout=15"
    local key_resolved="${SSH_KEY/#\~/$HOME}"
    if [ -f "$key_resolved" ]; then
        opts="$opts -i $SSH_KEY"
    fi
    echo "$opts"
}

ssh_target() { echo "${ECS_USER}@${ECS_HOST}"; }

# === Pre-flight checks ===
preflight() {
    command -v docker >/dev/null || { log_error "docker not installed"; exit 1; }
    command -v ssh    >/dev/null || { log_error "ssh not installed";    exit 1; }
    command -v git    >/dev/null || { log_error "git not installed";    exit 1; }
    [ -n "$ECS_HOST" ] || { log_error "ECS_HOST is required. Source deploy-alibaba.env first."; exit 1; }
}

# === ACR login (local) ===
acr_login_local() {
    log_step "ACR login (local)"
    if [ -n "${ACR_USERNAME:-}" ] && [ -n "${ACR_PASSWORD:-}" ]; then
        echo "$ACR_PASSWORD" | docker login --username="$ACR_USERNAME" --password-stdin "$ACR_REGISTRY"
    else
        log_warn "ACR_USERNAME / ACR_PASSWORD not set — interactive login."
        docker login "$ACR_REGISTRY"
    fi
}

# === Build ===
build_image() {
    log_step "Building ${IMAGE_REPO_PUBLIC}:${IMAGE_TAG}"

    if [ -n "$GIT_DIRTY" ]; then
        log_warn "Working tree has uncommitted changes. Tag: ${IMAGE_TAG}"
    fi

    local script_dir repo_root
    script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    repo_root="$(cd "$script_dir/../.." && pwd)"
    cd "$repo_root"

    docker build \
        --build-arg ACOMMERCE_BUILD_MODE=CLOUD \
        -t "${IMAGE_REPO_PUBLIC}:${IMAGE_TAG}" \
        -t "${IMAGE_REPO_PUBLIC}:latest" \
        -f Apps/Ashare.Api/Dockerfile \
        .

    log_info "Built: ${IMAGE_REPO_PUBLIC}:${IMAGE_TAG}"
    log_info "Built: ${IMAGE_REPO_PUBLIC}:latest"
}

# === Push ===
push_image() {
    log_step "Pushing to ACR"
    docker push "${IMAGE_REPO_PUBLIC}:${IMAGE_TAG}"
    docker push "${IMAGE_REPO_PUBLIC}:latest"
    log_info "Pushed: ${IMAGE_TAG} and latest"
}

# === Deploy on ECS via SSH ===
deploy_to_ecs() {
    log_step "Deploying ${IMAGE_TAG} to ${ECS_HOST}"

    # Pass values to remote via env (printf %q makes them shell-safe)
    local acr_user_esc acr_pass_esc
    acr_user_esc=$(printf '%q' "${ACR_USERNAME:-}")
    acr_pass_esc=$(printf '%q' "${ACR_PASSWORD:-}")

    # shellcheck disable=SC2029
    ssh $(ssh_opts) "$(ssh_target)" \
        "REMOTE_IMAGE=$(printf '%q' "${IMAGE_REPO_INTRANET}:${IMAGE_TAG}") \
         REMOTE_IMAGE_LATEST=$(printf '%q' "${IMAGE_REPO_INTRANET}:latest") \
         REMOTE_REGISTRY=$(printf '%q' "$ACR_REGISTRY_INTRANET") \
         ACR_USERNAME=$acr_user_esc \
         ACR_PASSWORD=$acr_pass_esc \
         CONTAINER_NAME=$(printf '%q' "$CONTAINER_NAME") \
         HOST_PORT=$(printf '%q' "$HOST_PORT") \
         CONTAINER_PORT=$(printf '%q' "$CONTAINER_PORT") \
         ENV_FILE=$(printf '%q' "$REMOTE_ENV_FILE") \
         SECRETS_DIR=$(printf '%q' "$REMOTE_SECRETS_DIR") \
         HEALTH_PATH=$(printf '%q' "$HEALTH_PATH") \
         bash -s" <<'REMOTE_SCRIPT'
set -euo pipefail

echo "→ Pre-flight: env file ($ENV_FILE)"
[ -f "$ENV_FILE" ] || { echo "[FATAL] $ENV_FILE not found"; exit 1; }

echo "→ Pre-flight: secrets dir ($SECRETS_DIR)"
[ -d "$SECRETS_DIR" ] || { echo "[FATAL] $SECRETS_DIR not found"; exit 1; }

echo "→ Logging into ACR intranet endpoint..."
if [ -n "$ACR_USERNAME" ] && [ -n "$ACR_PASSWORD" ]; then
    echo "$ACR_PASSWORD" | docker login --username="$ACR_USERNAME" --password-stdin "$REMOTE_REGISTRY"
else
    echo "[WARN] No ACR creds in env — assuming docker is already logged in to $REMOTE_REGISTRY"
fi

echo "→ Pulling: $REMOTE_IMAGE"
docker pull "$REMOTE_IMAGE"
docker tag  "$REMOTE_IMAGE" "$REMOTE_IMAGE_LATEST"

echo "→ Saving rollback marker..."
PREV="$(docker inspect --format '{{.Config.Image}}' "$CONTAINER_NAME" 2>/dev/null || echo none)"
echo "$PREV" > /tmp/ashare-previous-image.txt
echo "  Previous: $PREV"

echo "→ Stopping old container (if exists)..."
docker stop "$CONTAINER_NAME" 2>/dev/null || true
docker rm   "$CONTAINER_NAME" 2>/dev/null || true

echo "→ Starting new container..."
docker run -d \
    --name "$CONTAINER_NAME" \
    --restart unless-stopped \
    --env-file "$ENV_FILE" \
    -v "$SECRETS_DIR:$SECRETS_DIR:ro" \
    -p "$HOST_PORT:$CONTAINER_PORT" \
    "$REMOTE_IMAGE"

echo "→ Health check on http://localhost:$HOST_PORT$HEALTH_PATH ..."
HEALTHY=0
for i in 1 2 3 4 5 6 7 8 9 10 11 12; do
    sleep 5
    if curl -fsS "http://localhost:$HOST_PORT$HEALTH_PATH" >/dev/null 2>&1; then
        echo "  ✅ Healthy (attempt $i)"
        HEALTHY=1
        break
    fi
    echo "  attempt $i/12 — not ready..."
done

if [ "$HEALTHY" != "1" ]; then
    echo
    echo "[FATAL] Health check failed after 60s."
    echo "Last 60 lines of container logs:"
    docker logs --tail 60 "$CONTAINER_NAME" || true
    echo
    echo "To rollback:  ./deploy-alibaba.sh rollback"
    exit 1
fi

echo "→ Pruning dangling images..."
docker image prune -f >/dev/null

echo
echo "→ Final state:"
docker ps --filter "name=$CONTAINER_NAME" --format "table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}"
REMOTE_SCRIPT
}

# === Rollback ===
rollback() {
    preflight
    log_step "Reading previous image from server"
    PREV=$(ssh $(ssh_opts) "$(ssh_target)" "cat /tmp/ashare-previous-image.txt 2>/dev/null || echo none")
    if [ -z "$PREV" ] || [ "$PREV" = "none" ]; then
        log_error "No rollback marker on server. Run rollback manually with: IMAGE_TAG=<sha> $0 redeploy"
        exit 1
    fi
    log_info "Rolling back to: $PREV"
    # Parse out the tag, override IMAGE_TAG, redeploy
    IMAGE_TAG="${PREV##*:}"
    IMAGE_REPO_INTRANET="${PREV%:*}"
    deploy_to_ecs
    log_info "Rollback complete: $IMAGE_TAG"
}

# === Usage ===
usage() {
    cat <<HELP
Ashare API — Alibaba Cloud Deployment

Usage:
  source ./deploy-alibaba.env
  ./deploy-alibaba.sh <command>

Commands:
  build      Build image locally (tags: <git-sha> + latest)
  push       Login to ACR and push current image
  deploy     Full flow: build → push → SSH deploy → health check
  redeploy   Skip build/push; pull existing IMAGE_TAG and restart
  rollback   Revert to the image that was running before the last deploy
  ssh        Open interactive SSH to ECS
  logs       Tail container logs
  status     Show container state + last 30 log lines

Current settings:
  Git SHA:    ${GIT_SHA}${GIT_DIRTY}
  Branch:     ${GIT_BRANCH}
  Image:      ${IMAGE_REPO_PUBLIC}:${IMAGE_TAG}
  Target:     ${ECS_USER}@${ECS_HOST:-<unset>}

Override any variable inline:
  IMAGE_TAG=a3f9c12 ./deploy-alibaba.sh redeploy
HELP
}

# === Main ===
case "${1:-}" in
    build)
        preflight
        build_image
        ;;
    push)
        preflight
        acr_login_local
        push_image
        ;;
    deploy)
        preflight
        acr_login_local
        build_image
        push_image
        deploy_to_ecs
        log_info "════════════════════════════════════════════"
        log_info "Deployment finished: ${IMAGE_TAG}"
        log_info "URL: https://api.ashare.sa"
        log_info "════════════════════════════════════════════"
        ;;
    redeploy)
        preflight
        deploy_to_ecs
        log_info "Re-deployed: ${IMAGE_TAG}"
        ;;
    rollback)
        rollback
        ;;
    ssh)
        preflight
        # shellcheck disable=SC2086
        ssh $(ssh_opts) "$(ssh_target)"
        ;;
    logs)
        preflight
        # shellcheck disable=SC2086
        ssh $(ssh_opts) "$(ssh_target)" "docker logs -f --tail 100 $CONTAINER_NAME"
        ;;
    status)
        preflight
        # shellcheck disable=SC2086
        ssh $(ssh_opts) "$(ssh_target)" "docker ps --filter name=$CONTAINER_NAME && echo && docker logs --tail 30 $CONTAINER_NAME"
        ;;
    *)
        usage
        exit 1
        ;;
esac
