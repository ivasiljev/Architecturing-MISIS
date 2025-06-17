# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["JewelryStore.BlazorUI/JewelryStore.BlazorUI.csproj", "JewelryStore.BlazorUI/"]
COPY ["JewelryStore.Core/JewelryStore.Core.csproj", "JewelryStore.Core/"]

# Restore dependencies
RUN dotnet restore "JewelryStore.BlazorUI/JewelryStore.BlazorUI.csproj"

# Copy source code
COPY . .

# Publish the application
WORKDIR "/src/JewelryStore.BlazorUI"
RUN dotnet publish "JewelryStore.BlazorUI.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000

# Expose port
EXPOSE 5000

# Entry point
ENTRYPOINT ["dotnet", "JewelryStore.BlazorUI.dll"] 