# Getting started

## Dev Environment

Make sure to install the dev dependencies first.

> The project was set up on a mac and is intended to run on linux. Therefore everything is configured to run on unix systems. Windows User's might want to use WSL2.

- [dotnet 8 sdk](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [NodeJS](https://nodejs.org/en)
  - Version as listed in `.nvmrc`
  - Mac/Linux should use [nvm](https://github.com/nvm-sh/nvm)
  - Windows should use [nvm windows](https://github.com/coreybutler/nvm-windows)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

🚨🚨🚨 *There is an issue with Docker for Mac:*
1. MSSQL Image won't boot in current versions
2. Mac OS can classify docker as malware, that is a false positive
3. Remove docker by the scripts here: https://docs.docker.com/desktop/cert-revoke-solution/
4. Install Docker Desktop [4.32.1](https://docs.docker.com/desktop/release-notes/#4321)

## Recommended IDE

The Project is already configured for [VSCode](https://code.visualstudio.com/). When opening the directory, VSCode should ask to install recommended extensions: _DO SO_.

If something is missing or you find some cool extension worth for everyone, feel free to share in the team using the '.vscode/extensions.json'.

> Mac & Linux users, should have [Azure Data Studio](https://azure.microsoft.com/en-us/products/data-studio) as a replacement for SQL Server Management Studio.

## Make sure SSL works on localhost

This app requires HTTPS for working properly. Check the dotnet docs on how to [setup your certificates](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs).

## Installing dependencies

First we need to install developer libraries. We begin with globally installed dotnet tools:

```bash
dotnet tool install --global dotnet-aspnet-codegenerator
dotnet tool install --global dotnet-ef
dotnet tool install --global Microsoft.Web.LibraryManager.Cli
```

As this project uses some nodejs packages for developer tooling, make sure to switch to the correct node version and install the packages:

```bash
# Make sure to have the correct node version
nvm use # Windows users must lookup node version in .nvmrc and use an admin shell to set the version
npm ci
```

> Using `npm ci` command instead of `npm install` makes sure that the `package.lock.json` is not touched as it does not update any patch versions.

As soon as the node packages are installed, a gulp job copies client side libs into the wwwroot directory.

Now install all nuget packages for the solution:

```bash
dotnet restore
```

## User secrets

This project uses \*.env files to store user secrets and runtime app settings as they mimic environment variables. These files are stored in the directory `/.env` and must **never** be committed to source control.

The \*.env files are used for development. You can overwrite anything using Environment variables. Environment variables are always used in production.

> Ask a collegue to provide you the files and put them into `/.env`.

## Running the app

Before you run the application, make sure to execute the docker images for running a local server environment.

Make sure that docker desktop is running and and boot the images using:

```bash
docker-compose up -d # -d runs the immages as background service
```

If you need to stop the docker images, just run:

```bash
docker-compose down
```

### Available Services

- MS SQL Server

  - See [docs](sql-server.md) on how to use

- SMTP4Dev
  - See [docs](sending-email.md) on how to use

### Seed the database

When running this app for the first time, your database im empty. You must initiate a database with test data.

> See [SQL Server docs](sql-server.md)

### Launch from CLI

There are several ways to launch this app. All have one thing in common: We need to use the https profile!

First try if it works by using the CLI. Depending on your platform you might need

```bash
cd src/KDC.Main
dotnet run -lp https
```

If you wish to work with hot reload, you can instead use the watch feature. This rebuilds you app on file changes and refreshes the browser for you.

```bash
cd src/KDC.Main
dotnet watch run -lp https
```

### Launch from VSCode with Debugger

> The `select launch configuration` command is only available if you installed the recommended extensions.

If you didn't do so yet, hit [CMD/CRTL]+[SHIFT]+[P]. Type in `select launch configuration`, hit [Enter] and select https.

Once you did so, you can run the app in debug mode with breakpoints enabled by pressing [F5].

> To run without debugger, use the play button while you have some C# file open or use the command line.
