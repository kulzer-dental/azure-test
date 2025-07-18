# Dotnet Docker Deploy Prototype

## Overview

This project is a template example for building a Dotnet web app as a Docker image and deploying it to Azure using CI/CD.


## Functionality List

- [x] Build and run in a single Docker image
- [x] Add comment based documentation into dockerfile
- [ ] Document how to run a docker build and execute locally for test ([see details](#docker-build-and-local-test))
- [x] Script to run the Docker build in GitHub Actions
- [x] Configure GitHub Packages and GitHub Actions permissions
- [ ] Provide documentation on [GitHub Packages and Actions configuration](#github-packages-and-actions-configuration)
- [x] Script to run the Docker push to GitHub Packages from GitHub Actions
- [x] Improve performance by caching
- [x] Configure Azure App Service for hosting the Docker image
- [ ] Provide Azure configuration to allow Docker and CI/CD from this approach ([see details](#azure-configuration-for-docker-and-cicd))
- [x] Add support for local GitHub runners with cloud fallback if the runner is offline, to save quota during testing and development
- [ ] Add documentation on how to set up a local runner (can be hosted anywhere) ([see details](#local-github-runner-setup))

## Docker build and local test
TODO: Add instructions for building the Docker image and running it locally for testing, including example commands and troubleshooting tips.

## GitHub Packages and Actions configuration
TODO: GitHub Packages and Actions configuration, PAT, and required permissions.

## Azure configuration for Docker and CI/CD
TODO: Add Azure App Service configuration, Docker image deployment steps, and CI/CD pipeline setup for Azure.

## Local GitHub runner setup
TODO: Add instructions for installing, configuring, and using a local GitHub Actions runner, including security and fallback notes.

