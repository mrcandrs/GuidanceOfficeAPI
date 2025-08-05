# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Create necessary directories
RUN mkdir -p /.nuget/packages

# Set environment variables for NuGet
ENV NUGET_PACKAGES=/.nuget/packages
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false

# Create a clean NuGet.config
RUN cat > NuGet.config << 'EOF'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <config>
    <add key="globalPackagesFolder" value="/.nuget/packages" />
  </config>
</configuration>
EOF

# Copy project files first (for better Docker layer caching)
COPY *.csproj ./
COPY *.sln ./

# Restore packages
RUN dotnet restore /src/GuidanceOfficeAPI.sln \
    --packages /.nuget/packages \
    --configfile ./NuGet.config \
    --no-cache \
    --force \
    --verbosity normal

# Copy all source code
COPY . .

# Build the project
RUN dotnet build /src/GuidanceOfficeAPI.sln \
    --configuration Release \
    --no-restore \
    --verbosity normal

# Publish the application
RUN dotnet publish /src/GuidanceOfficeAPI.csproj \
    --configuration Release \
    --no-build \
    --output /app/publish \
    --verbosity normal

# Verify publish directory exists
RUN ls -la /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published app from build stage
COPY --from=build /app/publish .

# Expose port (adjust as needed)
EXPOSE 80
EXPOSE 443

# Set entry point
ENTRYPOINT ["dotnet", "GuidanceOfficeAPI.dll"]