FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FinanceMaker.Worker/FinanceMaker.Worker.csproj", "FinanceMaker.Worker/"]
COPY ["FinanceMaker.Broker/FinanceMaker.Broker.csproj", "FinanceMaker.Broker/"]
RUN dotnet restore "FinanceMaker.Worker/FinanceMaker.Worker.csproj"
COPY . .
WORKDIR "/src/FinanceMaker.Worker"
RUN dotnet build "FinanceMaker.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceMaker.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceMaker.Worker.dll"]