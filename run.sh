#!/bin/bash
set -euo pipefail

Xvfb :1 -screen 0 1024x768x16 &
echo "[Xvfb] display started"

# Launch IB Gateway via IBCAlpha
/opt/ibc/IBC.sh & 
echo "[IBC] IB Gateway startup triggered"

# Wait until API port is ready
time_left=60
echo "[Gateway] Waiting for port 4001..."
while ! nc -z localhost 4001; do
  sleep 2
  ((time_left--)) || (echo "Timeout awaiting gateway" && exit 1)
done
echo "[Gateway] API port ready"

# Start trading bot
echo "[Bot] Starting FinanceMaker Worker"
dotnet /app/FinanceMaker.Worker.dll & BOT=$!

# Start health HTTP server
python3 -u -m http.server 8080 &

wait -n
echo "[Error] Process exited"
kill "$BOT" || true
exit 1