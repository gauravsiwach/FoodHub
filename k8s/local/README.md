# FoodHub Kubernetes - Local Development

## Prerequisites

- Docker Desktop with Kubernetes enabled
- kubectl CLI installed
- Local Docker image built

## Quick Start

```bash
# 1. Build Docker image
docker build -t foodhub-api:latest .

# 2. Deploy to local Kubernetes
kubectl apply -f k8s/local/

# 3. Verify deployment
kubectl get pods -l app=foodhub-api

# 4. Access the application
open http://localhost:30080/graphql
```

## Deployment Commands

```bash
# Apply all manifests
kubectl apply -f k8s/local/

# Or apply individually
kubectl apply -f k8s/local/deployment.yaml
kubectl apply -f k8s/local/service.yaml
```

## Verify Deployment

```bash
# Check deployment status
kubectl get deployments foodhub-api

# Check pod status and logs
kubectl get pods -l app=foodhub-api
kubectl logs -l app=foodhub-api -f

# Check service
kubectl get service foodhub-api

# View all resources
kubectl get all -l app=foodhub-api
```

## Access Options

### Option 1: NodePort (Recommended for Local)
Direct access via NodePort 30080:
```bash
http://localhost:30080/graphql
```

### Option 2: Port Forward
Alternative method:
```bash
kubectl port-forward service/foodhub-api 8080:8080
```
Then access: `http://localhost:8080/graphql`

## Testing

```bash
# Health check
curl http://localhost:30080/graphql

# GraphQL introspection query
curl -X POST http://localhost:30080/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "{ __schema { queryType { name } } }"}'
```

## Update After Code Changes

```bash
# Rebuild image
docker build -t foodhub-api:latest .

# Restart pods to use new image
kubectl delete pod -l app=foodhub-api

# Or rollout restart
kubectl rollout restart deployment/foodhub-api
```

## Cleanup

```bash
# Delete all local resources
kubectl delete -f k8s/local/
```

## Troubleshooting

### ImagePullBackOff Error
```bash
# Verify image exists locally
docker images | grep foodhub-api

# Should show: foodhub-api   latest   ...
```

### Pod not starting
```bash
# Check pod events
kubectl describe pod -l app=foodhub-api

# Check logs
kubectl logs -l app=foodhub-api --tail=50
```

### Service not accessible
```bash
# Verify endpoints are ready
kubectl get endpoints foodhub-api

# Should show pod IP and port
```

## Configuration Details

- **Environment**: Development
- **Replicas**: 1 (single pod)
- **Service Type**: NodePort
- **External Port**: 30080
- **Container Port**: 8080
- **Image Pull Policy**: Never (uses local Docker image)
- **Health Endpoint**: /graphql
