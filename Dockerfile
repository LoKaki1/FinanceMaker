# 1. Build stage – compile C# bot
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY FinanceMaker.Worker/*.csproj FinanceMaker.Worker/
RUN dotnet restore FinanceMaker.Worker/FinanceMaker.Worker.csproj
COPY . . 
RUN dotnet publish FinanceMaker.Worker/FinanceMaker.Worker.csproj -c Release -o /app/publish

# 2. Runtime environment
FROM ubuntu:22.04

ENV DEBIAN_FRONTEND=noninteractive \
    DISPLAY=:1 \
    PATH="/root/.dotnet:$PATH"

RUN apt-get update && apt-get install -y \
    openjdk-17-jre wget curl unzip python3 xvfb && \
    rm -rf /var/lib/apt/lists/*

# Install .NET runtime
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o /dotnet-install.sh && \
    chmod +x /dotnet-install.sh && \
    ./dotnet-install.sh --runtime dotnet && \
    echo 'export PATH=/root/.dotnet:$PATH' >> /etc/profile.d/dotnet.sh

# Install IBC (IBCAlpha) for headless gateway automation
RUN mkdir /opt/ibc && \
    cd /opt/ibc && \
    wget https://github.com/IbcAlpha/IBC/releases/download/v4.6.1/IBC-linux-x64.zip && \
    unzip IBC-linux-x64.zip && rm IBC-linux-x64.zip

# Download and install IB Gateway headlessly
RUN wget https://download2.interactivebrokers.com/installers/ibgateway/stable-standalone/ibgateway-stable-standalone-linux-x64.sh && \
    chmod +x ibgateway-stable-standalone-linux-x64.sh && \
    ./ibgateway-stable-standalone-linux-x64.sh --mode unattended

# Copy compiled bot
WORKDIR /app
COPY --from=build /app/publish ./

# Copy run script
COPY run.sh /run.sh
RUN chmod +x /run.sh

EXPOSE 8080
RUN useradd -m appuser
USER appuser

CMD ["/run.sh"]