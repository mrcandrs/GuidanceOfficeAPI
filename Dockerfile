FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY . .

# Use the default nuget source and skip custom config
RUN dotnet restore /src/GuidanceOfficeAPI.sln \
    --source "https://api.nuget.org/v3/index.json" \
    --no-cache \
    --force \
    --ignore-failed-sources


RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GuidanceOfficeAPI.dll"]