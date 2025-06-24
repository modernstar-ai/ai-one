# AI-One Deployment Guide

This guide provides step-by-step instructions for deploying the AI-One application on Azure.

The deployment process provisions and configures the following solution components:

- **Platform**: Core infrastructure and shared services
- **API**: Backend services
- **Web App**: Frontend application for user interaction

## Infrastructure as Code (IaC)

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

### Platform Infrastructure

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

### API Infrastructure

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

### Web App Infrastructure

The web app infrastructure is defined in the `infra/frontend` folder and provisions the resources required to host the Agile.Chat frontend application. The following resources are created by the `webapp.bicep` file:

| Resource      | Resource Type                 | Purpose                                 |
|----------------------------|-------------------------------|-----------------------------------------|
| Web App                    | Microsoft.Web/sites           | Hosts the Agile.Chat frontend application|

## Deployment Steps

AI-One deployment supports both Azure DevOps and GitHub Actions.

### GitHub

Deployment is managed using GitHub Actions, defined in the `.github/workflows` directory of the project repository.

The deployment process includes the following steps:

- **Deploy AI-One Platform Infrastructure**: Provisions the core Azure resources and shared services required for the solution.
- **Deploy AI-One API**: Provisions the backend resources and deploys the Agile.Chat API application to Azure.
- **Deploy AI-One Web App**: Provisions the frontend resources and deploys the Agile.Chat web application to Azure.

---

#### Set Up Source Code Repository

