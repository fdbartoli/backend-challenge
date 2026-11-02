# Build: restore and publish the API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project files first, so the restore layer is cached while only code changes
COPY Banco.sln ./
COPY src/Banco.Domain/Banco.Domain.csproj src/Banco.Domain/
COPY src/Banco.Application/Banco.Application.csproj src/Banco.Application/
COPY src/Banco.Infrastructure/Banco.Infrastructure.csproj src/Banco.Infrastructure/
COPY src/Banco.Api/Banco.Api.csproj src/Banco.Api/
RUN dotnet restore src/Banco.Api/Banco.Api.csproj

COPY src/ src/
RUN dotnet publish src/Banco.Api/Banco.Api.csproj -c Release -o /app/publish --no-restore

# Database: built with the same versioned scripts
FROM debian:bookworm-slim AS database
RUN apt-get update \
    && apt-get install -y --no-install-recommends sqlite3 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /db
COPY database/ database/
RUN sqlite3 banco.db ".read database/masterSQL.sql"

# Runtime: only the ASP.NET runtime, running as a non-root user
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
RUN mkdir -p /app/data /app/logs && chown app:app /app/data /app/logs
COPY --from=database --chown=app:app /db/banco.db /app/data/banco.db

ENV ASPNETCORE_HTTP_PORTS=8080
ENV Database__ConnectionString="Data Source=/app/data/banco.db;Mode=ReadWrite;Foreign Keys=True"

EXPOSE 8080
USER app

ENTRYPOINT ["dotnet", "Banco.Api.dll"]
