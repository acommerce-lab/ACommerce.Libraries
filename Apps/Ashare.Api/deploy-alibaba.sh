#!/bin/bash
# Ashare API — Fast TAR-streaming Deployment (no registry needed)
# Builds locally, streams compressed image via SSH, restarts container on ECS.

set -euo pipefail

# === Git-based version ===
GIT_SHA="$(git rev-parse --short=8 HEAD 2>/dev/null || echo unknown)"
GIT_DIRTY=""
[ -n "$(git status --porcelain 2>/dev/null)" ] && GIT_DIRTY="-dirty"

# === Image ===
IMAGE_NAME="${IMAGE_NAME:-ashare-api}"
IMAGE_TAG="${IMAGE_TAG:-${GIT_SHA}${GIT_DIRTY}}"
LOCAL_IMAGE="${IMAGE_NAME}:${IMAGE_TAG}"
LATEST_IMAGE="${IMAGE_NAME}:latest"

# === ECS / SSH ===
ECS_HOST="${ECS_HOST:-}"
ECS_USER="${ECS_USER:-ecs-user}"
SSH_KEY="${SSH_KEY:-$HOME/.ssh/id_rsa}"

# === Container runtime (matches existing prod setup) ===
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

# === SSH options (key file optional → password fallback) ===
ssh_opts() {
    local opts="-o ServerAliveInterval=30 -o ConnectTimeout=15"
    local key_resolved="${SSH_KEY/#\~/$HOME}"
    [ -f "$key_resolved" ] && opts="$opts -i $SSH_KEY"
    echo "$opts"
}
ssh_target() { echo "${ECS_USER}@${ECS_HOST}"; }

# === Preflight ===
preflight() {
    command -v docker >/dev/null || { log_error "docker not installed"; exit 1; }
    command -v ssh    >/dev/null || { log_error "ssh not installed";    exit 1; }
    command -v git    >/dev/null || { log_error "git not installed";    exit 1; }
    command -v gzip   >/dev/null || { log_error "gzip not installed";   exit 1; }
    [ -n "$ECS_HOST" ] || { log_error "ECS_HOST is required. Source deploy-alibaba.env first."; exit 1; }
}

# === Build ===
build_image() {
    log_step "Building ${LOCAL_IMAGE}"
    [ -n "$GIT_DIRTY" ] && log_warn "Uncommitted changes; tag = ${IMAGE_TAG}"

    local script_dir repo_root
    script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    repo_root="$(cd "$script_dir/../.." && pwd)"
    cd "$repo_root"

    docker build \
        --build-arg ACOMMERCE_BUILD_MODE=CLOUD \
        -t "$LOCAL_IMAGE" \
        -t "$LATEST_IMAGE" \
        -f Apps/Ashare.Api/Dockerfile \
        .

    local size_mb
    size_mb=$(docker image inspect "$LOCAL_IMAGE" --format '{{.Size}}' | awk '{printf "%.0f", $1/1024/1024}')
    log_info "Built: ${LOCAL_IMAGE} (~${size_mb} MB uncompressed)"
}

# === Upload via SSH stream (gzipped) ===
upload_image() {
    log_step "Streaming image to ${ECS_HOST}"
    log_warn "Transfer takes a few minutes; do not interrupt."

    # shellcheck disable=SC2046
    docker save "$LOCAL_IMAGE" "$LATEST_IMAGE" \
        | gzip -1 \
        | ssh $(ssh_opts) "$(ssh_target)" "gunzip | docker load"

    log_info "Image loaded on server"
}

