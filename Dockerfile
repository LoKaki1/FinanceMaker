# ---------------------------
# Build Stage for C# app
# ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FinanceMaker.Worker/*.csproj ./FinanceMaker.Worker/
RUN dotnet restore ./FinanceMaker.Worker/FinanceMaker.Worker.csproj

COPY . .
RUN dotnet publish ./FinanceMaker.Worker/FinanceMaker.Worker.csproj -c Release -o /app/publish

# ---------------------------
# Final Image: Ubuntu + Java + .NET + IB Gateway + your bot
# ---------------------------
FROM ubuntu:22.04

ENV DEBIAN_FRONTEND=noninteractive
WORKDIR /app

# Install dependencies: Java (for IBKR), .NET, Python
RUN apt-get update && \
    apt-get install -y curl unzip wget openjdk-17-jre python3 && \
    wget https://dot.net/v1/dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 9.0 && \
    echo 'export PATH=$PATH:/root/.dotnet' >> ~/.bashrc

ENV PATH=$PATH:/root/.dotnet

# Download IB Gateway
RUN mkdir /ibgateway && \
    cd /ibgateway && \
    wget https://download2.interactivebrokers.com/installers/ibgateway/1010/ibgateway-linux-x64-1010.zip && \
    unzip ibgateway-linux-x64-1010.zip

# Copy C# app
COPY --from=build /app/publish /app

# Copy run.sh script
COPY run.sh /run.sh
RUN chmod +x /run.sh

EXPOSE 8080

CMD ["/run.sh"]