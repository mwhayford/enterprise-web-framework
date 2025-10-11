# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["Core.Domain/Core.Domain.csproj", "Core.Domain/"]
COPY ["Core.Application/Core.Application.csproj", "Core.Application/"]
COPY ["Core.Infrastructure/Core.Infrastructure.csproj", "Core.Infrastructure/"]
COPY ["Core.API/Core.API.csproj", "Core.API/"]

# Restore dependencies
RUN dotnet restore "Core.API/Core.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/Core.API"
RUN dotnet build "Core.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Core.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 9 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=publish /app/publish .

# Expose the port
EXPOSE 80
EXPOSE 443

# Set the entry point
ENTRYPOINT ["dotnet", "Core.API.dll"]
