# Prompt 7: Azure Key Vault Integration
## Overview
Integrate Azure Key Vault using DefaultAzureCredential for secure configuration.
## Package
`xml
<PackageReference Include=\"Azure.Extensions.AspNetCore.Configuration.Secrets\" Version=\"1.3.2\" />
`
## Program.cs Configuration
`csharp
// Production only
if (!builder.Environment.IsDevelopment())
{
    var keyVaultEndpoint = builder.Configuration[\"KeyVault:Endpoint\"];
    if (!string.IsNullOrEmpty(keyVaultEndpoint))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultEndpoint),
            new DefaultAzureCredential());
    }
}
`
## Key Vault Secrets
Store in Azure Key Vault:
- Cosmos--Key
- ConnectionStrings--DefaultConnection
- GoogleAuth--ClientId
- GoogleAuth--Aud
- Jwt--Secret
## Local Development
Use appsettings.Development.json for local secrets.
## DefaultAzureCredential Chain
1. EnvironmentCredential (AZURE_CLIENT_ID, etc.)
2. ManagedIdentityCredential (Azure services)
3. AzureCliCredential (local development)
## Success Criteria
- Secrets loaded from Key Vault in production
- Local development uses appsettings
- No secrets in source control
