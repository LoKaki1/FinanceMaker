# 1. Build C# app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY FinanceMaker.Worker/*.csproj ./FinanceMaker.Worker/
RUN dotnet restore ./FinanceMaker.Worker/FinanceMaker.Worker.csproj
COPY . .
RUN dotnet publish ./FinanceMaker.Worker/FinanceMaker.Worker.csproj -c Release -o /app/publish

# 2. Runtime image: Base off headless IBKR image
FROM ghcr.io/gnzsnz/ib-gateway:stable

USER root

# Install .NET runtime and python
RUN apt-get update && \
    apt-get install -y wget python3 && \
    wget https://dot.net/v1/dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 9.0 && \
    echo 'export PATH=$PATH:/root/.dotnet' >> ~/.bashrc

ENV PATH=$PATH:/root/.dotnet

# Copy the bot
WORKDIR /app
COPY --from=build /app/publish .

# Health check port
EXPOSE 8080

# Use entrypoint provided by gnzsnz + start your bot
# Then HTTP server
COPY run.sh /run.sh
RUN chmod +x /run.sh
CMD ["/run.sh"]