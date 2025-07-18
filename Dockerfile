
# ============================================================================
# DOCKERFILE FOR ASP.NET CORE APPLICATION (EXPERIMENTAL)
# ============================================================================
# This Dockerfile builds and runs an ASP.NET Core application using multi-stage builds.
# It is fully documented for beginners. Each instruction is explained in detail.
#
# Multi-stage builds allow us to use one image for building (with SDK/tools)
# and a smaller, optimized image for running (just the runtime).
# ============================================================================


# -----------------------------------------------------------------------------
# BUILD STAGE
# -----------------------------------------------------------------------------
# Purpose: Compile and publish the .NET application using the official SDK image.
#
# FROM: Specifies the base image. Here, we use the .NET 8.0 SDK image, which includes
# all tools needed to build and publish .NET applications.
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# WORKDIR: Sets the working directory inside the container to /app. All subsequent
# commands will run from this directory.
WORKDIR /app

# COPY: Copies all files and folders from your project directory on your computer
# into the /app directory in the container. This includes source code, configs, etc.
COPY . ./

# RUN dotnet restore: Restores NuGet packages (external libraries) required by the project.
RUN dotnet restore

# RUN dotnet publish: Builds the application in Release mode and publishes the output
# to the /app/out directory. This output contains everything needed to run the app.
RUN dotnet publish -c Release -o out


# -----------------------------------------------------------------------------
# RUNTIME STAGE
# -----------------------------------------------------------------------------
# Purpose: Create a lightweight image to run the published application.
#
# FROM: Uses the official ASP.NET Core runtime image (no build tools, just runtime).
# This keeps the final image small and secure.
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# WORKDIR: Sets the working directory to /app in the runtime container.
WORKDIR /app

# COPY --from=build /app/out .
# Copies the published output from the build stage into the runtime container's /app directory.
# Only the necessary files to run the app are included.
COPY --from=build /app/out .

# LABEL: Adds metadata to the image. These labels are useful for documentation,
# container management, and Docker Desktop UI. You can customize these as needed.
LABEL org.opencontainers.image.title="experimental" \
      org.opencontainers.image.description="ASP.NET Core app exposing port 8080" \
      org.opencontainers.image.authors="Just an experiment" \
      org.opencontainers.image.url="http://localhost:8080"


# -----------------------------------------------------------------------------
# RUNTIME CONFIGURATION
# -----------------------------------------------------------------------------
# EXPOSE: Informs Docker that the application will listen on port 8080 inside the container.
# This does NOT actually publish the port; it is a hint for users and tools.
EXPOSE 8080

# ENTRYPOINT: Specifies the command that will run when the container starts.
# Here, it launches the .NET application using the published DLL.
ENTRYPOINT ["dotnet", "experimental.dll"]

# ============================================================================
# USAGE NOTES
# ============================================================================
# - To build the image:   docker build -t experimental .
# - To run the container: docker run -p 8080:8080 experimental
#   (This maps port 8080 in the container to port 8080 on your host machine.)
# - You can change the EXPOSE and ENTRYPOINT values as needed for your app.
# - For more info, see: https://docs.docker.com/engine/reference/builder/
# ============================================================================