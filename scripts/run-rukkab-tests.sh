#!/usr/bin/env bash
set -euo pipefail

# Test-only runner for Rukkab services.
# Does NOT start or stop services. Assumes Driver and Rider are already running.
# Performs: request -> optional import attempts -> accept -> arrive -> start -> complete -> rate
# Usage: ./scripts/run-rukkab-tests.sh

ROOT_DIR=$(cd "$(dirname "$0")/.." && pwd)
LOG_DIR="$ROOT_DIR/logs/rukkab"
mkdir -p "$LOG_DIR"

DRIVER_URL=${DRIVER_URL:-http://127.0.0.1:5002}
RIDER_URL=${RIDER_URL:-http://127.0.0.1:5001}
DRIVER_LOG="$LOG_DIR/driver.log"
RIDER_LOG="$LOG_DIR/rider.log"

function http_post_strict() {
  local url=$1
  local data=$2
  local desc=${3:-}
  echo "--> POST $url ${desc:+($desc)}" >&2
  tmp=$(mktemp)
  status=$(curl -sS -w "%{http_code}" -o "$tmp" -X POST "$url" -H 'Content-Type: application/json' -d "$data" 2>/dev/null || echo "000")
  body=$(cat "$tmp" 2>/dev/null || true)
  rm -f "$tmp"
  echo "HTTP $status" >&2
  echo "Response body: $body" >&2
  if [[ "$status" != 2* ]]; then
    echo "Request to $url failed (status=$status). Showing recent logs and exiting." >&2
    echo "--- rider.log (tail 80) ---" >&2; tail -n 80 "$RIDER_LOG" || true
    echo "--- driver.log (tail 80) ---" >&2; tail -n 80 "$DRIVER_LOG" || true
    exit 1
  fi
  printf '%s' "$body"
}

echo "=== Running test-only flow against $RIDER_URL and $DRIVER_URL ==="

REQ_BODY='{"pickupLocation":{"latitude":40.720,"longitude":-73.995},"vehicleType":"car"}'
REQ_HEADERS=$'X-Rider-Id: rider-1'

echo "Requesting ride from Rider API"
ride_response=$(http_post_strict "$RIDER_URL/api/rides/request" "$REQ_BODY" "create ride")
echo "Rider response: $ride_response"

ride_id=$(printf '%s' "$ride_response" | python3 - <<PY || true
import sys, json
try:
  j=json.load(sys.stdin)
  print(j.get('id') or j.get('rideId') or j.get('ride',{}).get('id',''))
except Exception:
  print('')
PY
)

if [ -z "$ride_id" ]; then
  echo "Failed to obtain ride id from rider response; aborting. Saved response to $LOG_DIR/ride_response.json"
  printf '%s' "$ride_response" > "$LOG_DIR/ride_response.json"
  exit 1
fi

echo "Ride id: $ride_id"

IMPORT_URL="$DRIVER_URL/api/internal/debug/import-ride-raw"
echo "Attempting import (best-effort) via $IMPORT_URL"
import_ok=0
for i in 1 2 3; do
  status=$(curl -sS -o /dev/null -w "%{http_code}" -X POST "$IMPORT_URL" -H 'Content-Type: application/json' -d "$ride_response" || echo "000")
  echo "Import attempt $i -> HTTP $status" >&2
  if [[ "$status" == "200" ]]; then
    import_ok=1
    break
  fi
  sleep 0.2
done
if [[ "$import_ok" != "1" ]]; then
  echo "Import endpoint not available or returned non-200. Proceeding; driver may fetch ride on accept." >&2
fi

DRIVER_ID="driver-1"
echo "Driver accepting ride"
http_post_strict "$DRIVER_URL/api/assignments/$ride_id/accept?driverId=$DRIVER_ID" '{}' "driver accept"
sleep 1
echo "Driver arrive"
http_post_strict "$DRIVER_URL/api/assignments/$ride_id/arrive?driverId=$DRIVER_ID" '{}' "driver arrive"
sleep 1
echo "Driver start"
http_post_strict "$DRIVER_URL/api/assignments/$ride_id/start?driverId=$DRIVER_ID" '{}' "driver start"
sleep 1
echo "Driver complete"
http_post_strict "$DRIVER_URL/api/assignments/$ride_id/complete?driverId=$DRIVER_ID" '{}' "driver complete"
sleep 1

RATING_BODY='{"rating":5,"feedback":"Great ride"}'
echo "Rider posting rating"
http_post_strict "$RIDER_URL/api/rides/$ride_id/rating" "$RATING_BODY" "rider rating"

echo "Test-only flow complete. Tailing recent logs:"
echo "--- driver.log (tail 80) ---"
tail -n 80 "$DRIVER_LOG" || true
echo "--- rider.log (tail 80) ---"
tail -n 80 "$RIDER_LOG" || true
