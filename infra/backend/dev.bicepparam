using './apiapp.bicep'

param environmentName = 'dev'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Development'
param appServicePlanName = 'ag-aione-dev-app'
param applicationInsightsName = 'ag-aione-dev-apiapp'
param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-dev/providers/microsoft.operationalinsights/workspaces/ag-aione-dev-la'

param keyVaultName = 'ag-aione-dev-kv'
param storageAccountName = 'agaionedevsto'
param documentIntelligenceServiceName = 'ag-aione-dev-docintel'
param documentIntelligenceEndpoint = 'https://ag-aione-dev-docintel.cognitiveservices.azure.com/'
param openAiName = 'ag-aione-dev-foundry'
param openAiEndpoint = 'https://ag-aione-dev-foundry.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aione-dev-search'
param serviceBusName = 'ag-aione-dev-service-bus'
param cosmosDbAccountName = 'ag-aione-dev-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aione-dev-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aione-dev-blob-eg'

param allowedOrigins = ['https://ag-aione-dev-webapp.azurewebsites.net']
