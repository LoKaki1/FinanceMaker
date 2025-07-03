#!/bin/bash

# Start IB Gateway
echo "Starting IB Gateway..."
cd /ibgateway/ibgateway-linux-x64-1010
java -jar ibgateway.jar &

# Wait for it to boot
sleep 15

# Start your C# trading bot
echo "Starting FinanceMaker bot..."
dotnet /app/FinanceMaker.Worker.dll &

# Keep container alive (for Cloud Run)
python3 -m http.server 8080