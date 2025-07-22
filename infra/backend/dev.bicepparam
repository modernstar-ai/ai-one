using './apiapp.bicep'

param environmentName = 'dev'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Development'
param appServicePlanName = 'ms-aione-dev-app'
param applicationInsightsName = 'ms-aione-dev-apiapp'
param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-dev/providers/microsoft.operationalinsights/workspaces/ag-aione-dev-la'

param keyVaultName = 'ms-aione-dev-kv'
param storageAccountName = 'msaionedevsto'
param documentIntelligenceServiceName = 'ms-aione-dev-docintel'
param documentIntelligenceEndpoint = 'https://ms-aione-dev-docintel.cognitiveservices.azure.com/'
param openAiName = 'ms-aione-dev-foundry'
param openAiEndpoint = 'https://ms-aione-dev-foundry.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ms-aione-dev-search'
param serviceBusName = 'ms-aione-dev-service-bus'
param cosmosDbAccountName = 'ms-aione-dev-cosmos'
param cosmosDbAccountEndpoint = 'https://ms-aione-dev-cosmos.documents.azure.com:443/'
param eventGridName = 'ms-aione-dev-blob-eg'

param allowedOrigins = ['https://ms-aione-dev-webapp.azurewebsites.net']
