# PayAPI (.NET 8 ASP.NET API)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY apis/PayAPI/PayAPI.csproj apis/PayAPI/
COPY Shared/Security/dotnet/Shared.Security.Net/Shared.Security.Net.csproj Shared/Security/dotnet/Shared.Security.Net/
RUN dotnet restore "apis/PayAPI/PayAPI.csproj"

COPY . .
RUN dotnet publish "apis/PayAPI/PayAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8082
ENV ASPNETCORE_URLS=http://+:8082
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PayAPI.dll"]
