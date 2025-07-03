#!/bin/bash

# Start IB Gateway binary (not .jar)
echo "Starting IB Gateway..."
/ibgateway/ibgateway/ibgateway &

sleep 15

echo "Starting FinanceMaker bot..."
dotnet /app/FinanceMaker.Worker.dll &

# Keep Cloud Run happy
python3 -m http.server 8080