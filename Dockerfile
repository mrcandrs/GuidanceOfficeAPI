# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# Clean any existing build artifacts
RUN rm -rf obj bin || true && \
    dotnet clean || true

# Create the NuGet packages directory that's being referenced
RUN mkdir -p /.nuget/packages

# Clear NuGet cache
RUN dotnet nuget locals all --clear

# Create a clean NuGet.config to override any problematic settings
RUN echo '<?xml version="1.0" encoding="utf-8"?>' > nuget.config && \
    echo '<configuration>' >> nuget.config && \
    echo '  <packageSources>' >> nuget.config && \
    echo '    <clear />' >> nuget.config && \
    echo '    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />' >> nuget.config && \
    echo '  </packageSources>' >> nuget.config && \
    echo '</configuration>' >> nuget.config

# Set NuGet environment variables
ENV NUGET_PACKAGES=/.nuget/packages

# Restore packages using our clean config
RUN dotnet restore --configfile nuget.config --packages /.nuget/packages

# Build the application
RUN dotnet build --configuration Release --no-restore

# Publish the application
RUN dotnet publish --configuration Release --no-build --output /app/publish

# Runtime stage - Use .NET 6.0 runtime to match your application
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

# Copy the published application from build stage
COPY --from=build /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

# Set the entry point
ENTRYPOINT ["dotnet", "GuidanceOfficeAPI.dll"]