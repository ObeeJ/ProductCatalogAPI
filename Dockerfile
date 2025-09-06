FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ProductCatalogAPI.API/ProductCatalogAPI.API.csproj", "src/ProductCatalogAPI.API/"]
COPY ["src/ProductCatalogAPI.Application/ProductCatalogAPI.Application.csproj", "src/ProductCatalogAPI.Application/"]
COPY ["src/ProductCatalogAPI.Domain/ProductCatalogAPI.Domain.csproj", "src/ProductCatalogAPI.Domain/"]
COPY ["src/ProductCatalogAPI.Infrastructure/ProductCatalogAPI.Infrastructure.csproj", "src/ProductCatalogAPI.Infrastructure/"]
RUN dotnet restore "src/ProductCatalogAPI.API/ProductCatalogAPI.API.csproj"
COPY . .
WORKDIR "/src/src/ProductCatalogAPI.API"
RUN dotnet build "ProductCatalogAPI.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ProductCatalogAPI.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductCatalogAPI.API.dll"]