# Northwind Architecture Diagram

This diagram shows the Azure architecture for the modernised Northwind application.

```mermaid
graph TD
    User[👤 User] --> AppService[App Service\nNorthwind Web App]
    AppService --> |Managed Identity| AzureSQL[Azure SQL\nNorthwind DB]
    AppService --> |Managed Identity| AzureOpenAI[Azure OpenAI\nGPT-4o]
    AppService --> |Managed Identity| AISearch[Azure AI Search]
    ChatUI[Chat UI\nAI Assistant] --> AppService
    AzureOpenAI --> AISearch
    
    subgraph Azure["Azure Resource Group"]
        AppService
        AzureSQL
        AzureOpenAI
        AISearch
        MI[User-Assigned\nManaged Identity]
    end
    
    MI -.-> AppService
    MI -.-> AzureSQL
    MI -.-> AzureOpenAI
    MI -.-> AISearch
```

## Components

| Component | SKU | Region |
|-----------|-----|--------|
| App Service Plan | Standard S1 | UK South |
| App Service | ASP.NET Core .NET 8 | UK South |
| Azure SQL Server | Entra ID Only Auth | UK South |
| Azure SQL Database | Basic Tier | UK South |
| Azure OpenAI | S0 | Sweden Central |
| Azure AI Search | Standard S0 | UK South |
| User-Assigned Managed Identity | - | UK South |

## Security Architecture

- **No passwords or API keys** — everything uses Managed Identity
- App Service authenticates to SQL using `Authentication=Active Directory Managed Identity`
- App Service authenticates to Azure OpenAI using `ManagedIdentityCredential`
- App Service authenticates to AI Search using `ManagedIdentityCredential`
- SQL Server configured with `azureADOnlyAuthentication: true`

## Deployment Flow

```
az group create → az deployment group create (main.bicep)
  → App Service + Managed Identity
  → Azure SQL (Entra ID admin, Basic DB)
  → (Optional) Azure OpenAI + AI Search
→ Configure App Service settings
→ Wait for SQL (30s) + Firewall rules
→ python3 run-sql.py (schema)
→ python3 run-sql-dbrole.py (roles)
→ python3 run-sql-stored-procs.py (procedures)
→ az webapp deploy (app.zip)
```
