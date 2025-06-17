# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["JewelryStore.API/JewelryStore.API.csproj", "JewelryStore.API/"]
COPY ["JewelryStore.Core/JewelryStore.Core.csproj", "JewelryStore.Core/"]
COPY ["JewelryStore.Infrastructure/JewelryStore.Infrastructure.csproj", "JewelryStore.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "JewelryStore.API/JewelryStore.API.csproj"

# Copy source code
COPY . .

# Publish the application
WORKDIR "/src/JewelryStore.API"
RUN dotnet publish "JewelryStore.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

# Expose port
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "JewelryStore.API.dll"] 