# Use the .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["FireflyApi.csproj", "./"]
RUN dotnet restore "./FireflyApi.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "FireflyApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "FireflyApi.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "FireflyApi.dll"]