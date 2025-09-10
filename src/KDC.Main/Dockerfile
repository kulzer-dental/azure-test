# Use the official .NET 8 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Install Node.js for CSS build process
RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy root package.json and install npm dependencies (skip postinstall for now)
COPY package*.json ./
RUN npm install --ignore-scripts

# Copy csproj and restore as distinct layers
COPY src/KDC.Main/KDC.Main.csproj ./src/KDC.Main/
RUN dotnet restore ./src/KDC.Main/KDC.Main.csproj

# Copy all source files from the KDC.Main directory
COPY src/KDC.Main/ ./src/KDC.Main/

# Run the gulp copy task now that gulpfile is available
RUN npx gulp -f src/KDC.Main/gulpfile.js copy

# Change to the project directory and build
WORKDIR /app/src/KDC.Main
RUN dotnet publish KDC.Main.csproj -c Debug -o /app/out

# Use the official .NET 8 ASP.NET runtime image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose port 5000 (change if your app uses a different port)
EXPOSE 5000

# Set environment variables if needed
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "KDC.Main.dll"]
