FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HealthMed.sln", "."]
COPY ["HealthMed.Auth.API/HealthMed.Auth.API.csproj", "HealthMed.Auth.API/"]
COPY ["HealthMed.Auth.Application/HealthMed.Auth.Application.csproj", "HealthMed.Auth.Application/"]
COPY ["HealthMed.Auth.Domain/HealthMed.Auth.Domain.csproj", "HealthMed.Auth.Domain/"]
COPY ["HealthMed.Auth.Infrastructure/HealthMed.Auth.Infrastructure.csproj", "HealthMed.Auth.Infrastructure/"]
COPY ["HealthMed.Shared/HealthMed.Shared.csproj", "HealthMed.Shared/"]
COPY ["HealthMed.Appointments.Domain/HealthMed.Appointments.Domain.csproj", "HealthMed.Appointments.Domain/"]
COPY ["HealthMed.Auth.API/appsettings.json", "HealthMed.Auth.API/"]

WORKDIR "/src/HealthMed.Auth.API"
RUN dotnet restore "HealthMed.Auth.API.csproj"

COPY HealthMed.Auth.API/. ./HealthMed.Auth.API/
COPY HealthMed.Auth.Application/. ./HealthMed.Auth.Application/
COPY HealthMed.Auth.Domain/. ./HealthMed.Auth.Domain/
COPY HealthMed.Auth.Infrastructure/. ./HealthMed.Auth.Infrastructure/
COPY HealthMed.Shared/. ./HealthMed.Shared/
COPY HealthMed.Appointments.Domain/. ./HealthMed.Appointments.Domain/

RUN dotnet build "HealthMed.Auth.API.csproj" -c Release -o /app/build /p:UseAppHost=false

FROM build AS publish
RUN dotnet publish "HealthMed.Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthMed.Auth.API.dll"]
