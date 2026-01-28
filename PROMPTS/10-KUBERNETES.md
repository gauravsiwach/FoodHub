# Prompt 10: Kubernetes Deployment

## Overview
Create Kubernetes manifests for local (Docker Desktop) and production (AKS) deployments.

---

## Directory Structure

```
k8s/
├── local/              # Local Kubernetes (Docker Desktop)
│   ├── deployment.yaml
│   ├── service.yaml
│   └── README.md
└── prod/               # Production AKS
    ├── deployment.yaml
    ├── service.yaml
    └── README.md
```

---

## Local Deployment (k8s/local/deployment.yaml)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: foodhub-api
  labels:
    app: foodhub-api
spec:
  replicas: 1
  selector:
    matchLabels:
      app: foodhub-api
  template:
    metadata:
      labels:
        app: foodhub-api
    spec:
      containers:
      - name: foodhub-api
        image: foodhub-api:latest
        imagePullPolicy: Never
        ports:
        - containerPort: 8080
          name: http
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: \"Development\"
        - name: ASPNETCORE_URLS
          value: \"http://+:8080\"
        - name: AZURE_CLIENT_ID
          value: \"your-client-id\"
        - name: AZURE_CLIENT_SECRET
          value: \"your-client-secret\"
        - name: AZURE_TENANT_ID
          value: \"your-tenant-id\"
        resources:
          requests:
            memory: \"256Mi\"
            cpu: \"250m\"
          limits:
            memory: \"512Mi\"
            cpu: \"500m\"
        livenessProbe:
          httpGet:
            path: /graphql
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /graphql
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
```

## Local Service (k8s/local/service.yaml)

```yaml
apiVersion: v1
kind: Service
metadata:
  name: foodhub-api
  labels:
    app: foodhub-api
spec:
  type: NodePort
  ports:
  - port: 8080
    targetPort: 8080
    nodePort: 30080
    protocol: TCP
    name: http
  selector:
    app: foodhub-api
```

**Access:** http://localhost:30080/graphql

---

## Production Deployment (k8s/prod/deployment.yaml)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: foodhub-api
  labels:
    app: foodhub-api
spec:
  replicas: 1
  selector:
    matchLabels:
      app: foodhub-api
  template:
    metadata:
      labels:
        app: foodhub-api
    spec:
      containers:
      - name: foodhub-api
        image: __ACR_NAME__.azurecr.io/foodhub-api:latest
        ports:
        - containerPort: 8080
          name: http
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: \"Production\"
        - name: ASPNETCORE_URLS
          value: \"http://+:8080\"
        resources:
          requests:
            memory: \"256Mi\"
            cpu: \"250m\"
          limits:
            memory: \"512Mi\"
            cpu: \"500m\"
        livenessProbe:
          httpGet:
            path: /graphql
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /graphql
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
```

## Production Service (k8s/prod/service.yaml)

```yaml
apiVersion: v1
kind: Service
metadata:
  name: foodhub-api
  labels:
    app: foodhub-api
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 8080
    protocol: TCP
    name: http
  selector:
    app: foodhub-api
```

**Access:** External IP provided by Azure LoadBalancer

---

## Local Deployment Commands

```bash
# Apply manifests
kubectl apply -f k8s/local/deployment.yaml
kubectl apply -f k8s/local/service.yaml

# Verify deployment
kubectl get pods -l app=foodhub-api
kubectl get services

# View logs
kubectl logs -l app=foodhub-api -f

# Delete resources
kubectl delete -f k8s/local/
```

---

## Key Differences: Local vs Production

| Feature | Local | Production |
|---------|-------|------------|
| Image Source | Local Docker | Azure Container Registry |
| Service Type | NodePort | LoadBalancer |
| Replicas | 1 | 1 (scalable) |
| Image Pull | Never | IfNotPresent |
| Environment | Development | Production |
| Access | localhost:30080 | External IP |
| Secrets | Environment variables | Azure Key Vault + Managed Identity |

---

## Success Criteria

- Local deployment works on Docker Desktop Kubernetes
- Service accessible at localhost:30080
- Health probes working correctly
- Resource limits enforced
- Production manifests ready for AKS
- ACR placeholder in production deployment
