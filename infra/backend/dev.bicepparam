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
param logAnalyticsWorkspaceResourceId = '/subscriptions/a8d44b38-6c3d-459c-b6c5-8bc173941b44/resourcegroups/ms-rg-aione_dev1-ae/providers/microsoft.operationalinsights/workspaces/ms-aione-dev-la'

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

param allowedOrigins = ['https://ms-aione-dev-webapp.azurewebsites.net','https://ai-dev.modernstar.com']
