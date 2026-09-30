FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY SalesDashboard.Api/SalesDashboard.Api.csproj SalesDashboard.Api/
RUN dotnet restore SalesDashboard.Api/SalesDashboard.Api.csproj
COPY SalesDashboard.Api/ SalesDashboard.Api/
RUN dotnet publish SalesDashboard.Api/SalesDashboard.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
# curl нужен только для healthcheck контейнера
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "SalesDashboard.Api.dll"]