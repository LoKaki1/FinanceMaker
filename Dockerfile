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
# Install Java, .NET runtime, and required tools
RUN apt-get update && apt-get install -y \
    openjdk-17-jre wget curl unzip python3 xvfb netcat && \
    rm -rf /var/lib/apt/lists/*

RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o /dotnet-install.sh && \
    chmod +x /dotnet-install.sh && \
    ./dotnet-install.sh --runtime dotnet && \
    echo 'export PATH=/root/.dotnet:$PATH' >> /etc/profile.d/dotnet.sh

# Install IBCAlpha using the provided correct URL
RUN mkdir /opt/ibc && cd /opt/ibc && \
    wget https://github.com/IbcAlpha/IBC/releases/download/3.23.0/IBCLinux-3.23.0.zip && \
    unzip IBCLinux-3.23.0.zip && rm IBCLinux-3.23.0.zip

# Download and install IB Gateway silently
RUN wget https://download2.interactivebrokers.com/installers/ibgateway/stable-standalone/ibgateway-stable-standalone-linux-x64.sh && \
    chmod +x ibgateway-stable-standalone-linux-x64.sh && \
    ./ibgateway-stable-standalone-linux-x64.sh --mode unattended

WORKDIR /app
COPY --from=build /app/publish ./

COPY run.sh /run.sh
RUN chmod +x /run.sh

EXPOSE 8080
RUN useradd -m appuser
USER appuser

CMD ["/run.sh"]