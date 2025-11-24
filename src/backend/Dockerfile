# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["RentalManager.Domain/RentalManager.Domain.csproj", "RentalManager.Domain/"]
COPY ["RentalManager.Application/RentalManager.Application.csproj", "RentalManager.Application/"]
COPY ["RentalManager.Infrastructure/RentalManager.Infrastructure.csproj", "RentalManager.Infrastructure/"]
COPY ["RentalManager.API/RentalManager.API.csproj", "RentalManager.API/"]

# Restore dependencies
RUN dotnet restore "RentalManager.API/RentalManager.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/RentalManager.API"
RUN dotnet build "RentalManager.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "RentalManager.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
ENTRYPOINT ["dotnet", "RentalManager.API.dll"]
