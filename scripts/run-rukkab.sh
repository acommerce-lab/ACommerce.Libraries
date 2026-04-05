#!/usr/bin/env bash
set -euo pipefail

# Lightweight end-to-end runner for local Rukkab Rider+Driver APIs.
# Usage: ./scripts/run-rukkab.sh
# What it does:
#  - ensures logs dir
#  - stops previously started instances (by pid files)
#  - starts Driver (5002) and Rider (5001) with nohup, saves pids
#  - waits for swagger endpoints, then runs a simple flow:
#      1) Rider: request ride
#      2) Driver: accept, arrive, start, complete
#      3) Rider: post rating
#  - prints HTTP responses and tails small parts of logs for quick debugging

ROOT_DIR=$(cd "$(dirname "$0")/.." && pwd)
LOG_DIR="$ROOT_DIR/logs/rukkab"
mkdir -p "$LOG_DIR"

DRIVER_PROJECT="Apps/Rukkab/Driver/Driver.Api/Driver.Api.csproj"
RIDER_PROJECT="Apps/Rukkab/Rider/Rider.Api/Rider.Api.csproj"

DRIVER_URL="http://127.0.0.1:5002"
RIDER_URL="http://127.0.0.1:5001"

DRIVER_PID_FILE="$LOG_DIR/driver.pid"
RIDER_PID_FILE="$LOG_DIR/rider.pid"
DRIVER_LOG="$LOG_DIR/driver.log"
RIDER_LOG="$LOG_DIR/rider.log"

function stop_if_running() {
  local pidfile=$1
  if [ -f "$pidfile" ]; then
    pid=$(cat "$pidfile" 2>/dev/null || true)
    if [ -n "$pid" ] && ps -p "$pid" > /dev/null 2>&1; then
      echo "Stopping pid $pid (from $pidfile)"
      kill -15 "$pid" || true
      sleep 1
      if ps -p "$pid" > /dev/null 2>&1; then
        echo "Pid $pid still alive, killing"
        kill -9 "$pid" || true
      fi
    fi
    rm -f "$pidfile"
  fi
}

echo "Stopping any previously-run instances via pid files"
stop_if_running "$DRIVER_PID_FILE"
stop_if_running "$RIDER_PID_FILE"

echo "Building projects before launch (ensure changes are compiled)"
echo "Building Driver.Api..."
dotnet build "$ROOT_DIR/$DRIVER_PROJECT" -c Debug || { echo "Driver build failed; check output"; exit 1; }
echo "Building Rider.Api..."
dotnet build "$ROOT_DIR/$RIDER_PROJECT" -c Debug || { echo "Rider build failed; check output"; exit 1; }

echo "Starting Driver on $DRIVER_URL -> $DRIVER_LOG (ASPNETCORE_ENVIRONMENT=Development)"
nohup env ASPNETCORE_ENVIRONMENT=Development dotnet run --project "$ROOT_DIR/$DRIVER_PROJECT" --no-build --no-launch-profile --urls "$DRIVER_URL" > "$DRIVER_LOG" 2>&1 & echo $! > "$DRIVER_PID_FILE"
sleep 1

echo "Starting Rider on $RIDER_URL -> $RIDER_LOG (ASPNETCORE_ENVIRONMENT=Development)"
nohup env ASPNETCORE_ENVIRONMENT=Development dotnet run --project "$ROOT_DIR/$RIDER_PROJECT" --no-build --no-launch-profile --urls "$RIDER_URL" > "$RIDER_LOG" 2>&1 & echo $! > "$RIDER_PID_FILE"


echo "Waiting for services to bind (up to 20s)"
for i in $(seq 1 20); do
  ok=0
  if curl -sSfI "$RIDER_URL/swagger/index.html" >/dev/null 2>&1; then ok=$((ok+1)); fi
  if curl -sSfI "$DRIVER_URL/swagger/index.html" >/dev/null 2>&1; then ok=$((ok+1)); fi
  if [ "$ok" -eq 2 ]; then
    echo "Both swagger endpoints responding"
    break
  fi
  sleep 1
done

echo
echo "Listener check:"
lsof -nP -iTCP:5001,5002 -sTCP:LISTEN || true

echo
echo "Driver log tail (last 10 lines):"
tail -n 10 "$DRIVER_LOG" || true

echo
echo "Rider log tail (last 10 lines):"
tail -n 10 "$RIDER_LOG" || true

# Helper: http POST and show status+body
function http_post() {
  local url=$1
  local data=$2
  echo "POST $url ->"
  curl -sS -X POST "$url" -H 'Content-Type: application/json' -d "$data" || true
  echo -e "\n---\n"
}

