# Azure Configuration for Docker and CI/CD

This document provides comprehensive documentation for setting up Azure App Service to host Docker containers deployed via GitHub Actions CI/CD pipeline.

## Overview

The current setup uses:
- **Azure Subscription**: Customer Identity Management (`0287cce7-ccfc-4dd8-869d-a9d9e1d4ed98`)
- **Azure Tenant**: Kulzer (`cf8cde7b-6630-4908-8168-d675ccb1ec97`)
- **Resource Group**: `kdc-resource-group` (Germany West Central)
- **App Service Plan**: `kdc-hosting-plan` (Linux, Basic B1)
- **Web App**: `kdc-test` (Container-based)
- **Container Registry**: GitHub Container Registry (`ghcr.io`)

## Azure Infrastructure

### Resource Group
```bash
Name: kdc-resource-group
Location: Germany West Central
Status: Succeeded
```

### App Service Plan
```bash
Name: kdc-hosting-plan
Tier: Basic (B1)
OS: Linux
Workers: 1 (Small size)
Location: Germany West Central
Free Tier Expires: 2025-08-14T13:04:12
```

**Key Features:**
- Reserved Linux plan (required for Docker containers)
- Basic tier with 1 worker (can scale to 3)
- Zone redundant: Disabled
- Auto-scaling: Disabled

### Web App Configuration
```bash
Name: kdc-test
URL: https://kdc-test.azurewebsites.net
Docker Image: ghcr.io/kulzer-dental/kdc-experimental-image:latest
Platform: Linux
State: Running
```

**Key Settings:**
- **HTTPS Only**: Enabled
- **Container Settings**:
  - `DOCKER_ENABLE_CI`: `true` (enables webhook for auto-deployment)
  - `WEBSITES_ENABLE_APP_SERVICE_STORAGE`: `false` (ephemeral storage)
- **FTP**: FTPS Only
- **Default Documents**: Standard ASP.NET/HTML defaults
- **IP Restrictions**: Allow all (open to internet)

## GitHub Actions Integration

### Required Secrets

The GitHub repository (`kulzer-dental/azure-test`) contains these secrets:

#### 1. AZURE_WEBAPP_PUBLISH_PROFILE
Contains the XML publish profile for Azure Web App deployment. This profile includes:
- Deployment endpoints
- Authentication credentials
- Publishing methods (Web Deploy, FTP, Zip Deploy)

**To regenerate this secret:**
```bash
az webapp deployment list-publishing-profiles \
  --name kdc-test \
  --resource-group kdc-resource-group \
  --xml
```

#### 2. LOCAL_RUNNER_FALLBACK_TOKEN
Personal Access Token for GitHub Actions runner fallback functionality.

### Workflow Configuration

The CI/CD pipeline (`.github/workflows/docker-image.yml`) includes:

1. **Runner Detection**: Uses local runners with cloud fallback
2. **Docker Build**: Builds and pushes to GitHub Container Registry
3. **Azure Deployment**: Deploys latest image to Azure Web App

**Key Components:**
- **Container Registry**: `ghcr.io/kulzer-dental/kdc-experimental-image`
- **Deployment Method**: Azure Web Apps Deploy action with publish profile
- **Image Tags**: Uses docker/metadata-action for safe tagging

## Step-by-Step Setup Instructions

### 1. Azure Resource Creation

#### Create Resource Group
```bash
az group create \
  --name kdc-resource-group \
  --location germanywestcentral
```

#### Create App Service Plan
```bash
az appservice plan create \
  --name kdc-hosting-plan \
  --resource-group kdc-resource-group \
  --location germanywestcentral \
  --is-linux \
  --sku B1
```

#### Create Web App
```bash
az webapp create \
  --name kdc-test \
  --resource-group kdc-resource-group \
  --plan kdc-hosting-plan \
  --deployment-container-image-name ghcr.io/kulzer-dental/kdc-experimental-image:latest
```

### 2. Configure Web App Settings

#### Enable Docker CI
```bash
az webapp config appsettings set \
  --name kdc-test \
  --resource-group kdc-resource-group \
  --settings DOCKER_ENABLE_CI=true
```

#### Disable App Service Storage
```bash
az webapp config appsettings set \
  --name kdc-test \
  --resource-group kdc-resource-group \
  --settings WEBSITES_ENABLE_APP_SERVICE_STORAGE=false
```

#### Enable HTTPS Only
```bash
az webapp update \
  --name kdc-test \
  --resource-group kdc-resource-group \
  --https-only true
```

### 3. GitHub Integration Setup

#### Generate Publish Profile
```bash
az webapp deployment list-publishing-profiles \
  --name kdc-test \
  --resource-group kdc-resource-group \
  --xml > publish-profile.xml
```

#### Add to GitHub Secrets
1. Copy the entire XML content from `publish-profile.xml`
2. In GitHub repository settings, go to Secrets and Variables > Actions
3. Create secret `AZURE_WEBAPP_PUBLISH_PROFILE` with the XML content

