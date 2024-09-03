
Install the EF Core tools globally run (one time only)
```shell
dotnet tool install -g dotnet-ef
```
or to update run (one time only)

```shell
dotnet tool update -g dotnet-ef.
```

Goto the project directory and run the following command to install the EF Core tools locally

```shell
cd src/eShop.WebApi
```

Generate EF Core migrations

```shell
dotnet ef migrations add InitialCreate
```

Execute EF Core migrations

```shell
dotnet ef database update
```

Run the following command to start the Otlp Collector and Tools

```shell
docker compose up
```

Grafana: http://localhost:3000/
Loki: http://localhost:3100/
Prometheus: http://localhost:9090/
Jaeger: http://localhost:16686/


Run the following command to build & run the project

```shell
docker build --no-cache -t eshop:local  -f .\eShop.WebApi\Dockerfile .
docker run -p 8080:80 eshop:local
```