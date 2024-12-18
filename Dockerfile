FROM ubuntu:16.04

# Install prerequisites
RUN apt-get update && apt-get install -y \
curl
CMD /bin/bash

# Use the official image as a parent image for the runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080

# Set the environment variable to listen on port 8080 .//
ENV ASPNETCORE_URLS=http://*:8080

# Use the SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy each project file and restore its dependencies
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Presistence/Presistence.csproj", "Presistence/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["KawaAssignment/KawaAssignment.csproj", "KawaAssignment/"]

# Restore dependencies for all projects
RUN dotnet restore "KawaAssignment/KawaAssignment.csproj"

# Copy the remaining source files for all projects
COPY . .

# Build the KawaAssignment project
RUN dotnet build "KawaAssignment/KawaAssignment.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "KawaAssignment/KawaAssignment.csproj" -c Release -o /app/publish

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KawaAssignment.dll"]

