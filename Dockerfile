# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN cp appsettings.example.json appsettings.json
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Copy firebase credentials
COPY firebase-hml.json ./
COPY firebase.json ./

# Copy build output from build stage
COPY --from=publish /app/publish .

# Set environment variable for Firebase credentials
ENV GOOGLE_APPLICATION_CREDENTIALS=/app/firebase.json

# Expose port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["dotnet", "GupyIntegration.dll"] 