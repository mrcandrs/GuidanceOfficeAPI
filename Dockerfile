# Build stage with extensive debugging
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Debug: Show initial state
RUN echo "=== Initial environment ===" && \
    dotnet --version && \
    pwd && \
    ls -la

# Create directories
RUN mkdir -p /.nuget/packages && \
    mkdir -p /app/publish

# Set environment variables
ENV NUGET_PACKAGES=/.nuget/packages
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false

# Debug: Show all existing NuGet configs
RUN echo "=== Finding existing NuGet configs ===" && \
    find / -name "*.config" 2>/dev/null | grep -i nuget || echo "No existing NuGet configs found"

# Create clean NuGet.config
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

# Debug: Show our NuGet config
RUN echo "=== Our NuGet.config ===" && \
    cat NuGet.config

# Copy project files
COPY *.csproj ./
COPY *.sln ./

# Debug: Show copied files and project content
RUN echo "=== Copied project files ===" && \
    ls -la && \
    echo "=== Project file content ===" && \
    cat *.csproj

# Clear any cached NuGet data
RUN dotnet nuget locals all --clear

# Restore with detailed logging
RUN echo "=== Starting restore ===" && \
    dotnet restore /src/GuidanceOfficeAPI.sln \
    --packages /.nuget/packages \
    --configfile ./NuGet.config \
    --no-cache \
    --force \
    --verbosity detailed

# Debug: Verify restore worked
RUN echo "=== After restore ===" && \
    ls -la /.nuget/packages

# Copy source code
COPY . .

# Debug: Show all files
RUN echo "=== All source files ===" && \
    find . -type f -name "*.cs" | head -10

# Build with logging
RUN echo "=== Starting build ===" && \
    dotnet build /src/GuidanceOfficeAPI.sln \
    --configuration Release \
    --no-restore \
    --verbosity detailed

# Publish with logging
RUN echo "=== Starting publish ===" && \
    dotnet publish /src/GuidanceOfficeAPI.csproj \
    --configuration Release \
    --no-build \
    --output /app/publish \
    --verbosity detailed

# Verify publish directory
RUN echo "=== Published files ===" && \
    ls -la /app/publish && \
    echo "=== Published DLL check ===" && \
    ls -la /app/publish/*.dll

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy from build stage
COPY --from=build /app/publish .

# Final verification
RUN echo "=== Final app directory ===" && \
    ls -la

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "GuidanceOfficeAPI.dll"]