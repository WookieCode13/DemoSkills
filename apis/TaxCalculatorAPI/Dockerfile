# TaxCalculatorAPI (.NET 8 ASP.NET API)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaxCalculatorAPI.csproj .
RUN dotnet restore "TaxCalculatorAPI.csproj"

COPY . .
RUN dotnet publish "TaxCalculatorAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8083
ENV ASPNETCORE_URLS=http://+:8083
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaxCalculatorAPI.dll"]
