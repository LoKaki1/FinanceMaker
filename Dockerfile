# -----------------------------
# Build Stage – Build the C# Bot
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .                  
# Use COPY . . as requested
RUN dotnet restore FinanceMaker.Worker/FinanceMaker.Worker.csproj
RUN dotnet publish FinanceMaker.Worker/FinanceMaker.Worker.csproj -c Release -o /app/publish

# -----------------------------
# Final Image – IB Gateway + Bot
# -----------------------------
FROM ghcr.io/gnzsnz/ib-gateway:stable

ENV PATH="/root/.dotnet:$PATH"
WORKDIR /app

# Install .NET runtime & Python for health endpoint
RUN apt-get update && apt-get install -y curl python3 && \
    curl -sSL https://dot.net/v1/dotnet-install.sh -o /dotnet-install.sh && \
    chmod +x /dotnet-install.sh && \
    ./dotnet-install.sh --runtime dotnet && \
    rm -rf /var/lib/apt/lists/*

# Copy C# bot
COPY --from=build /app/publish .

# Add run script
COPY run.sh /run.sh
RUN chmod +x /run.sh

EXPOSE 8080
CMD ["/run.sh"]