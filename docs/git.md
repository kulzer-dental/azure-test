# GIT Conventions

## Branch conventions

We do not use gitflow. For convinience and correct naming: Use Jira to create branches.

## Conventional commits

We enforce [conventional commits](www.conventionalcommits.org) for commit messages. For your convinence you can use the vs code extension by `[CTRL/CMD]+[SHIFT]+[P]` and then `Conventional Commits`. It will format your message correct and trigger the commit.

## Jira ID in commits

As long as your commit message complies, it should be accepted. However having the jira id in the message is very helpful for rebasing. Therefore we auto-add the jira id if your branch name starts with it.

> For convinience: Use Jira to create branches

### GIT Husky

This repo uses [Git Husky](https://typicode.github.io/husky/get-started.html) to enforce some rules.

- Prevent commit to main: You will not be able to commit to main. Managed by `.husky/pre-commit`
- Adds jira id from branch name to commit messages
- Enforces conventional commits

> **Note**: At the time of writing the GIT server does not block pushes to main! This is blocked locally only.
