# Prompt 11: CI/CD Pipeline (GitHub Actions)

## Overview
Automated pipeline: Build Docker image → Push to ACR → Deploy to AKS using OIDC authentication (no secrets).

---

## GitHub Actions Workflow (.github/workflows/build-deploy.yml)

```yaml
name: Build and Deploy to AKS

on:
  push:
    branches: [ main ]

env:
  ACR_NAME: ${{ vars.ACR_NAME }}
  AKS_RESOURCE_GROUP: ${{ vars.AKS_RESOURCE_GROUP }}
  AKS_CLUSTER_NAME: ${{ vars.AKS_CLUSTER_NAME }}
  IMAGE_NAME: foodhub-api

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    permissions:
      id-token: write   # Required for Azure OIDC
      contents: read    # Required to checkout code
    
    steps:
    # Checkout source code
    - name: Checkout code
      uses: actions/checkout@v4
    
    # Set up Docker Buildx for cross-platform builds
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
    
    # Azure login using OIDC (no secrets)
    - name: Azure Login
      uses: azure/login@v2
      with:
        client-id: ${{ vars.AZURE_CLIENT_ID }}
        tenant-id: ${{ vars.AZURE_TENANT_ID }}
        subscription-id: ${{ vars.AZURE_SUBSCRIPTION_ID }}
    
    # Login to Azure Container Registry
    - name: Login to ACR
      run: |
        az acr login --name ${{ env.ACR_NAME }}
    
    # Build Docker image for linux/amd64
    - name: Build Docker image
      run: |
        docker buildx build --platform linux/amd64 --load -t ${{ env.ACR_NAME }}.azurecr.io/${{ env.IMAGE_NAME }}:${{ github.sha }} .
        docker tag ${{ env.ACR_NAME }}.azurecr.io/${{ env.IMAGE_NAME }}:${{ github.sha }} ${{ env.ACR_NAME }}.azurecr.io/${{ env.IMAGE_NAME }}:latest
    
    # Push image to ACR with both tags
    - name: Push to ACR
      run: |
        docker push ${{ env.ACR_NAME }}.azurecr.io/${{ env.IMAGE_NAME }}:${{ github.sha }}
        docker push ${{ env.ACR_NAME }}.azurecr.io/${{ env.IMAGE_NAME }}:latest
    
    # Get AKS credentials
    - name: Get AKS credentials
      run: |
        az aks get-credentials --resource-group ${{ env.AKS_RESOURCE_GROUP }} --name ${{ env.AKS_CLUSTER_NAME }} --overwrite-existing
    
    # Apply Kubernetes manifests
    - name: Deploy to AKS
      run: |
        # Replace ACR_NAME placeholder in deployment.yaml
        sed -i \"s/__ACR_NAME__/${{ env.ACR_NAME }}/g\" k8s/prod/deployment.yaml
        
        # Apply manifests
        kubectl apply -f k8s/prod/deployment.yaml
        kubectl apply -f k8s/prod/service.yaml
        
        # Update image to trigger rollout
        kubectl set image deployment/foodhub-api foodhub-api=${{ env.ACR_NAME }}.azurecr.io/${{ env.IMAGE_NAME }}:${{ github.sha }}
    
    # Verify deployment rollout
    - name: Verify deployment
      run: |
        kubectl rollout status deployment/foodhub-api --timeout=300s
    
    # Get deployment status
    - name: Get deployment status
      run: |
        kubectl get pods -l app=foodhub-api
        kubectl get services
```

---

## GitHub Repository Configuration

### 1. Repository Variables (Settings → Secrets and variables → Actions → Variables)

Create these **Variables** (public, non-sensitive):
- `ACR_NAME` - Your Azure Container Registry name (e.g., `foodhubacr`)
- `AKS_RESOURCE_GROUP` - Resource group name (e.g., `foodhub-rg`)
- `AKS_CLUSTER_NAME` - AKS cluster name (e.g., `foodhub-aks`)
- `AZURE_CLIENT_ID` - App registration client ID for OIDC
- `AZURE_TENANT_ID` - Azure tenant ID
- `AZURE_SUBSCRIPTION_ID` - Azure subscription ID

**DO NOT create any Secrets** - OIDC eliminates the need for secrets.

