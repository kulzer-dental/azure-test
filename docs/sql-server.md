# SQL Server

## Local docker instance

> See [Getting started](./getting-started.md) to launch it.

- Listens at localhost:6005
- Credentials from `.env/mssql.env`

## Seeding data

You can pre-populate the database with test data using:

```bash
cd src/KDC.Main
dotnet run /seed
```

> TODO: At the time of writing this is only available in Development, while there should be a setup for production too.

### Adding more seed data

When adding new database config, make sure to have seed scripts for other developers! This can be done by using the [EF Core seed feature](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding) or for more complex scenarios have a look at [Program.SeedUsers.cs](../src/KDC.Main/Program.SeedUsers.cs) which is called by [Program.Args.cs](../src/KDC.Main/Program.Args.cs)

## Entity framework

This application uses entity framework for it's database connections and uses more than only one DbContext. As we want to avoid running multiple databases at azure, the DbContexts are configured to share the same connection string using a different schema. Therefore you need to provide the context when working with the CLI. See: [Using Multiple EF Core DbContexts in a Single Application](https://www.youtube.com/watch?v=-_AKTzDrYVc)

> Put your own entities into `ApplicationDbContext`. The others are exclusive to 3rd party components and should not be touched!

### Generate Migrations for ApplicationDbContext

See the [Mirosoft Docs](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli) on how to use migrations in general.

To add a migration use:

```bash
cd src/KDC.Main

dotnet ef migrations add "YOUR_NAME" -o Data/Migrations/Application -c ApplicationDbContext
```

### Apply migrations

The app will ask you to apply migrations with an error page if you run in Development environment. However you might want to update the database manually. You can do so by either the launch args or running ef commands:

```bash
cd src/KDC.Main

# Using launch args
dotnet run /migrate

# Using entity framework
dotnet ef database update -c ApplicationDbContext
dotnet ef database update -c PersistedGrantDbContext
dotnet ef database update -c ConfigurationDbContext
```

> TODO: At the time of writing this is only available in Development, while there should be a setup for production too.

### Delete the database

In case you ever want to drop the database, there is a script for it.

> Only available in Development

```bash
cd src/KDC.Main

dotnet run /drop

# Using entity framework
dotnet ef database update -c ApplicationDbContext
dotnet ef database update -c PersistedGrantDbContext
dotnet ef database update -c ConfigurationDbContext
```

### Re-Create Migrations

In case migrations are ever corrupted, just delete the `src/KDC.Main/Data/Migrations` directory and run this:

```bash
cd src/KDC.Main

dotnet ef migrations add ReInitApplicationDb -c ApplicationDbContext -o Data/Migrations/Application
dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb
dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb
```
