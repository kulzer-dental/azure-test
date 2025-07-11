### Build Stage ###
# Use the .NET SDK image for building the application
# This stage compiles the application and prepares it for deployment
# Use the official .NET SDK image

FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app

# Copy the project files
COPY . ./
# Restore the dependencies
RUN dotnet restore
# Build the application
RUN dotnet publish -c Release -o out

### Final Stage###
# Use the ASP.NET runtime image for running the application
# This stage contains only the necessary runtime files
# Use the official ASP.NET runtime image

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory in the runtime image
# This is where the application will run
WORKDIR /app
# Copy the published output from the build stage
COPY --from=0 /app/out .

# Add a label for Docker Desktop UI
LABEL org.opencontainers.image.title="experimental" \
      org.opencontainers.image.description="ASP.NET Core app exposing port 80" \
      org.opencontainers.image.authors="Just an experiment" \
      org.opencontainers.image.url="http://localhost:80"

#### Runtime Configuration ####

# Expose the port the app runs on
EXPOSE 8080
# Set the entry point for the application
ENTRYPOINT ["dotnet", "experimental.dll"]