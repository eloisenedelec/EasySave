FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# On copie d'abord les fichiers .csproj pour restaurer les dépendances
# Le joker (*) permet de trouver les fichiers peu importe le dossier
COPY ["LogCentralizationService/LogCentralizationService.csproj", "LogCentralizationService/"]
COPY ["EasyLog.Contracts/EasyLog.Contracts.csproj", "EasyLog.Contracts/"]

RUN dotnet restore "LogCentralizationService/LogCentralizationService.csproj"

# Maintenant on copie tout le reste
COPY . .
WORKDIR "/src/LogCentralizationService"
RUN dotnet publish "LogCentralizationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LogCentralizationService.dll"]