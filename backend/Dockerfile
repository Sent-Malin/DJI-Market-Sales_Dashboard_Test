FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY SalesDashboard.Api/SalesDashboard.Api.csproj SalesDashboard.Api/
RUN dotnet restore SalesDashboard.Api/SalesDashboard.Api.csproj
COPY SalesDashboard.Api/ SalesDashboard.Api/
RUN dotnet publish SalesDashboard.Api/SalesDashboard.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SalesDashboard.Api.dll"]