FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eShop.WebApi/eShop.WebApi.csproj", "eShop.WebApi/"]
RUN dotnet restore "eShop.WebApi/eShop.WebApi.csproj"
COPY . .
WORKDIR "/src/eShop.WebApi"
RUN dotnet build "eShop.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "eShop.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build ./eShop.db ./eShop.db
ENTRYPOINT ["dotnet", "eShop.WebApi.dll"]
