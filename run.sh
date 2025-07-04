#!/bin/bash

# Exit immediately on error, undefined var, or failed pipeline
set -euo pipefail

# Start a virtual display required by IB Gateway/IBController
Xvfb :1 -screen 0 1024x768x16 &
echo "🖥 Xvfb started"

# Launch IB Gateway using IBController automation (in background)
# Logs are redirected to a file in /app
/opt/ibcontroller/ibcontroller.sh -g >/app/ibcontroller.log 2>&1 &
echo "🔐 IBController + IB Gateway started"

# Wait until the Gateway is ready (port 4001 open) or timeout
echo "⏳ Waiting for Gateway socket..."
timeout=60
while ! netstat -tln | grep -q ':4001'; do
  sleep 2
  ((timeout--)) || (echo "❌ Gateway socket not ready in time" && exit 1)
done
echo "✅ Gateway socket is open"

# Start the C# FinanceMaker bot in background
echo "🤖 Starting FinanceMaker bot..."
dotnet /app/FinanceMaker.Worker.dll &
BOT_PID=$!

# Start a basic HTTP server for health checks (port 8080)
python3 -u -m http.server 8080 &

# Wait for any process to exit (Gateway or bot)
wait -n

# Clean up gracefully if something crashes
echo "⚠️ One of the processes exited; stopping bot..."
kill "$BOT_PID" || true
exit 1