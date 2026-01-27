# FoodHub Kubernetes - Production (AKS)

## Prerequisites

- Azure Container Registry (ACR) configured
- AKS cluster provisioned
- kubectl configured for AKS cluster
- Docker image pushed to ACR

## Configuration Required

Before deploying, update `deployment.yaml`:

```yaml
image: __ACR_NAME__.azurecr.io/foodhub-api:latest
```

Replace `__ACR_NAME__` with your actual Azure Container Registry name.

## Build and Push Image

```bash
# Set your ACR name
ACR_NAME="your-acr-name"

# Login to ACR
az acr login --name $ACR_NAME

# Build and tag image
docker build -t $ACR_NAME.azurecr.io/foodhub-api:latest .

# Push to ACR
docker push $ACR_NAME.azurecr.io/foodhub-api:latest
```

## Deploy to AKS

```bash
# Connect to AKS cluster
az aks get-credentials --resource-group <rg-name> --name <aks-name>

# Deploy application
kubectl apply -f k8s/prod/

# Verify deployment
kubectl get deployments foodhub-api
kubectl get pods -l app=foodhub-api
kubectl get service foodhub-api
```

## Access the Application

```bash
# Get external IP (may take a few minutes)
kubectl get service foodhub-api --watch

# Once EXTERNAL-IP is assigned
EXTERNAL_IP=$(kubectl get service foodhub-api -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "Application URL: http://$EXTERNAL_IP/graphql"
```

## Monitoring

```bash
# View logs
kubectl logs -l app=foodhub-api -f

# Check pod health
kubectl get pods -l app=foodhub-api
kubectl describe pod -l app=foodhub-api

# View events
kubectl get events --sort-by=.metadata.creationTimestamp
```

## Scaling

```bash
# Manual scaling
kubectl scale deployment foodhub-api --replicas=3

# Check status
kubectl get deployment foodhub-api
```

## Update Deployment

```bash
# Build and push new image
docker build -t $ACR_NAME.azurecr.io/foodhub-api:v1.1 .
docker push $ACR_NAME.azurecr.io/foodhub-api:v1.1

# Update deployment image
kubectl set image deployment/foodhub-api \
  foodhub-api=$ACR_NAME.azurecr.io/foodhub-api:v1.1

# Check rollout status
kubectl rollout status deployment/foodhub-api

# Rollback if needed
kubectl rollout undo deployment/foodhub-api
```

## Cleanup

```bash
# Delete resources
kubectl delete -f k8s/prod/
```

## Configuration Details

- **Environment**: Production
- **Replicas**: 1 (scale as needed)
- **Service Type**: LoadBalancer (Azure Load Balancer)
- **External Port**: 80
- **Container Port**: 8080
- **Image Source**: Azure Container Registry
- **Health Endpoint**: /graphql

## Security Considerations

1. Use Azure Key Vault for secrets
2. Enable network policies
3. Configure ingress with TLS
4. Use managed identities for ACR access
5. Enable pod security policies
6. Configure resource quotas and limits
