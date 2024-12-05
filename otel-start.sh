#!/bin/bash

set -e
set -x

rm -rf otel
rm -rf build

 mkdir otel
mkdir build



export OTEL_DOTNET_AUTO_HOME="$(pwd)/otel"
echo $OTEL_DOTNET_AUTO_HOME

 curl -L -o otel/otel-dotnet-install.sh https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v1.9.0/otel-dotnet-auto-install.sh

 chmod +x otel/otel-dotnet-install.sh
 otel/otel-dotnet-install.sh

 chmod +x otel/instrument.sh
 source otel/instrument.sh

export OTEL_SERVICE_NAME="eShop.Orders.Api"
export OTEL_RESOURCE_ATTRIBUTES="deployment.environment=staging,service.version=1.0.0"
export OTEL_EXPORTER_OTLP_ENDPOINT="http://host.docker.internal:4318"
export OTEL_TRACES_EXPORTER="console"
export OTEL_LOGS_EXPORTER="console"
export OTEL_METRICS_EXPORTER="console"

# Disable auto-instrumentation
export OTEL_DOTNET_AUTO_LOGS_ENABLED="true"
export OTEL_DOTNET_AUTO_TRACES_ENABLED="true"

# Enable auto-instrumentation
export OTEL_DOTNET_AUTO_METRICS_ENABLED="true"

source otel/instrument.sh


# dotnet restore --source https://api.nuget.org/v3/index.json --source http://ng-repo.dev.kibocommerce.com:8081/repository/nuget-localbuild/ --source http://ng-repo.dev.kibocommerce.com:8081/repository/nuget-external/ Mozu.CommerceRuntime.sln
dotnet build eShop.sln  -c Release
dotnet publish eShop.Orders.Api/eShop.Orders.Api.csproj -c Release -o ./build --no-build

source otel/instrument.sh && cd build && dotnet eShop.Orders.Api.dll