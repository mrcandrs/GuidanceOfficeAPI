FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY . .

# Create the directory that's being referenced
RUN mkdir -p /.nuget/packages

# Set environment variables to override NuGet behavior
ENV NUGET_PACKAGES=/.nuget/packages
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false

# Copy NuGet config
COPY NuGet.config ./

# Try restore with explicit package directory
RUN dotnet restore /src/GuidanceOfficeAPI.sln \
    --packages /.nuget/packages \
    --configfile ./NuGet.config \
    --no-cache \
    --force \
    --verbosity detailed


FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GuidanceOfficeAPI.dll"]