# Dotnet Docker Deploy Prototype

## Overview

This project is a template example for building a Dotnet web app as a Docker image and deploying it to Azure using CI/CD.


## Functionality List

- [x] Build and run in a single Docker image
- [x] Add comment based documentation into dockerfile
- [x] Document how to run a docker build and execute locally for test ([see details](Docs/dockerBuildAndLocalTest.md))
- [x] Script to run the Docker build in GitHub Actions
- [x] Configure GitHub Packages and GitHub Actions permissions
- [x] Provide documentation on [GitHub Packages and Actions configuration](Docs/githubPackagesAndActionsConfig.md)
- [x] Script to run the Docker push to GitHub Packages from GitHub Actions
- [x] Improve performance by caching
- [x] Configure Azure App Service for hosting the Docker image
- [ ] Provide Azure configuration to allow Docker and CI/CD from this approach ([see details](Docs/azureConfigurationForDockerAndCicd.md))
- [x] Add support for local GitHub runners with cloud fallback if the runner is offline, to save quota during testing and development
- [ ] Add documentation on how to set up a local GitHub Actions runner (can be hosted anywhere like on azure or developer machine) ([see details](Docs/localGithubRunnerSetup.md))
