# 1. Build the C# FinanceMaker.Worker project using .NET SDK 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY FinanceMaker.Worker/*.csproj ./FinanceMaker.Worker/
RUN dotnet restore ./FinanceMaker.Worker/FinanceMaker.Worker.csproj
COPY . .
RUN dotnet publish ./FinanceMaker.Worker/FinanceMaker.Worker.csproj -c Release -o /app/publish

# 2. Final runtime image: Ubuntu with Java, .NET, Python, IBController, IB Gateway
FROM ubuntu:22.04

# Suppress interactive prompts
ENV DEBIAN_FRONTEND=noninteractive \
    DISPLAY=:1 \
    PATH="/root/.dotnet:$PATH"

# Install system dependencies: Java (for IB Gateway), Python (for HTTP server), etc.
RUN apt-get update && apt-get install -y \
    openjdk-17-jre \
    wget curl unzip python3 xvfb && \
    rm -rf /var/lib/apt/lists/*

# Install .NET runtime manually into /root/.dotnet
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o /dotnet-install.sh && \
    chmod +x /dotnet-install.sh && \
    ./dotnet-install.sh --runtime dotnet && \
    echo 'export PATH=$PATH:/root/.dotnet' >> /etc/profile.d/dotnet.sh

# Download and install IBController (automates IB Gateway headlessly)
RUN mkdir /opt/ibcontroller && cd /opt/ibcontroller && \
    wget https://github.com/ib-controller/ib-controller/releases/download/v5.0.0/ibcontroller-linux-x64.zip && \
    unzip ibcontroller-linux-x64.zip && rm ibcontroller-linux-x64.zip

# Download and install IB Gateway in unattended mode
RUN mkdir /ibgateway && cd /ibgateway && \
    wget https://download2.interactivebrokers.com/installers/ibgateway/stable-standalone/ibgateway-stable-standalone-linux-x64.sh && \
    chmod +x ibgateway-stable-standalone-linux-x64.sh && \
    ./ibgateway-stable-standalone-linux-x64.sh --mode unattended || \
      (echo "Gateway install failed" && exit 1)

# Copy the C# publish output from build stage into this image
WORKDIR /app
COPY --from=build /app/publish ./

# Copy the container startup script and make it executable
COPY run.sh /run.sh
RUN chmod +x /run.sh

# Expose port 8080 (used by dummy HTTP server for Cloud Run health checks)
EXPOSE 8080

# Create a non-root user for better security
RUN useradd -m appuser
USER appuser

# Define container entrypoint
CMD ["/run.sh"]