### 4. Container Registry Access

The setup uses GitHub Container Registry (`ghcr.io`) with the following configuration:
- **Registry**: `ghcr.io`
- **Image Name**: `kulzer-dental/kdc-experimental-image`
- **Authentication**: GitHub Actions `GITHUB_TOKEN` (automatic)

## Deployment Flow

1. **Code Push**: Developer pushes to `main` branch
2. **Runner Selection**: Action detects local runner or falls back to cloud
3. **Docker Build**: 
   - Checks out code
   - Sets up Docker Buildx
   - Logs into GitHub Container Registry
   - Builds and pushes image with proper tags
4. **Azure Deployment**:
   - Uses Azure Web Apps Deploy action
   - Authenticates with publish profile
   - Pulls latest image from GHCR
   - Updates web app container

## Monitoring and Troubleshooting

### Useful Azure CLI Commands

#### Check Web App Status
```bash
az webapp show \
  --name kdc-test \
  --resource-group kdc-resource-group \
  --query "state"
```

#### View Application Logs
```bash
az webapp log tail \
  --name kdc-test \
  --resource-group kdc-resource-group
```

#### Check Container Settings
```bash
az webapp config show \
  --name kdc-test \
  --resource-group kdc-resource-group \
  --query "linuxFxVersion"
```

#### Restart Web App
```bash
az webapp restart \
  --name kdc-test \
  --resource-group kdc-resource-group
```

### Common Issues and Solutions

#### 1. Container Fails to Start
- **Check logs**: Use `az webapp log tail` to see startup errors
- **Verify image**: Ensure the Docker image runs locally
- **Check ports**: Ensure the container exposes the correct port (typically 80 or 8080)

#### 2. GitHub Actions Deploy Fails
- **Publish Profile**: Regenerate and update the `AZURE_WEBAPP_PUBLISH_PROFILE` secret
- **Permissions**: Verify GitHub Actions has `packages: write` permission
- **Image Tags**: Check that the image tag exists in GHCR

#### 3. Slow Deployments
- **Container Image Size**: Optimize Dockerfile to reduce image size
- **Docker Layer Caching**: Ensure build cache is working in GitHub Actions
- **App Service Plan**: Consider upgrading to higher tier for better performance

## Security Considerations

### Current Security Settings
- **HTTPS Only**: Enabled (all HTTP requests redirect to HTTPS)
- **TLS Version**: Minimum 1.2
- **FTP**: FTPS Only (encrypted FTP)
- **IP Restrictions**: None (open to internet)
- **Authentication**: None (public access)

### Recommended Security Enhancements
1. **Add IP Restrictions** for administrative access
2. **Enable Application Insights** for monitoring
3. **Set up Custom Domain** with SSL certificate
4. **Configure Authentication** if needed
5. **Enable Diagnostic Logging** for security auditing

## Cost Optimization

### Current Costs
- **App Service Plan**: Basic B1 (~$13.14/month)
- **Free Tier**: Expires 2025-08-14 (currently free)

### Optimization Strategies
1. **Auto-scaling**: Configure based on demand
2. **Deployment Slots**: Use staging slots for testing
3. **Reserved Instances**: For production workloads
4. **Resource Monitoring**: Use Azure Cost Management

## Backup and Disaster Recovery

### Current Setup
- **No automated backups** configured
- **Single region deployment** (Germany West Central)

### Recommended Enhancements
1. **Configure App Service Backup** for application data
2. **Multi-region deployment** for high availability
3. **Database backup** (if using databases)
4. **Infrastructure as Code** (ARM templates/Terraform)

## CLI Reference Commands

### Resource Management
```bash
# List all resources
az resource list --resource-group kdc-resource-group --output table

# Get resource group info
az group show --name kdc-resource-group

# Delete resource group (careful!)
az group delete --name kdc-resource-group --yes --no-wait
```

### Web App Management
```bash
# Get all web app settings
az webapp config appsettings list --name kdc-test --resource-group kdc-resource-group

# Update container image
az webapp config container set \
  --name kdc-test \
  --resource-group kdc-resource-group \
  --docker-custom-image-name ghcr.io/kulzer-dental/kdc-experimental-image:latest

# Scale web app
az webapp update \
  --name kdc-test \
  --resource-group kdc-resource-group \
  --number-of-workers 2
```

### Deployment Management
```bash
# List deployment history
az webapp deployment list \
  --name kdc-test \
  --resource-group kdc-resource-group

# Get deployment source info
az webapp deployment source show \
  --name kdc-test \
  --resource-group kdc-resource-group
```

---

## Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Docker on App Service](https://docs.microsoft.com/en-us/azure/app-service/tutorial-custom-container)
- [GitHub Actions for Azure](https://docs.microsoft.com/en-us/azure/developer/github/)
- [Azure CLI Reference](https://docs.microsoft.com/en-us/cli/azure/)

---

*Last updated: 2025-07-18*  
*Environment: Customer Identity Management subscription*  
*Status: Production-ready*
