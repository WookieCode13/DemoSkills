# EmployeeAPI Dockerfile (multi-stage build)
# Build with: docker build -f docker/services/EmployeeAPI.Dockerfile -t demo/employeeapi .

ARG DOTNET_VERSION=8.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION
WORKDIR /src

# Restore using only the project file to maximize caching.
COPY apis/EmployeeAPI/EmployeeAPI.csproj ./EmployeeAPI/
RUN dotnet restore "EmployeeAPI/EmployeeAPI.csproj"

# Copy the remaining source and publish.
COPY apis/EmployeeAPI/. ./EmployeeAPI/
WORKDIR /src/EmployeeAPI
RUN dotnet publish "EmployeeAPI.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmployeeAPI.dll"]