# === Restart container on ECS with the new image ===
deploy_container() {
    log_step "Restarting container on ${ECS_HOST}"

    # shellcheck disable=SC2029,SC2046
    ssh $(ssh_opts) "$(ssh_target)" \
        "IMAGE=$(printf '%q' "$LOCAL_IMAGE") \
         CONTAINER_NAME=$(printf '%q' "$CONTAINER_NAME") \
         HOST_PORT=$(printf '%q' "$HOST_PORT") \
         CONTAINER_PORT=$(printf '%q' "$CONTAINER_PORT") \
         ENV_FILE=$(printf '%q' "$REMOTE_ENV_FILE") \
         SECRETS_DIR=$(printf '%q' "$REMOTE_SECRETS_DIR") \
         HEALTH_PATH=$(printf '%q' "$HEALTH_PATH") \
         bash -s" <<'REMOTE_SCRIPT'
set -euo pipefail

echo "→ Pre-flight: env file ($ENV_FILE)"
[ -f "$ENV_FILE" ] || { echo "[FATAL] $ENV_FILE missing"; exit 1; }

echo "→ Pre-flight: secrets dir ($SECRETS_DIR)"
[ -d "$SECRETS_DIR" ] || { echo "[FATAL] $SECRETS_DIR missing"; exit 1; }

echo "→ Verifying new image present locally..."
docker image inspect "$IMAGE" >/dev/null || { echo "[FATAL] $IMAGE not loaded on server"; exit 1; }

echo "→ Saving rollback marker..."
PREV="$(docker inspect --format '{{.Config.Image}}' "$CONTAINER_NAME" 2>/dev/null || echo none)"
echo "$PREV" > /tmp/ashare-previous-image.txt
echo "  Previous: $PREV"

echo "→ Stopping old container (if any)..."
docker stop "$CONTAINER_NAME" 2>/dev/null || true
docker rm   "$CONTAINER_NAME" 2>/dev/null || true

echo "→ Starting new container: $IMAGE"
docker run -d \
    --name "$CONTAINER_NAME" \
    --restart unless-stopped \
    --env-file "$ENV_FILE" \
    -v "$SECRETS_DIR:$SECRETS_DIR:ro" \
    -p "$HOST_PORT:$CONTAINER_PORT" \
    "$IMAGE"

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
    echo "[FATAL] Health check failed after 60s. Container logs:"
    docker logs --tail 60 "$CONTAINER_NAME" || true
    echo
    echo "Rollback:  ./deploy-alibaba.sh rollback"
    exit 1
fi

echo "→ Pruning dangling images..."
docker image prune -f >/dev/null

echo
echo "→ Final state:"
docker ps --filter "name=$CONTAINER_NAME" --format "table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}"
REMOTE_SCRIPT
}

# === Rollback to previous image (must still be on server) ===
rollback() {
    preflight
    log_step "Reading previous image marker"
    # shellcheck disable=SC2046
    PREV=$(ssh $(ssh_opts) "$(ssh_target)" "cat /tmp/ashare-previous-image.txt 2>/dev/null || echo none")
    if [ -z "$PREV" ] || [ "$PREV" = "none" ]; then
        log_error "No rollback marker found"
        exit 1
    fi
    log_info "Previous image: $PREV"

    # Check image still exists on server
    # shellcheck disable=SC2046,SC2029
    ssh $(ssh_opts) "$(ssh_target)" "docker image inspect $PREV >/dev/null 2>&1" || {
        log_error "Image $PREV no longer present on server (was pruned). Re-upload it first."
        exit 1
    }

    LOCAL_IMAGE="$PREV"
    deploy_container
    log_info "Rollback complete"
}

usage() {
    cat <<HELP
Ashare API — Fast SSH-streaming Deployment

Usage:
  source ./deploy-alibaba.env
  ./deploy-alibaba.sh <command>

Commands:
  build       Build image only (tags: <git-sha> + latest)
  upload      Build + stream image to server (no restart)
  deploy      Full flow: build → upload → restart → health check
  restart     Restart container with already-loaded LOCAL_IMAGE on server
  rollback    Switch back to previous image (if still on server)
  ssh         Open interactive SSH
  logs        Tail container logs (follow)
  status      Container state + last 30 log lines

Current settings:
  Git SHA:  ${GIT_SHA}${GIT_DIRTY}
  Image:    ${LOCAL_IMAGE}
  Target:   ${ECS_USER}@${ECS_HOST:-<unset>}
  Health:   http://localhost:${HOST_PORT}${HEALTH_PATH}

Override variables inline:
  IMAGE_TAG=a3f9c12 ./deploy-alibaba.sh restart
HELP
}

case "${1:-}" in
    build)
        preflight
        build_image
        ;;
    upload)
        preflight
        build_image
        upload_image
        ;;
    deploy)
        preflight
        build_image
        upload_image
        deploy_container
        log_info "════════════════════════════════════════════"
        log_info "Deployed: ${LOCAL_IMAGE}"
        log_info "URL: https://api.ashare.sa"
        log_info "════════════════════════════════════════════"
        ;;
    restart)
        preflight
        deploy_container
        ;;
    rollback)
        rollback
        ;;
    ssh)
        preflight
        # shellcheck disable=SC2046
        ssh $(ssh_opts) "$(ssh_target)"
        ;;
    logs)
        preflight
        # shellcheck disable=SC2046,SC2029
        ssh $(ssh_opts) "$(ssh_target)" "docker logs -f --tail 100 $CONTAINER_NAME"
        ;;
    status)
        preflight
        # shellcheck disable=SC2046,SC2029
        ssh $(ssh_opts) "$(ssh_target)" "docker ps --filter name=$CONTAINER_NAME && echo && docker logs --tail 30 $CONTAINER_NAME"
        ;;
    *)
        usage
        exit 1
        ;;
esac
