
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
cd eShop.WebApi
```

Generate EF Core migrations

```shell
dotnet ef migrations add InitialCreate
```

Execute EF Core migrations

```shell
dotnet ef database update
```