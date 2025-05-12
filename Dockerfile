# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything
COPY . ./

# Go to your main project folder
WORKDIR /src/FinanceMaker.Worker

# Restore and publish the main project
RUN dotnet restore
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "FinanceMaker.Worker.dll"]