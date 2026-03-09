# Azure Services Architecture Diagram

This diagram shows the architecture of the Expense Management App deployed on Azure.

```mermaid
graph TB
    subgraph "Developer Machine"
        DEV["🖥️ Developer<br/>deploy.sh / deploy-with-chat.sh"]
        PY["🐍 Python Scripts<br/>(run-sql*.py)"]
    end

    subgraph "UKSOUTH Region"
        subgraph "App Service"
            APP["⚙️ Azure App Service<br/>(S1 SKU)<br/>ASP.NET Core .NET 8<br/>Razor Pages + Web API"]
        end

        subgraph "Identity"
            MI["🔑 User-Assigned<br/>Managed Identity<br/>mid-AppModAssist-*"]
        end

        subgraph "Data"
            SQL["🗄️ Azure SQL Database<br/>ExpenseMgmt<br/>(Azure AD-only auth)"]
        end

        subgraph "AI Search"
            SEARCH["🔍 Azure AI Search<br/>(Basic SKU)"]
        end
    end

    subgraph "Sweden Central Region"
        subgraph "GenAI (optional)"
            AOAI["🤖 Azure OpenAI<br/>GPT-4o<br/>(Capacity 8)"]
        end
    end

    subgraph "Users"
        USER["👤 End User<br/>Browser"]
    end

    USER -->|"HTTPS /Index"| APP
    USER -->|"HTTPS /chatui"| APP
    APP -->|"Assigned to"| MI
    MI -->|"Active Directory<br/>Managed Identity auth"| SQL
    MI -->|"Cognitive Services<br/>OpenAI User role"| AOAI
    MI -->|"Search Index Data<br/>Contributor role"| SEARCH
    APP -->|"Function calling + RAG"| AOAI
    APP -->|"Vector search"| SEARCH
    DEV -->|"az deployment group create"| APP
    DEV -->|"az deployment group create"| SQL
    DEV -->|"az deployment group create"| MI
    PY -->|"pyodbc + AzureCliCredential"| SQL

    style MI fill:#f0f4fa,stroke:#0f6cbd
    style APP fill:#e8f5e9,stroke:#2e7d32
    style SQL fill:#fff3e0,stroke:#f57c00
    style AOAI fill:#fce4ec,stroke:#c62828
    style SEARCH fill:#e8eaf6,stroke:#3949ab
```

## Resource Summary

| Resource | Type | Region | SKU |
|---|---|---|---|
| App Service Plan | Microsoft.Web/serverfarms | uksouth | S1 |
| App Service | Microsoft.Web/sites | uksouth | - |
| User-Assigned MI | Microsoft.ManagedIdentity | uksouth | - |
| SQL Server | Microsoft.Sql/servers | uksouth | - |
| SQL Database | Microsoft.Sql/databases | uksouth | Basic |
| Azure OpenAI | Microsoft.CognitiveServices/accounts | swedencentral | S0 |
| AI Search | Microsoft.Search/searchServices | uksouth | Basic |

## Authentication Flow

```
App Service
    └── Managed Identity (mid-AppModAssist-*)
            ├── → Azure SQL: Active Directory Managed Identity (User Id=<clientId>)
            ├── → Azure OpenAI: Cognitive Services OpenAI User role via ManagedIdentityCredential
            └── → AI Search: Search Index Data Contributor + Search Service Contributor roles
```

## Deployment Order

1. Deploy Bicep (`infra/main.bicep`) — creates all resources
2. Configure App Service connection string with Managed Identity client ID
3. Add local machine IP to SQL firewall
4. Wait 30 seconds for SQL to be ready
5. Run `run-sql.py` — applies database schema
6. Run `run-sql-dbrole.py` — grants managed identity `db_datareader`, `db_datawriter`, `EXECUTE`
7. Run `run-sql-stored-procs.py` — deploys stored procedures
8. `dotnet publish` + zip + `az webapp deploy`
