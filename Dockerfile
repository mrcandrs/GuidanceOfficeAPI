FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Show initial state
RUN echo "=== Initial environment ===" && \
    dotnet --version && \
    pwd && \
    ls -la

# Copy everything
COPY . .

# Show what we copied
RUN echo "=== After copying files ===" && \
    ls -la && \
    echo "=== Project file content ===" && \
    cat *.csproj && \
    echo "=== Solution file ===" && \
    cat *.sln || echo "No .sln file found"

# Clean any existing build artifacts
RUN echo "=== Cleaning build artifacts ===" && \
    rm -rf obj bin || true && \
    dotnet clean || true

# Clear NuGet cache
RUN echo "=== Clearing NuGet cache ===" && \
    dotnet nuget locals all --clear

# Try restore with verbose output
RUN echo "=== Attempting restore ===" && \
    dotnet restore --verbosity diagnostic || \
    echo "Restore failed, trying with different approach"

# Check what restore created
RUN echo "=== After restore attempt ===" && \
    ls -la && \
    ls -la obj/ || echo "No obj directory" && \
    find . -name "project.assets.json" || echo "No project.assets.json found"

# Try building
RUN echo "=== Attempting build ===" && \
    dotnet build --verbosity diagnostic || \
    echo "Build failed"