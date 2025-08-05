# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Create necessary directories
RUN mkdir -p /.nuget/packages

# Set environment variables for NuGet
ENV NUGET_PACKAGES=/.nuget/packages
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false

# Create a clean NuGet.config
RUN echo '<?xml version="1.0" encoding="utf-8"?>' > NuGet.config && \
    echo '<configuration>' >> NuGet.config && \
    echo '  <packageSources>' >> NuGet.config && \
    echo '    <clear />' >> NuGet.config && \
    echo '    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />' >> NuGet.config && \
    echo '  </packageSources>' >> NuGet.config && \
    echo '  <config>' >> NuGet.config && \
    echo '    <add key="globalPackagesFolder" value="/.nuget/packages" />' >> NuGet.config && \
    echo '  </config>' >> NuGet.config && \
    echo '</configuration>' >> NuGet.config

# Copy project files first (for better Docker layer caching)
COPY *.csproj ./
COPY *.sln ./

# CRITICAL: Remove any obj/bin directories that might contain Windows-specific cache
RUN find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true && \
    find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true

# Clear all NuGet caches completely
RUN dotnet nuget locals all --clear

# Restore packages with clean environment
RUN dotnet restore /src/GuidanceOfficeAPI.sln \
    --packages /.nuget/packages \
    --configfile ./NuGet.config \
    --no-cache \
    --force \
    --ignore-failed-sources

# Copy all source code
COPY . .

# Remove obj/bin again after copying source (in case they were copied)
RUN find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true && \
    find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true

# Build the project with clean slate
RUN dotnet build /src/GuidanceOfficeAPI.sln \
    --configuration Release \
    --no-restore \
    --force

# Publish the application
RUN dotnet publish /src/GuidanceOfficeAPI.csproj \
    --configuration Release \
    --no-build \
    --output /app/publish

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