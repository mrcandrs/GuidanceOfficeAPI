FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY . .

# Copy NuGet config first
COPY NuGet.config ./
# Then restore
RUN dotnet restore /src/GuidanceOfficeAPI.sln --no-cache --force

RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GuidanceOfficeAPI.dll"]