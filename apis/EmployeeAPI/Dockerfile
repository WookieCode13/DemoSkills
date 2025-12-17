# .NET 8 ASP.NET API - multi-stage build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY EmployeeAPI.csproj .
RUN dotnet restore "EmployeeAPI.csproj"

# copy everything and publish
COPY . .
RUN dotnet publish "EmployeeAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmployeeAPI.dll"]