Fork or copy [agile-chat](https://github.com/agile-internal/agile-chat) repository to a new project repository in Customer's GitHub organization.

---

#### Create Environments

The deployment is designed to work with multiple environments (e.g., dev, test, uat, and prod).
Create the required environments in the new project repository on GitHub.

The environments will be used to manage environment-specific configurations and secrets.

***To create environments in GitHub:***

1. Navigate to the new project repository on GitHub.
2. Click on the **Settings** tab.
3. In the left sidebar, select **Environments** under the **Code and automation** section.
4. Click the **New environment** button and enter a name (e.g., `dev`, `test`, `uat`, `prod`).
5. Repeat step 4 for each environment required.

---

#### Create Environment Variables

Create the following environment variables in each environment created in the previous step.

***To add environment variables to an environment:***

1. In your repository, go to **Settings** > **Environments** and select the environment to configure.
2. Under the **Environment secrets and variables** section, click **Add variable**.
3. Add each of the following variables (repeat for each environment as needed):

    - `AZURE_SUBSCRIPTION_ID`: The unique identifier of the Azure subscription.
    - `AZURE_TENANT_ID`: The Azure Active Directory (AAD) tenant ID associated with the organization.
    - `AZURE_CLIENT_ID`: The application (client) ID of the Azure Service Principal used for authentication and deployment automation.
    - `AZURE_RESOURCE_GROUP`: The name of the Azure resource group.
    - `VITE_AGILECHAT_API_URL`: The base URL for the Agile.Chat API. e.g., `https://ag-aione-apiapp.azurewebsites.net`.

4. Repeat step 3 for each environment to support.

---

#### Platform Deployment

```plaintext
infra
├── platform
    ├── platform.bicep
    ├── [env].bicepparam

```

- `platform.bicep`: The bicep file that defines the platform infrastructure resources.
- `[env].bicepparam`: Environment-specific parameter file for the platform deployment.

##### Configure Parameters

For every environment required, define in GitHub (such as `dev`, `test`, `uat`, or `prod`), create a corresponding parameter file (e.g., `dev.bicepparam`, `test.bicepparam`) in the `infra/platform` directory.

The parameter file name must exactly match the environment name. These files provide environment-specific values that the GitHub Action uses during deployment.

| Parameter Name      | Description                                                                                                 | Default Value      |
|---------------------|-------------------------------------------------------------------------------------------------------------|--------------------|
| `environmentName`   | The name of the deployment environment (should match the environment, e.g., `dev`, `test`).                 | (none)             |
| `projectName`       | The project name.                                                                                           | `ag-aione`         |
| `location`          | Azure region for resource deployment.                                                                       | `australiaeast`    |
| `azureClientId`     | Azure Service Principal Client ID for authentication.                                                       | (none)             |
| `azureTenantId`     | Azure Active Directory Tenant ID.                                                                           | (none)             |
| `openAILocation`    | Azure region for OpenAI resources.                                                                         | `australiaeast`    |
| `deployAzureOpenAi` | Set to `true` to provision Azure OpenAI Service, or `false` to skip deployment.                            | `true`             |
| `deployOpenAiModels`| Set to `true` to deploy OpenAI models, or `false` to skip model deployment.                                | `true`             |
| `semanticSearchSku` | SKU for Azure AI Search.                                                                                   | `standard`         |
| `networkIsolation`  | Set to `true` to enable private network isolation, or `false` for public access.                           | `false`            |

**Example: `dev.bicepparam`**

```bicep

using './platform.bicep'

param environmentName = 'dev'
param projectName = 'ag-aione'
param location = 'australiaeast'
param azureClientId = '<your-client-id>'
param azureTenantId = '<your-tenant-id>'
param openAILocation = 'australiaeast'
param deployAzureOpenAi = true
param deployOpenAiModels = true
param semanticSearchSku = 'standard'
param networkIsolation = false
```

> **Tip:**  
> Keep sensitive values secure and never commit secrets to source control.  
> Use GitHub environment secrets for all sensitive data such as client IDs, tenant IDs, and API keys.

---

##### Run Platform Deployment GitHub Action

The platform infrastructure is deployed using the `.github/workflows/deploy-platform.yml` GitHub Action.

***To deploy the platform infrastructure:***

1. Navigate to the **Actions** tab in your GitHub repository.
2. Find and select the **Deploy AI-One Platform Infrastructure** workflow.
3. Click **Run workflow**.
4. Fill in the required inputs:
    - **Project Name**: The name of your project (e.g., `ag-aione`).
    - **Environment**: The target environment (e.g., `dev`, `test`, `uat`, or `prod`).
    - **Location**: The Azure region for deployment (e.g., `australiaeast`).
5. Click **Run workflow** to start deployment.

The workflow will use the appropriate parameter file (e.g., `dev.bicepparam`) based on the selected environment, and deploy the platform infrastructure resources defined in `platform.bicep`.

---

#### API Deployment

```plaintext
infra
├── backend
    ├── apiapp.bicep
    ├── [env].bicepparam
```

- `apiapp.bicep`: The Bicep file that defines the API infrastructure resources.
- `[env].bicepparam`: Environment-specific parameter file for the API deployment.

The API infrastructure is deployed using the `.github/workflows/deploy-backend.yml` GitHub Action. This action provisions the backend Azure resources and deploys the Agile.Chat API application.

##### Configure API Parameters

For each environment (such as `dev`, `test`, `uat`, or `prod`), create a corresponding parameter file (e.g., `dev.bicepparam`, `test.bicepparam`) in the `infra/backend` directory.

The parameter file name must exactly match the environment name. These files provide environment-specific values for the API deployment.

| Parameter Name                | Description                                                                                 | Default Value                |
|-------------------------------|---------------------------------------------------------------------------------------------|------------------------------|
| `environmentName`             | The name of the deployment environment (should match the environment, e.g., `dev`, `test`). | (none)                       |
| `projectName`                 | The project name.                                                                           | `ag-aione`                   |
| `location`                    | Azure region for resource deployment.                                                        | `australiaeast`              |
| `azureClientId`               | Azure Service Principal Client ID for authentication.                                        | (none)                       |
| `azureTenantId`               | Azure Active Directory Tenant ID.                                                            | (none)                       |
| `apiAppName`                  | Name of the API App Service.                                                                | (none)           |
| `appServicePlanName`          | Name of the App Service Plan.                                                               | (none)           |
| `applicationInsightsName`     | Name of the Application Insights resource.                                                  | (none)           |
| `logAnalyticsWorkspaceName`   | Name of the Log Analytics Workspace.                                                        | (none)           |
| `keyVaultName`                | Name of the Key Vault.                                                                      | (none)           |
| `storageName`                 | Name of the Storage Account.                                                                | (none)           |
| `storageAccountName`          | Name of the existing Storage Account.                                                       | (none)           |
| `formRecognizerName`          | Name of the Form Recognizer resource.                                                       | (none)           |
| `openAiName`                  | Name of the Azure OpenAI resource.                                                          | (none)           |
| `openAiApiVersion`            | API version for Azure OpenAI.                                                               | (none)           |
| `searchServiceName`           | Name of the Azure Search Service.                                                           | (none)           |
| `serviceBusName`              | Name of the Service Bus namespace.                                                          | (none)           |
| `serviceBusQueueName`         | Name of the Service Bus queue.                                                              | (none)           |
| `cosmosDbAccountName`         | Name of the Cosmos DB account.                                                              | (none)           |
| `cosmosDbAccountEndpoint`     | Endpoint for the Cosmos DB account.                                                         | (none)           |
| `allowedOrigins`              | Allowed origins for CORS.                                                                   | `[]`                         |
| `adminEmailAddresses`         | Array of admin email addresses.                                                             | `[]`                         |
| `aspCoreEnvironment`          | Deployment environment for ASP.NET Core.                                                    | (none)               |
| `eventGridName`               | Name of the Event Grid system topic.                                                        | (none)           |

**Example: `dev.bicepparam`**

```bicep

using './apiapp.bicep'

param environmentName = 'dev'
param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Development'
param appServicePlanName = 'ag-aione-dev-app'
param applicationInsightsName = 'ag-aione-dev-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-dev-la'
param keyVaultName = 'ag-aione-dev-kv'
param storageName = 'agaionedevsto'
param storageAccountName = 'agaionedevsto'
param formRecognizerName = 'ag-aione-dev-form'
param openAiName = 'ag-aione-dev-aillm'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aione-dev-search'
param serviceBusName = 'ag-aione-dev-service-bus'
param cosmosDbAccountName = 'ag-aione-dev-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aione-dev-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aione-dev-blob-eg'

param allowedOrigins = ['https://ag-aione-dev-webapp.azurewebsites.net']

param adminEmailAddresses = ['adam-stephensen@agile-analytics.com.au']
```

---

##### Run API Deployment GitHub Action

To deploy the API infrastructure and application:

1. Navigate to the **Actions** tab in your GitHub repository.
2. Find and select the **Deploy AI-One Api** workflow.
3. Click **Run workflow**.
4. Fill in the required inputs:
    - **Environment Name**: The target environment (e.g., `dev`, `test`, `uat`, or `prod`).
    - **Api App Name**: The name of your API App Service (e.g., `ag-aione-dev-apiapp`).
5. Click **Run workflow** to start deployment.

The workflow will use the appropriate parameter file (e.g., `dev.bicepparam`) based on the selected environment, and deploy the API infrastructure and application as defined in `apiapp.bicep`.

---

#### Web App Deployment

```plaintext
infra
├── frontend
    ├── webapp.bicep
    ├── [env].bicepparam
```

- `webapp.bicep`: The Bicep file that defines the web app infrastructure resources.
- `[env].bicepparam`: Environment-specific parameter file for the web app deployment.

The web app infrastructure is deployed using a GitHub Actions workflow (e.g., `.github/workflows/deploy-frontend.yml`). This workflow provisions the frontend Azure resources and deploys the Agile.Chat web application.

##### Configure Web App Parameters

For each environment (such as `dev`, `test`, `uat`, or `prod`), create a corresponding parameter file (e.g., `dev.bicepparam`, `test.bicepparam`) in the `infra/frontend` directory. The parameter file name must exactly match the environment name. These files provide environment-specific values for the web app deployment.

| Parameter Name            | Description                                                      | Default Value                |
|--------------------------|------------------------------------------------------------------|------------------------------|
| `environmentName`         | The name of the deployment environment (should match the environment, e.g., `dev`, `test`). | (none)                       |
| `projectName`             | The project name.                                                | `ag-aione`                   |
| `location`                | Azure region for resource deployment.                            | `australiaeast`              |
| `tags`                    | Resource tags (loaded from tags.json).                           | `{}`                         |
| `appServicePlanName`      | Name of the App Service Plan.                                    | (none)                       |
| `apiAppName`              | Name of the API App Service.                                     | (none)                       |
| `logAnalyticsWorkspaceName`| Name of the Log Analytics Workspace.                             | (none)                       |

**Example: `dev.bicepparam`**

```bicep
using './webapp.bicep'

param environmentName = 'dev'
param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')
param appServicePlanName = 'ag-aione-dev-app'
param apiAppName = 'ag-aione-dev-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-dev-la'
```

---

##### Run Web App Deployment GitHub Action

To deploy the web app infrastructure and application:

1. Navigate to the **Actions** tab in your GitHub repository.
2. Find and select the **Deploy AI-One Web App** workflow.
3. Click **Run workflow**.
4. Fill in the required inputs:
    - **Environment Name**: The target environment (e.g., `dev`, `test`, `uat`, or `prod`).
    - **Web App Name**: The name of your Web App Service (e.g., `ag-aione-dev-webapp`).
5. Click **Run workflow** to start deployment.

The workflow will use the appropriate parameter file (e.g., `dev.bicepparam`) based on the selected environment, and deploy the web app infrastructure and application as defined in `webapp.bicep`.

---
