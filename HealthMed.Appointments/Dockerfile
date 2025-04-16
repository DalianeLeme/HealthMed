FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto (csproj)
COPY ["HealthMed.sln", "."]
COPY ["HealthMed.Appointments/HealthMed.Appointments.API.csproj", "HealthMed.Appointments/"]
COPY ["HealthMed.Appointments.Application/HealthMed.Appointments.Application.csproj", "HealthMed.Appointments.Application/"]
COPY ["HealthMed.Appointments.Domain/HealthMed.Appointments.Domain.csproj", "HealthMed.Appointments.Domain/"]
COPY ["HealthMed.Appointments.Infrastructure/HealthMed.Appointments.Infrastructure.csproj", "HealthMed.Appointments.Infrastructure/"]
COPY ["HealthMed.Auth.Domain/HealthMed.Auth.Domain.csproj", "HealthMed.Auth.Domain/"]
COPY ["HealthMed.Schedule.Domain/HealthMed.Schedule.Domain.csproj", "HealthMed.Schedule.Domain/"]
COPY ["HealthMed.Shared/HealthMed.Shared.csproj", "HealthMed.Shared/"]
COPY ["HealthMed.Appointments/appsettings.json", "HealthMed.Appointments/"]
COPY ["HealthMed.Schedule.Application/HealthMed.Schedule.Application.csproj", "HealthMed.Schedule.Application/"]

WORKDIR "/src/HealthMed.Appointments"
RUN dotnet restore "HealthMed.Appointments.API.csproj"

# Copia o restante dos arquivos necessários
COPY HealthMed.Appointments/. ./HealthMed.Appointments/
COPY HealthMed.Appointments.Application/. ./HealthMed.Appointments.Application/
COPY HealthMed.Appointments.Domain/. ./HealthMed.Appointments.Domain/
COPY HealthMed.Appointments.Infrastructure/. ./HealthMed.Appointments.Infrastructure/
COPY HealthMed.Auth.Domain/. ./HealthMed.Auth.Domain/
COPY HealthMed.Schedule.Domain/. ./HealthMed.Schedule.Domain/
COPY HealthMed.Shared/. ./HealthMed.Shared/
COPY HealthMed.Schedule.Application/. ./HealthMed.Schedule.Application/

WORKDIR "/src/HealthMed.Appointments"
RUN dotnet build "HealthMed.Appointments.API.csproj" -c Release -o /app/build /p:UseAppHost=false

FROM build AS publish
RUN dotnet publish "HealthMed.Appointments.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthMed.Appointments.API.dll"]
