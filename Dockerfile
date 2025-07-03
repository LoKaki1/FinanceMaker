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

RUN apt-get update && apt-get install -y python3 wget
# rest of your Dockerfile...
# Install .NET runtime (9.0)
RUN apt-get update && \
    apt-get install -y wget && \
    wget https://dot.net/v1/dotnet-install.sh && \
    chmod +x dotnet-install.sh && \
    ./dotnet-install.sh --channel 9.0 && \
    ln -s /root/.dotnet/dotnet /usr/bin/dotnet

# Copy published C# app
WORKDIR /app
COPY --from=build /app/publish .

# Required by Cloud Run
EXPOSE 8080

# Healthcheck workaround (Cloud Run needs something listening on 8080)
RUN apt-get install -y python3

# Entrypoint script
COPY run.sh /run.sh
RUN chmod +x /run.sh

CMD ["/run.sh"]