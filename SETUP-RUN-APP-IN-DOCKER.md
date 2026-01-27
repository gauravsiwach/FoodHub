# Setup: Run FoodHub App in Docker with Azure Key Vault Access

This document explains how to run the **FoodHub** application locally using **Docker**, while securely accessing **Azure Key Vault** secrets via an Azure Service Principal.

---

## Why This Setup Is Required

When the application runs inside a Docker container, it does **not** have access to:
- Visual Studio Azure login
- Azure CLI login
- Managed Identity (Azure-only feature)

Therefore, Docker must be explicitly provided with an **Azure identity** to access Azure Key Vault.

---

## Solution Overview

1. Use an **Azure Service Principal** as the application identity
2. Assign the Service Principal permission to read secrets from Azure Key Vault
3. Pass Azure identity details to Docker as environment variables
4. Use `DefaultAzureCredential` in the application (no code change required)

---

## Prerequisites

- Docker Desktop installed and running
- Azure subscription
- Azure Key Vault already created
- Application Docker image built (`foodhub-api:latest`)

---

## Step 1: Azure Service Principal

A Service Principal represents the application identity in Azure.

Example Service Principal:
```
foodhub-github-actions
```

This Service Principal can be reused for:
- Local Docker runs
- CI/CD pipelines (GitHub Actions)

---

## Step 2: Assign Azure Key Vault Permission

If the Key Vault uses **Azure RBAC**, assign the role using IAM.

### Required Role
```
Key Vault Secrets User
```

### Where to Assign
Azure Portal → Key Vault → **Access control (IAM)** → Role assignments

Ensure the role is assigned **at the Key Vault scope**.

> RBAC role propagation can take a few minutes.

---

## Step 3: Required Environment Variables

The Docker container requires the following environment variables:

| Variable Name | Description |
|--------------|------------|
| `AZURE_CLIENT_ID` | Application (client) ID |
| `AZURE_CLIENT_SECRET` | Client Secret VALUE |
| `AZURE_TENANT_ID` | Directory (tenant) ID |
| `KeyVaultUrl` | Azure Key Vault URL |

---

## Step 4: Run Application in Docker (Windows CMD)

```cmd
docker rm -f foodhub-api & docker run -d -p 5000:8080 ^
--name foodhub-api ^
-e AZURE_CLIENT_ID=<CLIENT_ID> ^
-e AZURE_CLIENT_SECRET=<CLIENT_SECRET> ^
-e AZURE_TENANT_ID=<TENANT_ID> ^
-e KeyVaultUrl=https://<KEY_VAULT_NAME>.vault.azure.net/ ^
foodhub-api:latest
```

---

## Verification

```cmd
docker ps
docker logs foodhub-api
```

---

## Security Notes

- Never commit secrets to source control
- Rotate client secrets periodically
- Use Managed Identity when deploying to Azure

---
 
docker rm -f foodhub-api

