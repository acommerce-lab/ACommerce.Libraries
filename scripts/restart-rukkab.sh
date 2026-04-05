#!/usr/bin/env bash
set -euo pipefail

# Restart runner for Rukkab services.
# - kills any existing Driver/Rider processes (pidfile or pkill fallback)
# - builds both projects
# - starts Driver and Rider, writes pid files and logs
# Usage: ./scripts/restart-rukkab.sh

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

echo "Stopping any previously-run instances (pidfile + pkill fallback)"
if [ -f "$DRIVER_PID_FILE" ]; then
  pid=$(cat "$DRIVER_PID_FILE" 2>/dev/null || true)
  if [ -n "$pid" ] && ps -p "$pid" > /dev/null 2>&1; then
    echo "Stopping driver pid $pid"
    kill -15 "$pid" || true
    sleep 1
    if ps -p "$pid" > /dev/null 2>&1; then
      echo "Forcing kill of driver pid $pid"
      kill -9 "$pid" || true
    fi
  fi
  rm -f "$DRIVER_PID_FILE"
fi
if [ -f "$RIDER_PID_FILE" ]; then
  pid=$(cat "$RIDER_PID_FILE" 2>/dev/null || true)
  if [ -n "$pid" ] && ps -p "$pid" > /dev/null 2>&1; then
    echo "Stopping rider pid $pid"
    kill -15 "$pid" || true
    sleep 1
    if ps -p "$pid" > /dev/null 2>&1; then
      echo "Forcing kill of rider pid $pid"
      kill -9 "$pid" || true
    fi
  fi
  rm -f "$RIDER_PID_FILE"
fi

# best-effort: kill any stray dotnet processes for these projects
pkill -f "Apps/Rukkab/Driver/Driver.Api" || true
pkill -f "Apps/Rukkab/Rider/Rider.Api" || true

echo "Building projects (Driver + Rider)"
dotnet build "$ROOT_DIR/$DRIVER_PROJECT" -c Debug || { echo "Driver build failed"; exit 1; }
dotnet build "$ROOT_DIR/$RIDER_PROJECT" -c Debug || { echo "Rider build failed"; exit 1; }

echo "Starting Driver on $DRIVER_URL -> $DRIVER_LOG"
nohup env ASPNETCORE_ENVIRONMENT=Development dotnet run --project "$ROOT_DIR/$DRIVER_PROJECT" --no-build --no-launch-profile --urls "$DRIVER_URL" > "$DRIVER_LOG" 2>&1 &
echo $! > "$DRIVER_PID_FILE"

sleep 1

echo "Starting Rider on $RIDER_URL -> $RIDER_LOG"
nohup env ASPNETCORE_ENVIRONMENT=Development dotnet run --project "$ROOT_DIR/$RIDER_PROJECT" --no-build --no-launch-profile --urls "$RIDER_URL" > "$RIDER_LOG" 2>&1 &
echo $! > "$RIDER_PID_FILE"

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

echo "Driver log tail (last 20 lines):"
tail -n 20 "$DRIVER_LOG" || true
echo "Rider log tail (last 20 lines):"
tail -n 20 "$RIDER_LOG" || true

echo "Restart complete. Driver PID: $(cat "$DRIVER_PID_FILE") Rider PID: $(cat "$RIDER_PID_FILE")"
