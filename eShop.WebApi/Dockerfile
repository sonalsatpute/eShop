FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_VER=1.0.0
WORKDIR /src

COPY ["./**/*.csproj", "./eShop.sln", "./"]

RUN for FILE in `ls *.csproj`; \
    do \
      directory_name=${FILE%.*}; \
#      echo $directory_name; \
      mkdir $directory_name; >/dev/null 2>&1; \
      mv $FILE $directory_name/$FILE; \
    done

RUN dotnet restore --source https://api.nuget.org/v3/index.json eShop.sln

COPY . .

RUN dotnet build /p:Version=${BUILD_VER} ./eShop.sln && \
    dotnet publish /src/eShop.WebApi/eShop.WebApi.csproj -c Release -o /app/eShop.WebApi


FROM base AS final
WORKDIR /app
# Copy the dotnet tools from the build stage
#COPY --from=build /root/.dotnet/tools/ /root/.dotnet/tools
#ENV PATH="$PATH:/root/.dotnet/tools"

COPY --from=build /src/eShop.db .
COPY --from=build /app/. .

WORKDIR /app/eShop.WebApi
ENTRYPOINT ["dotnet", "eShop.WebApi.dll"]