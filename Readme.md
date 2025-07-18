# Dotnet Docker Deploy Prototype

## Overview

This project is a template example for building a Dotnet web app as a Docker image and deploying it to Azure using CI/CD.


## Functionality List

- [x] Build and run in a single Docker image
- [x] Add comment based documentation into dockerfile
- [ ] Document how to run a docker build and execute locally for test ([see details](#docker-build-and-local-test))
- [x] Script to run the Docker build in GitHub Actions
- [x] Configure GitHub Packages and GitHub Actions permissions
- [x] Provide documentation on [GitHub Packages and Actions configuration](#github-packages-and-actions-configuration)
- [x] Script to run the Docker push to GitHub Packages from GitHub Actions
- [x] Improve performance by caching
- [x] Configure Azure App Service for hosting the Docker image
- [ ] Provide Azure configuration to allow Docker and CI/CD from this approach ([see details](#azure-configuration-for-docker-and-cicd))
- [x] Add support for local GitHub runners with cloud fallback if the runner is offline, to save quota during testing and development
- [ ] Add documentation on how to set up a local GitHub Actions runner (can be hosted anywhere like on azure or developer machine) ([see details](#local-github-runner-setup))

## Docker build and local test
This section explains how to build, tag, and run the Docker image for this project on your local machine. No prior Docker experience is required.

### Prerequisites
- [Install Docker Desktop](https://www.docker.com/products/docker-desktop/) for your operating system (Windows, macOS, or Linux).
- Make sure Docker is running (check the Docker icon in your system tray/menu bar).

### 1. Build the Docker Image
Open a terminal in the project folder (where the `Dockerfile` is located) and run:

```sh
docker build -t yourimagename .
```

- `-t yourimagename` gives your image a name. You can choose any name you like (e.g., "mywebapp").
- The `.` means "build from the current directory".

### 2. Run the Docker Container Locally
Start the container and expose port 8080 so you can access the app in your browser:

```sh
docker run -p 8080:8080 yourimagename
```

- `-p 8080:8080` maps port 8080 in the container to port 8080 on your computer.
- Replace `yourimagename` with the name you used in the build step.
- The app will start and listen on port 8080.

### 3. Test in Your Browser
Open your browser and go to:

```
http://localhost:8080
```

You should see the web app running. If you see an error, check the troubleshooting section below.

### 4. Stop the Container
Press `Ctrl+C` in the terminal to stop the running container.

---

### Troubleshooting
- **Port already in use:** If you get an error about port 8080, make sure nothing else is running on that port, or use a different port (e.g., `-p 8081:8080`).
- **Docker not found:** Make sure Docker Desktop is installed and running.
- **App not loading:** Check the terminal for errors. Make sure the build completed successfully and the app is listening on the correct port.

For more help, see the [Docker documentation](https://docs.docker.com/get-started/).

## GitHub Packages and Actions configuration
---

## Why we use `kulzer-dental/runner-fallback-action@main`


This project uses the custom GitHub Action [`kulzer-dental/runner-fallback-action`](https://github.com/kulzer-dental/runner-fallback-action) to automatically select between a self-hosted (local) runner and a cloud (GitHub-hosted) runner for CI/CD jobs.

### Why use a local/self-hosted runner?

- **Data protection:** Local runners can be hosted on infrastructure you control, ensuring sensitive code and secrets never leave your environment. This is important for organizations with strict data protection or compliance requirements.
- **Cost savings:** GitHub Actions for private repositories include a limited amount of free build minutes. Additional minutes require a paid plan. By using local runners, you can run builds on your own hardware (e.g., Azure VMs, on-prem servers, or developer laptops) and save on paid GitHub build minutes.
- **Flexibility:** Local runners can be hosted on any machine—cloud servers, physical servers, or even a developer's notebook. This allows you to tailor your CI/CD environment to your needs and resources.

The action checks if your local agent runner is online and uses it if available; otherwise, it automatically falls back to the default GitHub-hosted runner (using paid build minutes if your free quota is exhausted).

> **Note:** This approach can also enable running local Xcode and Android Studio builds for publishing mobile apps to the stores, using your own macOS or Windows machines as runners.

### Purpose and Benefits
- **Quota savings:** Prefer self-hosted runners for development and testing, saving GitHub-hosted runner minutes.
- **Reliability:** Automatically falls back to a cloud runner if the self-hosted runner is offline or unavailable.
- **Consistency:** Ensures jobs always run, regardless of local infrastructure status.

### Source Code and Documentation
The source code and documentation for this action are available at:
- [kulzer-dental/runner-fallback-action on GitHub](https://github.com/kulzer-dental/runner-fallback-action)

You can review, audit, and contribute to the action in that repository. It is shared across multiple projects in the organization for consistent runner selection logic.

---

## ⚠️🚨 Important: Use a Service Account for CI/CD PATs 🚨⚠️

**Do NOT use a personal access token (PAT) from a real user account for CI/CD workflows!**

If the PAT is bound to a personal user account, CI/CD will break when that user leaves the organization or loses access.

### ✅ Best Practice: Organization Service Account

- Create a dedicated GitHub user account ("service account") for your organization.
- Grant this account only the minimum required permissions (ideally only for CI/CD and package publishing).
- Generate the PAT from this service account, with the required scopes:
  - `repo`
  - `read:packages`
  - `write:packages`
- Add the PAT as the `LOCAL_RUNNER_FALLBACK_TOKEN` secret in your repository.

**This ensures CI/CD and package publishing will continue to work, even if individual developers leave or change roles.**

Future developers should never need to create or use their own personal PATs for CI/CD features. All automation should use the organization service account.

---

This section explains how to configure GitHub Packages as a Docker registry for hosting your Docker images, and set up GitHub Actions permissions to build and push images automatically.

### 1. Authenticate with GitHub CLI (optional, for local testing)
If you want to test pushing images locally, sign in with the GitHub CLI:

```sh
gh auth login
gh auth status
```

### 2. Configure Repository Permissions
Go to your repository **Settings > Actions > General**:
- Under **Workflow permissions**, select **Read and write permissions**.
- Under **Settings > Packages**, ensure your user/team has **write** access.

### 3. Required Secrets
Your workflow uses `${{ secrets.GITHUB_TOKEN }}` for authentication to GitHub Packages. This is automatically available in GitHub Actions and does not require manual setup.


#### LOCAL_RUNNER_FALLBACK_TOKEN (for self-hosted/local runners)
If you use self-hosted or local GitHub Actions runners, you need to provide a Personal Access Token (PAT) as the secret `LOCAL_RUNNER_FALLBACK_TOKEN`.

This PAT must have the following scopes:
- `repo` (full repository access)
- `read:packages` (read access to GitHub Packages)
- `write:packages` (write/push access to GitHub Packages)

How to set up:
1. Go to [GitHub Personal Access Tokens](https://github.com/settings/tokens) and generate a new token with the scopes above.
2. In your repository, go to **Settings > Secrets and variables > Actions**.
3. Add a new secret named `LOCAL_RUNNER_FALLBACK_TOKEN` and paste your PAT value.

This token is used in workflows (see `.github/workflows/docker-image.yml`) to authenticate and push images when running jobs on self-hosted/local runners, since the default `GITHUB_TOKEN` does not have sufficient permissions for package write operations outside GitHub-hosted runners.

### 4. Docker Registry Setup
Your workflow pushes images to `ghcr.io/kulzer-dental/kdc-experimental-image`. No extra configuration is needed for public images. For private images, ensure your workflow uses the correct token (the default `${{ secrets.GITHUB_TOKEN }}` is sufficient for most cases).

### 5. Workflow Reference
The workflow uses these steps:
- **Log in to registry:**
  ```yaml
  - name: Log in to container registry
    uses: docker/login-action@v2
    with:
      registry: ghcr.io/
      username: ${{ github.actor }}
      password: ${{ secrets.GITHUB_TOKEN }}
  ```
- **Build and push image:**
  ```yaml
  - name: Build and push container image to registry
    uses: docker/build-push-action@v3
    with:
      context: .
      push: true
      tags: ${{ steps.meta.outputs.tags }}
      file: ./Dockerfile
  ```

### 6. Personal Access Token (PAT) for Local Push (optional)
If you want to push images to GitHub Packages from your local machine, create a PAT with `write:packages`, `read:packages`, and `repo` scopes at https://github.com/settings/tokens and use it to authenticate Docker:

```sh
echo $CR_PAT | docker login ghcr.io -u <your-username> --password-stdin
```

---

For more details, see:
- [GitHub Packages: Docker](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-docker-registry)
- [GitHub Actions: Workflow permissions](https://docs.github.com/en/actions/security-guides/automatic-token-authentication)
- [gh CLI docs](https://cli.github.com/manual/)

## Azure configuration for Docker and CI/CD
TODO: Add Azure App Service configuration, Docker image deployment steps, and CI/CD pipeline setup for Azure.

## Local GitHub runner setup
TODO: Add instructions for installing, configuring, and using a local GitHub Actions runner, including security and fallback notes.

