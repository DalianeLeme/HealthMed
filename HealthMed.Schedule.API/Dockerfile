FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["HealthMed.sln", "."]
COPY ["HealthMed.Schedule.API/HealthMed.Schedule.API.csproj", "HealthMed.Schedule.API/"]
COPY ["HealthMed.Schedule.Application/HealthMed.Schedule.Application.csproj", "HealthMed.Schedule.Application/"]
COPY ["HealthMed.Schedule.Domain/HealthMed.Schedule.Domain.csproj", "HealthMed.Schedule.Domain/"]
COPY ["HealthMed.Schedule.Infrastructure/HealthMed.Schedule.Infrastructure.csproj", "HealthMed.Schedule.Infrastructure/"]
COPY ["HealthMed.Shared/HealthMed.Shared.csproj", "HealthMed.Shared/"]
COPY ["HealthMed.Schedule.API/appsettings.json", "HealthMed.Schedule.API/"]

WORKDIR "/src/HealthMed.Schedule.API"
RUN dotnet restore "HealthMed.Schedule.API.csproj"

COPY HealthMed.Schedule.API/. ./HealthMed.Schedule.API/
COPY HealthMed.Schedule.Application/. ./HealthMed.Schedule.Application/
COPY HealthMed.Schedule.Infrastructure/. ./HealthMed.Schedule.Infrastructure/
COPY HealthMed.Schedule.Domain/. ./HealthMed.Schedule.Domain/
COPY HealthMed.Shared/. ./HealthMed.Shared/

RUN dotnet build "HealthMed.Schedule.API.csproj" -c Release -o /app/build /p:UseAppHost=false

FROM build AS publish
RUN dotnet publish "HealthMed.Schedule.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=base /app/wait-for-it.sh .

ENTRYPOINT ["bash", "-c", "./wait-for-it.sh rabbitmq:5672 -- ./wait-for-it.sh sqlserver:1433 -- dotnet HealthMed.Schedule.API.dll"]
