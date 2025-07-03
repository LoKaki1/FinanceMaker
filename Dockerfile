# ---------------------------
# Build Stage for C# app
# ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY FinanceMaker.Worker/*.csproj ./FinanceMaker.Worker/
RUN dotnet restore ./FinanceMaker.Worker/FinanceMaker.Worker.csproj

# Copy rest of the code
COPY . .
RUN dotnet publish ./FinanceMaker.Worker/FinanceMaker.Worker.csproj -c Release -o /app/publish

# ---------------------------
# Runtime Base: IB Gateway + .NET app
# ---------------------------
FROM ghcr.io/gnzsnz/ib-gateway:stable

USER root

# Install Python (for Cloud Run) and .NET 9 runtime
RUN apt-get update && \
    apt-get install -y wget python3 && \
    wget https://dot.net/v1/dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 9.0 && \
    echo 'export PATH=$PATH:/root/.dotnet' >> ~/.bashrc

ENV PATH=$PATH:/root/.dotnet

# Copy published C# app
WORKDIR /app
COPY --from=build /app/publish .

# Cloud Run requirement
EXPOSE 8080

# Entrypoint script
COPY run.sh /run.sh
RUN chmod +x /run.sh

CMD ["/run.sh"]