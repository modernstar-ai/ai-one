# AI-One Deployment Using GitHub Actions

Deployment is managed using GitHub Actions, defined in the `.github/workflows` directory of the project repository.

The deployment process includes the following steps:

- **Deploy AI-One Platform Infrastructure**: Provisions the core Azure resources and shared services required for the solution.
- **Deploy AI-One API**: Provisions the backend resources and deploys the Agile.Chat API application to Azure.
- **Deploy AI-One Web App**: Provisions the frontend resources and deploys the Agile.Chat web application to Azure.

---

## 1. Set Up Source Code Repository

Fork or copy [agile-chat](https://github.com/agile-internal/agile-chat) repository to a new project repository in Customer's GitHub organization.

---

## 2. Create Environments

The deployment is designed to work with multiple environments (e.g., dev, tst, uat, and prod).
Create the required environments in the new project repository on GitHub.

The environments will be used to manage environment-specific configurations and secrets.

***To create environments in GitHub:***

1. Navigate to the new project repository on GitHub.
2. Click on the **Settings** tab.
3. In the left sidebar, select **Environments** under the **Code and automation** section.
4. Click the **New environment** button and enter a name (e.g., `dev`, `tst`, `uat`, `prod`).
5. Repeat step 4 for each environment required.

---

## 3. Create Environment Variables

Create the following environment variables in each environment created in the previous step.

***To add environment variables to an environment:***

1. In your repository, go to **Settings** > **Environments** and select the environment to configure.
2. Under the **Environment secrets and variables** section, click **Add variable**.
3. Add each of the following variables (repeat for each environment as needed):

    - `AZURE_SUBSCRIPTION_ID`: The unique identifier of the Azure subscription.
    - `AZURE_TENANT_ID`: The Azure Active Directory (AAD) tenant ID associated with the organization.
    - `AZURE_CLIENT_ID`: The application (client) ID of the Azure Service Principal used for authentication and deployment automation.
    - `AZURE_RESOURCE_GROUP`: The name of the Azure resource group.
    - `PROJECT_NAME`: The name of the project (e.g., `ag-aione`).
    - `AZURE_LOCATION`: The location (region) where resources will be deployed (e.g., `australiaeast`).
    - `VITE_AGILECHAT_API_URL`: The base URL for the Agile.Chat API. 
      
        e.g., `https://ag-aione-apiapp.azurewebsites.net`.

4. Repeat step 3 for each environment to support.

---

## 4. Configure Access to GitHub Actions

To enable GitHub Actions to deploy resources to Azure, you must set up **Federated Credentials** between Azure Active Directory (Entra ID) and the GitHub repository. This allows GitHub Actions workflows to authenticate securely to Azure without storing secrets.

***To set up Federated Credentials:***

1. In the Azure Portal, go to **Azure Active Directory > App registrations** and select the application registration created earlier (e.g., `My Company AI Platform - Dev`).
2. In the left menu, select **Certificates & secrets > Federated credentials**.
3. Click **+ Add credential**.
4. Fill in the form:
    - **Name**: e.g., `GitHubActions-<env>`
    - **Issuer**: `https://token.actions.githubusercontent.com`
    - **Organization**: Your GitHub organization name (e.g., `myorg`)
    - **Repository**: The repository name (e.g., `myrepo`)
    - **Entity type**: The entity type, which should be `Environment`
    - **Environment**: The environment name (e.g., `dev`, `tst`, `uat`, `prod`)
    - **Subject identifier**: `repo:<org>/<repo>:environment:<env>` (e.g., `repo:myorg/myrepo:environment:dev`)
5. Click **Add** to save the federated credential.
6. Repeat steps 3-5 for each environment you created in the previous step (e.g., `dev`, `tst`, `uat`, `prod`).

---

## 5. Platform Deployment

```plaintext
infra
├── platform
    ├── platform.bicep
    ├── [env].bicepparam

```

- `platform.bicep`: The bicep file that defines the platform infrastructure resources.
- `[env].bicepparam`: Environment-specific parameter file for the platform deployment.

### 5.1 Configure Parameters

For every environment required, define in GitHub (such as `dev`, `tst`, `uat`, or `prod`), create a corresponding parameter file (e.g., `dev.bicepparam`, `tst.bicepparam`) in the `infra/platform` directory.

The parameter file name must exactly match the environment name. These files provide environment-specific values that the GitHub Action uses during deployment.

| Parameter Name      | Description                                                                                                 | Default Value      |
|---------------------|-------------------------------------------------------------------------------------------------------------|--------------------|
| `environmentName`   | The name of the deployment environment (should match the environment, e.g., `dev`, `tst`).                 | (none)             |
| `projectName`       | The project name.                                                                                           | `ag-aione`         |
| `location`          | Azure region for resource deployment.                                                                       | `australiaeast`    |
| `azureClientId`     | Azure Service Principal Client ID for authentication.                                                       | (none)             |
| `azureTenantId`     | Azure Active Directory Tenant ID.                                                                           | (none)             |
| `openAILocation`    | Azure region for OpenAI resources.                                                                         | `australiaeast`    |
| `deployAIFoundryResources`| Set to `true` to deploy AI Foundry resources, or `false` to deploy resources as standalone services.    | `true`             |
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
param deployAIFoundryResources = true
param deployOpenAiModels = true
param semanticSearchSku = 'standard'
param networkIsolation = false
```

> **Tip:**  
> Keep sensitive values secure and never commit secrets to source control.  
> Use GitHub environment secrets for all sensitive data such as client IDs, tenant IDs, and API keys.

---

### 5.2 Run Platform Deployment GitHub Action

The platform infrastructure is deployed using the `.github/workflows/deploy-platform.yml` GitHub Action.

***To deploy the platform infrastructure:***

1. Navigate to the **Actions** tab in your GitHub repository.
2. Find and select the **Deploy AI-One Platform Infrastructure** workflow.
3. Click **Run workflow**.
4. Fill in the required inputs:
    - **Project Name**: The name of your project (e.g., `ag-aione`).
    - **Environment**: The target environment (e.g., `dev`, `tst`, `uat`, or `prod`).
    - **Location**: The Azure region for deployment (e.g., `australiaeast`).
5. Click **Run workflow** to start deployment.

The workflow will use the appropriate parameter file (e.g., `dev.bicepparam`) based on the selected environment, and deploy the platform infrastructure resources defined in `platform.bicep`.

---

## 6 API Deployment

```plaintext
infra
├── backend
    ├── apiapp.bicep
    ├── [env].bicepparam
```

- `apiapp.bicep`: The Bicep file that defines the API infrastructure resources.
- `[env].bicepparam`: Environment-specific parameter file for the API deployment.

The API infrastructure is deployed using the `.github/workflows/deploy-backend.yml` GitHub Action. This action provisions the backend Azure resources and deploys the Agile.Chat API application.

### 6.1 Configure API Parameters

For each environment (such as `dev`, `tst`, `uat`, or `prod`), create a corresponding parameter file (e.g., `dev.bicepparam`, `tst.bicepparam`) in the `infra/backend` directory.

The parameter file name must exactly match the environment name. These files provide environment-specific values for the API deployment.

| Parameter Name                | Description                                                                                 | Default Value                |
|-------------------------------|---------------------------------------------------------------------------------------------|------------------------------|
| `environmentName`             | The name of the deployment environment (should match the environment, e.g., `dev`, `tst`). | (none)                       |
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
| `documentIntelligenceServiceName`          | Name of the Form Recognizer resource.                                                       | (none)           |
| `documentIntelligenceEndpoint`| Endpoint URL for the Document Intelligence Service.                                          | (none)           |
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
param documentIntelligenceServiceName = 'ag-aione-dev-docintel'
param documentIntelligenceEndpoint = 'https://ag-aione-dev-foundry.cognitiveservices.azure.com/'
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

### 6.2 Run API Deployment GitHub Action

To deploy the API infrastructure and application:

1. Navigate to the **Actions** tab in your GitHub repository.
2. Find and select the **Deploy AI-One Api** workflow.
3. Click **Run workflow**.
4. Fill in the required inputs:
    - **Environment Name**: The target environment (e.g., `dev`, `tst`, `uat`, or `prod`).
    - **Api App Name**: The name of your API App Service (e.g., `ag-aione-dev-apiapp`).
5. Click **Run workflow** to start deployment.

The workflow will use the appropriate parameter file (e.g., `dev.bicepparam`) based on the selected environment, and deploy the API infrastructure and application as defined in `apiapp.bicep`.

---

## 7. Web App Deployment

```plaintext
infra
├── frontend
    ├── webapp.bicep
    ├── [env].bicepparam
```

- `webapp.bicep`: The Bicep file that defines the web app infrastructure resources.
- `[env].bicepparam`: Environment-specific parameter file for the web app deployment.

### 7.1 Configure Web App Parameters

For each environment (such as `dev`, `tst`, `uat`, or `prod`), create a corresponding parameter file (e.g., `dev.bicepparam`, `tst.bicepparam`) in the `infra/frontend` directory. The parameter file name must exactly match the environment name. These files provide environment-specific values for the web app deployment.

| Parameter Name            | Description                                                      | Default Value                |
|--------------------------|------------------------------------------------------------------|------------------------------|
| `environmentName`         | The name of the deployment environment (should match the environment, e.g., `dev`, `tst`). | (none)                       |
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

### 7.2 Run Web App Deployment GitHub Action

To deploy the web app infrastructure and application:

1. Navigate to the **Actions** tab in your GitHub repository.
2. Find and select the **Deploy AI-One Web App** workflow.
3. Click **Run workflow**.
4. Fill in the required inputs:
    - **Environment Name**: The target environment (e.g., `dev`, `tst`, `uat`, or `prod`).
    - **Web App Name**: The name of your Web App Service (e.g., `ag-aione-dev-webapp`).
5. Click **Run workflow** to start deployment.

The workflow will use the appropriate parameter file (e.g., `dev.bicepparam`) based on the selected environment, and deploy the web app infrastructure and application as defined in `webapp.bicep`.

---
