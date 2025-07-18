

# Local GitHub Actions Runner Setup

This guide explains how to install, configure, and use a local GitHub Actions runner. You can host the runner on any platform (Windows, macOS, Linux, cloud VM, etc.). All required commands and steps are provided directly in the GitHub UI during setup.



## 1. Prerequisites

- A machine with internet access (any OS)
- GitHub account with repository admin access
- All build tools required by your workflows must be installed on the runner machine (e.g., Docker, language runtimes, compilers, etc.)



## 2. Download and Install the Runner

1. Go to your repository on GitHub.
2. Navigate to **Settings** > **Actions** > **Runners** > **New self-hosted runner**.
3. Select your operating system and follow the instructions provided by GitHub. The UI will display all required commands and steps for your platform.



## 3. Configure the Runner

1. Use the configuration command shown in the GitHub UI to register your runner with your repository.
2. Optionally, set labels to identify your runner (e.g., `local`, `docker`, `azure`).



## 4. Start the Runner

Start the runner using the command provided in the GitHub UI. For background or service installation, follow the platform-specific instructions shown during setup.



## 5. Security Notes

- Only run trusted workflows on your local runner.
- Restrict network and file system access if possible.
- Regularly update the runner software.
- Use a dedicated user account for the runner process.



## 6. Fallback to Cloud Runners

You can configure your workflow to use the local runner when available, and fallback to GitHub-hosted runners if offline. See the GitHub Actions documentation for examples and best practices.



## 7. Maintenance

- Stop or uninstall the runner using the commands and instructions provided in the GitHub UI for your platform.



## References

- [GitHub Docs: Self-hosted runners](https://docs.github.com/en/actions/hosting-your-own-runners/about-self-hosted-runners)
- [Security best practices](https://docs.github.com/en/actions/hosting-your-own-runners/security-best-practices-for-self-hosted-runners)