# Strict POST helper: prints status + body, tails logs on failure and exits non-zero.
function http_post_strict() {
  local url=$1
  local data=$2
  local desc=${3:-}
  local headers=${4:-}
  echo "--> POST $url ${desc:+($desc)}" >&2
  tmp=$(mktemp)
  # Build curl header args
  header_args=( -H 'Content-Type: application/json' )
  if [ -n "$headers" ]; then
    # allow multiple headers separated by '\n'
    IFS=$'\n' read -rd '' -a hdrs <<<"$headers" || true
    for h in "${hdrs[@]}"; do
      header_args+=( -H "$h" )
    done
  fi
  status=$(curl -sS -w "%{http_code}" -o "$tmp" -X POST "$url" "${header_args[@]}" -d "$data" 2>/dev/null || echo "000")
  body=$(cat "$tmp" 2>/dev/null || true)
  rm -f "$tmp"
  echo "HTTP $status" >&2
  echo "Response body: $body" >&2
  if [[ "$status" != 2* ]]; then
    echo "\nRequest to $url failed (status=$status). Showing recent logs and exiting." >&2
    echo "--- rider.log (tail 80) ---" >&2; tail -n 80 "$RIDER_LOG" || true
    echo "--- driver.log (tail 80) ---" >&2; tail -n 80 "$DRIVER_LOG" || true
    exit 1
  fi
  # print body to stdout for capture (logs and status were printed to stderr)
  printf '%s' "$body"
}

echo
echo "=== Running ride flow ==="

# 1) Rider requests a ride
# Use camel-case latitude/longitude and include X-Rider-Id header so server receives rider id
REQ_BODY='{"pickupLocation":{"latitude":40.720,"longitude":-73.995},"vehicleType":"car"}'
REQ_HEADERS=$'X-Rider-Id: rider-1'
echo "Requesting ride from Rider API"
# Use strict POST so we fail fast and print logs if something is wrong (pass X-Rider-Id header)
ride_response=$(http_post_strict "$RIDER_URL/api/rides/request" "$REQ_BODY" "create ride" "$REQ_HEADERS")
echo "Rider response: $ride_response"

# Save raw response for debugging
tmp_resp="$LOG_DIR/ride_response.json"
printf '%s' "$ride_response" > "$tmp_resp"

echo "Rider response: $ride_response"

# Robustly extract ride id using jq (falls back to rideId or ride.id). If jq isn't available, try python as fallback.
ride_id=""
if command -v jq >/dev/null 2>&1; then
  ride_id=$(printf '%s' "$ride_response" | jq -r '.id // .rideId // .ride.id // empty' 2>/dev/null || true)
else
  ride_id=$(python3 - <<PY
import sys, json
try:
    j=json.load(sys.stdin)
except Exception:
    print("")
    sys.exit(0)
print(j.get('id') or j.get('rideId') or j.get('ride',{}).get('id',''))
PY
  <<<"$ride_response")
fi

if [ -z "$ride_id" ]; then
  echo "(debug) saved response to $tmp_resp"
fi

if [ -z "$ride_id" ]; then
  echo "Failed to obtain ride id from rider response; aborting flow. Check logs in $LOG_DIR"
  exit 1
fi

echo "Ride id: $ride_id"

sleep 1

## NOTE: we used to import the ride into the Driver service here via a debug endpoint,
## but the Driver now attempts to fetch and import the ride from Rider on accept when
## needed. So we don't need to call an import endpoint from the runner anymore.

# For local deterministic demos, reliably import the created ride into the Driver
# in case the Driver process has a separate in-memory store. Try a few times until
# the debug import endpoint responds 200. This keeps the end-to-end run stable.
IMPORT_URL="$DRIVER_URL/api/internal/debug/import-ride-raw"
echo "Importing ride into Driver via $IMPORT_URL"
import_ok=0
for i in 1 2 3 4 5; do
  status=$(curl -sS -o /dev/null -w "%{http_code}" -X POST "$IMPORT_URL" -H 'Content-Type: application/json' -d "$ride_response" || echo "000")
  echo "Import attempt $i -> HTTP $status" >&2
  if [[ "$status" == "200" ]]; then
    import_ok=1
    break
  fi
  sleep 0.2
done
if [[ "$import_ok" != "1" ]]; then
  echo "Warning: could not import ride into Driver (import endpoint may be unavailable). Proceeding to accept and relying on Driver fallback." >&2
fi

# 2) Driver accepts (driverId required as query param)
DRIVER_ID="driver-1"
echo "Driver accepting ride"
http_post_strict "$DRIVER_URL/api/assignments/$ride_id/accept?driverId=$DRIVER_ID" '{}' "driver accept"
echo
sleep 1

# 3) Driver arrives
echo "Driver marking arrival"
http_post_strict "$DRIVER_URL/api/assignments/$ride_id/arrive?driverId=$DRIVER_ID" '{}' "driver arrive"
echo
sleep 1

# 4) Driver starts ride
echo "Driver starting ride"
http_post_strict "$DRIVER_URL/api/assignments/$ride_id/start?driverId=$DRIVER_ID" '{}' "driver start"
echo
sleep 1

