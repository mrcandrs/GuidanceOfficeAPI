# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Set up clean NuGet environment
ENV NUGET_PACKAGES=/nuget
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false

# Create NuGet packages directory
RUN mkdir -p /nuget

# Clear any existing NuGet configuration
RUN dotnet nuget locals all --clear || true

# Create minimal NuGet.config
RUN echo '<?xml version="1.0" encoding="utf-8"?>' > nuget.config && \
    echo '<configuration>' >> nuget.config && \
    echo '  <packageSources>' >> nuget.config && \
    echo '    <clear />' >> nuget.config && \
    echo '    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />' >> nuget.config && \
    echo '  </packageSources>' >> nuget.config && \
    echo '</configuration>' >> nuget.config

# Copy project file(s) for restore
COPY *.csproj ./
COPY *.sln ./

# Restore packages using the solution file
RUN dotnet restore GuidanceOfficeAPI.sln \
    --configfile nuget.config \
    --packages /nuget \
    --runtime linux-x64

# Copy the rest of the source code
COPY . ./

# Build the application
RUN dotnet build GuidanceOfficeAPI.sln \
    --configuration Release \
    --no-restore

# Publish the application
RUN dotnet publish GuidanceOfficeAPI.csproj \
    --configuration Release \
    --no-build \
    --runtime linux-x64 \
    --self-contained false \
    --output /app/out

# Final stage - runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser

# Copy the published application
COPY --from=build /app/out .

# Change ownership to the app user
RUN chown -R appuser:appuser /app
USER appuser

# Expose the port
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "GuidanceOfficeAPI.dll"]