---

## Azure Setup

### 1. Create Azure Resources

```bash
# Resource group
az group create --name foodhub-rg --location eastus

# ACR
az acr create --resource-group foodhub-rg --name foodhubacr --sku Basic

# AKS
az aks create \
  --resource-group foodhub-rg \
  --name foodhub-aks \
  --node-count 1 \
  --enable-managed-identity \
  --attach-acr foodhubacr \
  --generate-ssh-keys
```

### 2. Configure OIDC Authentication

```bash
# Create App Registration
az ad app create --display-name \"GitHub-FoodHub-Actions\"

# Get Application (Client) ID
APP_ID=$(az ad app list --display-name \"GitHub-FoodHub-Actions\" --query \"[0].appId\" -o tsv)

# Get Object ID
OBJECT_ID=$(az ad app show --id $APP_ID --query \"id\" -o tsv)

# Configure Federated Credentials
az ad app federated-credential create \
  --id $OBJECT_ID \
  --parameters '{
    \"name\": \"github-actions\",
    \"issuer\": \"https://token.actions.githubusercontent.com\",
    \"subject\": \"repo:YOUR_GITHUB_USERNAME/FoodHub:ref:refs/heads/main\",
    \"audiences\": [\"api://AzureADTokenExchange\"]
  }'

# Create Service Principal
az ad sp create --id $APP_ID

# Get Subscription ID
SUBSCRIPTION_ID=$(az account show --query \"id\" -o tsv)

# Assign Contributor role to Service Principal
az role assignment create \
  --assignee $APP_ID \
  --role Contributor \
  --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/foodhub-rg

# Output values for GitHub Variables
echo \"AZURE_CLIENT_ID: $APP_ID\"
echo \"AZURE_TENANT_ID: $(az account show --query tenantId -o tsv)\"
echo \"AZURE_SUBSCRIPTION_ID: $SUBSCRIPTION_ID\"
echo \"ACR_NAME: foodhubacr\"
echo \"AKS_RESOURCE_GROUP: foodhub-rg\"
echo \"AKS_CLUSTER_NAME: foodhub-aks\"
```

---

## Pipeline Flow

1. **Trigger:** Push to `main` branch
2. **Checkout:** Clone repository code
3. **Docker Buildx:** Setup for linux/amd64 cross-compilation
4. **Azure Login:** OIDC authentication (no password/secret)
5. **Build Image:** Multi-stage Docker build targeting linux/amd64
6. **Tag Image:** Commit SHA + `latest` tags
7. **Push to ACR:** Upload both tags to Azure Container Registry
8. **AKS Credentials:** Get kubectl config
9. **Deploy:** Replace ACR placeholder, apply manifests, rollout
10. **Verify:** Wait for pods to be ready (5 min timeout)
11. **Status:** Show pods and LoadBalancer external IP

---

## Platform Targeting

**Critical:** `--platform linux/amd64` flag ensures image runs on AKS AMD64 nodes.

Without this:
- ❌ Image might build for GitHub runner's architecture (ARM64)
- ❌ AKS pods fail with \"exec format error\"
- ❌ Binary mismatch between build and runtime architecture

With this:
- ✅ Cross-compilation via QEMU emulation
- ✅ Works on any AKS node architecture
- ✅ Production-ready deployment

---

## Monitoring Deployment

### GitHub Actions UI:
- View workflow runs
- See real-time logs
- Check deployment status

### kubectl Commands:
```bash
# Get pods
kubectl get pods -l app=foodhub-api

# View logs
kubectl logs -l app=foodhub-api --tail=50 -f

# Describe pod
kubectl describe pod -l app=foodhub-api

# Get service external IP
kubectl get service foodhub-api
```

### Access Application:
1. Get external IP from `kubectl get service foodhub-api`
2. Wait for LoadBalancer provisioning (2-5 minutes)
3. Access: `http://<EXTERNAL-IP>/graphql`

---

## Success Criteria

- Pipeline runs successfully on push to main
- Docker image builds for linux/amd64
- Image pushed to ACR with commit SHA tag
- Kubernetes deployment updates automatically
- Pods start successfully without errors
- Service gets external IP
- GraphQL endpoint accessible via LoadBalancer
- Zero secrets stored in GitHub
