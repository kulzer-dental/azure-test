## GitHub Packages and Actions configuration
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
