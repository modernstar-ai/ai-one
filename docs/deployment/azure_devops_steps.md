# AI-One Deployment Using Azure DevOps

Deployment is managed using Azure DevOps Pipelines, defined in the `.azure-pipelines` directory of the project repository.

The deployment process includes the following steps:

- **Deploy AI-One Platform Infrastructure**: Provisions the core Azure resources and shared services required for the solution.
- **Deploy AI-One API**: Provisions the backend resources and deploys the Agile.Chat API application to Azure.
- **Deploy AI-One Web App**: Provisions the frontend resources and deploys the Agile.Chat web application to Azure.

---

## 1. Set Up Source Code Repository

Clone or import the [agile-chat](https://github.com/agile-internal/agile-chat) repository into your Azure DevOps project.

---

## 2. Create Azure DevOps Service Connections

To enable Azure DevOps Pipelines to deploy resources to Azure, create a Service Connection using a Service Principal with appropriate permissions.

Repeat these steps for each environment (e.g., `dev`, `tst`, `uat`, `prod`) as needed.

***To create a Service Connection:***

1. In Azure DevOps, go to **Project Settings > Service connections**.
2. Click **New service connection** > **Azure Resource Manager**.
3. Select **App registration (Manual)**.
4. For **Credential Type**, choose **Workload Identity Federation**.
5. Enter a **Service Connection Name** and **Description** (e.g., `dev-service-connection`).
6. Set **Environment** to **Azure Cloud**.
7. Enter your Azure **Tenant ID**.
8. Click **Next**.

***Configure Federated Credentials in Azure***

1. In the Azure portal, navigate to **Azure Active Directory > App registrations** and select your app registration.
2. In the left menu, select **Certificates & secrets > Federated credentials**.
3. Click **+ Add credential** and choose **Other issuer** as the scenario.
4. Copy the **Issuer URL** and **Subject identifier** from the Azure DevOps service connection setup page and paste them into the corresponding fields in Azure.
5. Provide a **Name** for the federated credential (e.g., `AzureDevOps-<env>`).
6. Click **Add** to save the federated credential.

***Complete Service Connection Setup***

1. Back in Azure DevOps, set the **Scope Level** to **Subscription**.
2. Enter your **Azure Subscription ID** and **Subscription Name**.
3. Enter the **Service Principal Client ID** (from your app registration).
4. (Optional) Select **Grant access permission to all pipelines** if you want all pipelines to use this connection.
5. Click **Save** to create the service connection.

---

## 3. Create Environments in Azure DevOps

The deployment is designed to work with multiple environments (e.g., dev, tst, uat, prod).
Create the required environments in your Azure DevOps project.

***To create environments in Azure DevOps:***

1. Go to **Pipelines > Environments**.
2. Click **New environment** and enter a name (e.g., `dev`, `tst`, `uat`, `prod`).
3. Repeat for each environment required.

Environments are used to manage environment-specific configurations, secrets, and approvals.

---

## 4. Configure Pipeline Variables

Create the following environment variables in each environment created in the previous step.

***To add environment variables:***

1. Go to **Pipelines > Library** in Azure DevOps.
2. Click **+ Variable group**.
3. Name the group exactly as your environment (e.g., `dev`, `tst`, `uat`, `prod`).
4. Add each of the following variables:

    - `AZURE_SUBSCRIPTION_ID`: The unique identifier of the Azure subscription.
    - `AZURE_TENANT_ID`: The Azure Active Directory (AAD) tenant ID associated with the organization.
    - `AZURE_CLIENT_ID`: The application (client) ID of the Azure Service Principal used for authentication and deployment automation.
    - `AZURE_RESOURCE_GROUP`: The name of the Azure resource group.
    - `VITE_AGILECHAT_API_URL`: The base URL for the Agile.Chat API.

5. Repeat step 3 and 4 for each environment to support.

---

## 5. Create Azure Pipelines for Platform, API, and Web App

Create three pipelines in Azure DevOps for deploying the platform infrastructure, backend API, and frontend web app. Each pipeline should use the corresponding YAML file from the `.azure-pipelines` directory.

***To create each pipeline:***

1. In Azure DevOps, go to **Pipelines**.
2. Click **New pipeline** and select your repository.
3. Choose **Existing Azure Pipelines YAML file**.
4. For the platform infrastructure pipeline, select `.azure-pipelines/deploy-platform.yml`.
5. For the backend API pipeline, select `.azure-pipelines/deploy-backend.yml`.
6. For the frontend web app pipeline, select `.azure-pipelines/deploy-frontend.yml`.
7. Complete the setup for each pipeline and save.

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

For every environment required, create a corresponding parameter file (e.g., `dev.bicepparam`, `tst.bicepparam`) in the `infra/platform` directory.

The parameter file name must exactly match the environment name. These files provide environment-specific values that the pipeline uses during deployment.

| Parameter Name      | Description                                                                                                 | Default Value      |
|---------------------|-------------------------------------------------------------------------------------------------------------|--------------------|
| `environmentName`   | The name of the deployment environment (should match the environment, e.g., `dev`, `tst`).                 | (none)             |
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
> Use Azure DevOps variable groups or pipeline secrets for all sensitive data such as client IDs, tenant IDs, and API keys.

---

### 5.2 Run Platform Deployment Pipeline

The platform infrastructure is deployed using the `.azure-pipelines/deploy-platform.yml` pipeline.

***To deploy the platform infrastructure:***

1. In Azure DevOps, go to **Pipelines**.
2. Find and select the **Deploy Platform Infrastructure** pipeline.
3. Click **Run pipeline**.
4. Fill in the required inputs:
    - **Project Name**: The name of your project (e.g., `ag-aione`).
    - **Environment**: The target environment (e.g., `dev`, `tst`, `uat`, or `prod`).
    - **Location**: The Azure region for deployment (e.g., `australiaeast`).
5. Click **Run** to start deployment.

The pipeline will use the appropriate parameter file (e.g., `dev.bicepparam`) based on the selected environment, and deploy the platform infrastructure resources defined in `platform.bicep`.

---

## 6. API Deployment

```plaintext
infra
├── backend
    ├── apiapp.bicep
    ├── [env].bicepparam
```

- `apiapp.bicep`: The Bicep file that defines the API infrastructure resources.
- `[env].bicepparam`: Environment-specific parameter file for the API deployment.

The API infrastructure is deployed using the `.azure-pipelines/deploy-backend.yml` pipeline. This pipeline provisions the backend Azure resources and deploys the Agile.Chat API application.

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
param projectName = 'ag-aione'
param location = 'australiaeast'
param azureClientId = '<your-client-id>'
param azureTenantId = '<your-tenant-id>'
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
param adminEmailAddresses = ['admin@example.com']
```

---

### 6.2 Run API Deployment Pipeline

To deploy the API infrastructure and application:

1. In Azure DevOps, go to **Pipelines**.
2. Find and select the **Deploy Backend API** pipeline.
3. Click **Run pipeline**.
4. Fill in the required inputs:
    - **Environment**: The target environment (e.g., `dev`, `tst`, `uat`, or `prod`).
    - **Api App Name**: The name of your API App Service (e.g., `ag-aione-dev-apiapp`).
5. Click **Run** to start deployment.

The pipeline will use the appropriate parameter file (e.g., `dev.bicepparam`) based on the selected environment, and deploy the API infrastructure and application as defined in `apiapp.bicep`.

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
param projectName = 'ag-aione'
param location = 'australiaeast'
param tags = loadJsonContent('../tags.json')
param appServicePlanName = 'ag-aione-dev-app'
param apiAppName = 'ag-aione-dev-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-dev-la'
```

---

### 7.2 Run Web App Deployment Pipeline

To deploy the web app infrastructure and application:

1. In Azure DevOps, go to **Pipelines**.
2. Find and select the **Deploy Frontend Web App** pipeline.
3. Click **Run pipeline**.
4. Fill in the required inputs:
    - **Environment**: The target environment (e.g., `dev`, `tst`, `uat`, or `prod`).
    - **Web App Name**: The name of your Web App Service (e.g., `ag-aione-dev-webapp`).
5. Click **Run** to start deployment.

The pipeline will use the appropriate parameter file (e.g., `dev.bicepparam`) based on the selected environment, and deploy the web app infrastructure and application as defined in `webapp.bicep`.

---
