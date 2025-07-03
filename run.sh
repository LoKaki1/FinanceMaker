#!/bin/bash

# Start IBKR Gateway in background
/opt/ibgateway/entrypoint.sh &

# Wait 10 seconds to let IBKR start
sleep 10

# Start C# bot
dotnet FinanceMaker.Worker.dll &

# Dummy HTTP server for Cloud Run
python3 -m http.server 8080