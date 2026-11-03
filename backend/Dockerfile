
# Bygger applikasjonen med .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY backend.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Kjører applikasjonen med ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "backend.dll"]
