# AI-One Deployment Guide

This guide provides step-by-step instructions for deploying the AI-One application on Azure.

The deployment process provisions and configures the following solution components:

- **Platform**: Core infrastructure and shared services
- **API**: Backend services
- **Web App**: Frontend application for user interaction

## 1. Infrastructure as Code (IaC)

The infrastructure resources are defined using modular Bicep templates. All the templates and supporting scripts are under the `infra` directory of the project repository:

```plaintext
infra
├── platform
├── backend
├── frontend
├── scripts
└── modules
```

- `platform`: Platform Bicep code & parameter files
- `backend`: API app Bicep code & parameter files
- `frontend`: Web app Bicep code & parameter files
- `scripts`: Deployment and utility scripts
- `modules`: Reusable Bicep modules (e.g., Key Vault, Cosmos DB etc.)

### 1.1 Platform Infrastructure

The platform infrastructure is defined in the `infra/platform` folder. This folder contains the Bicep files that provision the core resources required for the AI-One platform.

| Resource                | Resource Type                                   | Purpose                                                      |
|-----------------------------|-------------------------------------------------|--------------------------------------------------------------|
| Log Analytics Workspace      | Microsoft.OperationalInsights/workspaces        | Centralized logging and monitoring                           |
| Key Vault                    | Microsoft.KeyVault/vaults                       | Secure storage for secrets and keys                          |
| Storage Account              | Microsoft.Storage/storageAccounts               | General-purpose storage for the platform                     |
| Azure AI Search              | Microsoft.Search/searchServices                 | Provides search capabilities                                 |
| App Service Plan             | Microsoft.Web/serverfarms                      | Hosts web applications and APIs                              |
| Cosmos DB Account            | Microsoft.DocumentDB/databaseAccounts           | Globally distributed NoSQL database for application data     |
| Document Intelligence        | Microsoft.CognitiveServices/accounts            | AI-powered document analysis                                 |
| Service Bus                  | Microsoft.ServiceBus/namespaces                 | Messaging and eventing between services                      |
| Azure OpenAI Service         | Microsoft.CognitiveServices/accounts            | Access to Azure OpenAI models                                |

### 1.2 API Infrastructure

The API infrastructure is defined in the `infra/backend` folder and provisions the resources required to host Agile.Chat API. The following resources are created by the `apiapp.bicep` file:

| Resource      | Resource Type                                         | Purpose                                         |
|----------------------------|-------------------------------------------------------|-------------------------------------------------|
| API App                    | Microsoft.Web/sites                                   | Hosts the Agile.Chat API application            |
| Service Bus Queue          | Microsoft.ServiceBus/namespaces/queues                | Queue for managing indexing requests            |
| Cosmos DB Database         | Microsoft.DocumentDB/databaseAccounts/sqlDatabases    | Stores chat history and configuration           |
| Event Grid System Topic    | Microsoft.EventGrid/systemTopics                      | Eventing for blob storage events                |
| Event Grid Subscription    | Microsoft.EventGrid/systemTopics/eventSubscriptions   | Routes blob events to Service Bus               |
| Application Insights       | Microsoft.Insights/components                         | Application monitoring and telemetry            |
| Blob Containers            | Microsoft.Storage/storageAccounts/blobServices/containers | Storage containers for API data             |

### 1.3 Web App Infrastructure

The web app infrastructure is defined in the `infra/frontend` folder and provisions the resources required to host the Agile.Chat frontend application. The following resources are created by the `webapp.bicep` file:

| Resource      | Resource Type                 | Purpose                                 |
|----------------------------|-------------------------------|-----------------------------------------|
| Web App                    | Microsoft.Web/sites           | Hosts the Agile.Chat frontend application|

## 2. Deployment Steps

1. Create App Registration in Azure Active Directory (Entra ID). The registered application will be used for the below mentioned requirements.

    - Authorize CI/CD pipelines to deploy resources.
    - Secure access to AI-One API using OAuth 2.0 authentication.

    Follow the [Application Registration Guide](app_registration.md) to complete this step.

    You would need a new app registration for each environment (e.g., `dev`, `tst`, `uat`, `prod`) to isolate resources and permissions.
  
2. AI-One deployment supports both Azure DevOps and GitHub Actions.

    Based on the choice of CI/CD platform, follow the respective deployment steps:

    - [Deployment Steps Using GitHub](github_steps.md)
    - [Deployment Steps Using Azure DevOps](azure_devops_steps.md)

3. Update the redirect URI in the app registration after deployment. The redirect URI is the URL where Azure AD will send authentication responses. It should match the URL of your deployed web app. 

    e.g., `https://ag-aione-dev-webapp.azurewebsites.net`.