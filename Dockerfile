FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CryptoIntelligence.Api.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish CryptoIntelligence.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5080
EXPOSE 5080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "CryptoIntelligence.Api.dll"]
