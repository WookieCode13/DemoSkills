# ReportAPI (.NET 8 ASP.NET API)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY apis/ReportAPI/ReportAPI.csproj apis/ReportAPI/
COPY Shared/Security/dotnet/Shared.Security.Net/Shared.Security.Net.csproj Shared/Security/dotnet/Shared.Security.Net/
RUN dotnet restore "apis/ReportAPI/ReportAPI.csproj"

COPY . .
RUN dotnet publish "apis/ReportAPI/ReportAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8084
ENV ASPNETCORE_URLS=http://+:8084
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReportAPI.dll"]
