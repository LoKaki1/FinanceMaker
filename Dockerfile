# Build your C# app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY FinanceMaker.Worker/*.csproj ./FinanceMaker.Worker/
RUN dotnet restore ./FinanceMaker.Worker/FinanceMaker.Worker.csproj
COPY . .
RUN dotnet publish ./FinanceMaker.Worker/FinanceMaker.Worker.csproj -c Release -o /app/publish

# Final image: Ubuntu + Java + .NET + IB Gateway + your bot
FROM ubuntu:22.04
ENV DEBIAN_FRONTEND=noninteractive
WORKDIR /app

# Install prerequisites: Java, dotnet, python, curl
RUN apt-get update && \
    apt-get install -y wget curl unzip openjdk-17-jre python3 && \
    wget https://dot.net/v1/dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 9.0 && \
    echo 'export PATH=$PATH:/root/.dotnet' >> ~/.bashrc

ENV PATH=$PATH:/root/.dotnet

# Download and install IB Gateway
RUN mkdir /ibgateway && cd /ibgateway && \
    wget https://download2.interactivebrokers.com/installers/ibgateway/stable-standalone/ibgateway-stable-standalone-linux-x64.sh && \
    chmod +x ibgateway-stable-standalone-linux-x64.sh && \
    ./ibgateway-stable-standalone-linux-x64.sh --mode unattended

# Copy C# app and run script
COPY --from=build /app/publish /app
COPY run.sh /run.sh
RUN chmod +x /run.sh

EXPOSE 8080
CMD ["/run.sh"]