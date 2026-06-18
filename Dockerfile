FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/NewsCollector.Application/NewsCollector.Application.csproj", "NewsCollector.Application/"]
COPY ["src/NewsCollector.Worker/NewsCollector.Worker.csproj", "NewsCollector.Worker/"]
COPY ["src/NewsCollector.Infrastructure/NewsCollector.Infrastructure.csproj", "NewsCollector.Infrastructure/"]
COPY ["src/NewsCollector.Domain/NewsCollector.Domain.csproj", "NewsCollector.Domain/"]

RUN dotnet restore "NewsCollector.Worker/NewsCollector.Worker.csproj"

COPY src/ .
WORKDIR /src/NewsCollector.Worker
RUN dotnet publish "NewsCollector.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NewsCollector.Worker.dll"]