# 5) Driver completes ride
echo "Driver completing ride"
http_post_strict "$DRIVER_URL/api/assignments/$ride_id/complete?driverId=$DRIVER_ID" '{}' "driver complete"
echo
sleep 1

# 6) Rider rates the ride
RATING_BODY='{"rating":5,"feedback":"Great ride"}'
echo "Rider posting rating"
# include X-Rider-Id header so rating is accepted
http_post_strict "$RIDER_URL/api/rides/$ride_id/rating" "$RATING_BODY" "rider rating" "$REQ_HEADERS"
echo

echo "Flow finished — printing final log tails and pid files"
echo "Driver PID: " $(cat "$DRIVER_PID_FILE" 2>/dev/null || true)
echo "Rider PID:  " $(cat "$RIDER_PID_FILE" 2>/dev/null || true)

echo "--- driver.log (tail 80) ---"
tail -n 80 "$DRIVER_LOG" || true

echo "--- rider.log (tail 80) ---"
tail -n 80 "$RIDER_LOG" || true

echo "Run complete. If you want to stop the services later:"
echo "  kill \$(cat $DRIVER_PID_FILE) \$(cat $RIDER_PID_FILE)"
#!/usr/bin/env zsh
set -euo pipefail

# Simple runner to start both Rukkab backend APIs and stream their logs.
# Usage: ./scripts/run-rukkab.sh

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
LOGDIR="$ROOT_DIR/logs/rukkab"
mkdir -p "$LOGDIR"
DRIVER_LOG="$LOGDIR/driver.log"
RIDER_LOG="$LOGDIR/rider.log"

echo "Starting Rukkab services - logs -> $LOGDIR"

echo "=== Driver starting: $(date -u) ===" > "$DRIVER_LOG"
echo "=== Rider starting: $(date -u) ===" > "$RIDER_LOG"

# Kill any existing runs of these projects (avoid address-in-use)
echo "Cleaning up any existing Rukkab dotnet processes..."
pkill -f "Apps/Rukkab/Driver/Driver.Api" || true
pkill -f "Apps/Rukkab/Rider/Rider.Api" || true
sleep 1

# Give the OS a moment and make sure ports are free
echo "Waiting for ports to be released..."
sleep 1

# Build projects sequentially to avoid concurrent build / file lock races
echo "Building Driver.Api..."
dotnet build "$ROOT_DIR/Apps/Rukkab/Driver/Driver.Api/Driver.Api.csproj" -c Debug || { echo "Driver build failed; check logs"; exit 1; }

echo "Building Rider.Api..."
dotnet build "$ROOT_DIR/Apps/Rukkab/Rider/Rider.Api/Rider.Api.csproj" -c Debug || { echo "Rider build failed; check logs"; exit 1; }

# Start Driver API in background on port 5001 (use --no-build to avoid race)

# helper: find free port (start at $1 and increment until free)
find_free_port() {
  local p="$1"
  while lsof -nP -iTCP:$p -sTCP:LISTEN >/dev/null 2>&1; do
    p=$((p + 1))
  done
  echo "$p"
}

# pick ports dynamically to avoid system conflicts
RIDER_PORT=$(find_free_port 5000)
# ensure driver port is different from rider
DRIVER_PORT=$(find_free_port $((RIDER_PORT + 1)))

echo "Using ports: Rider=$RIDER_PORT Driver=$DRIVER_PORT"

# Start Driver API in background (use --no-build to avoid race)
env PORT=$DRIVER_PORT ASPNETCORE_URLS="http://127.0.0.1:$DRIVER_PORT" ASPNETCORE_ENVIRONMENT=Development EnableSwagger=true \
  dotnet run --project "$ROOT_DIR/Apps/Rukkab/Driver/Driver.Api/Driver.Api.csproj" --no-build --no-launch-profile > "$DRIVER_LOG" 2>&1 &
DRIVER_PID=$!

echo "Driver started (pid $DRIVER_PID) -> http://127.0.0.1:$DRIVER_PORT"

# Start Rider API in background (use --no-build to avoid race)
env PORT=$RIDER_PORT ASPNETCORE_URLS="http://127.0.0.1:$RIDER_PORT" ASPNETCORE_ENVIRONMENT=Development EnableSwagger=true \
  dotnet run --project "$ROOT_DIR/Apps/Rukkab/Rider/Rider.Api/Rider.Api.csproj" --no-build --no-launch-profile > "$RIDER_LOG" 2>&1 &
RIDER_PID=$!

echo "Rider started (pid $RIDER_PID) -> http://127.0.0.1:$RIDER_PORT"

# Ensure child processes are killed when this script is interrupted
cleanup() {
  echo "Stopping Rukkab services..."
  kill $DRIVER_PID $RIDER_PID 2>/dev/null || true
  wait 2>/dev/null || true
}
trap cleanup INT TERM EXIT

# Tail both logs (tail will block and stream logs)
# -n +1 ensures we start from beginning so you see startup logs immediately
exec tail -n +1 -f "$DRIVER_LOG" "$RIDER_LOG"
