FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Adiciona o script wait-for-it.sh ao container base
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

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

# Copia os demais arquivos
COPY HealthMed.Appointments/. ./HealthMed.Appointments/
COPY HealthMed.Appointments.Application/. ./HealthMed.Appointments.Application/
COPY HealthMed.Appointments.Domain/. ./HealthMed.Appointments.Domain/
COPY HealthMed.Appointments.Infrastructure/. ./HealthMed.Appointments.Infrastructure/
COPY HealthMed.Auth.Domain/. ./HealthMed.Auth.Domain/
COPY HealthMed.Schedule.Domain/. ./HealthMed.Schedule.Domain/
COPY HealthMed.Shared/. ./HealthMed.Shared/
COPY HealthMed.Schedule.Application/. ./HealthMed.Schedule.Application/

RUN dotnet build "HealthMed.Appointments.API.csproj" -c Release -o /app/build /p:UseAppHost=false

FROM build AS publish
RUN dotnet publish "HealthMed.Appointments.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=base /app/wait-for-it.sh .

ENTRYPOINT ["bash", "-c", "./wait-for-it.sh rabbitmq:5672 -- ./wait-for-it.sh sqlserver:1433 -- dotnet HealthMed.Appointments.API.dll"]
