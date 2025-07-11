using './apiapp.bicep'

param environmentName = 'uat'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'UAT'
param appServicePlanName = 'ag-aionev10-uat-app'
param applicationInsightsName = 'ag-aionev10-uat-apiapp'
param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-private-dev/providers/microsoft.operationalinsights/workspaces/ag-aionev10-uat-la'

param keyVaultName = 'ag-aionev10-uat-kv'
param storageAccountName = 'agaionev10uatsto'
param maxServiceBusQueueMessageSizeInKilobytes = 1024
param documentIntelligenceServiceName = 'ag-aionev10-uat-docintel'
param documentIntelligenceEndpoint = 'https://ag-aionev10-uat-docintel.cognitiveservices.azure.com/'
param openAiName = 'ag-aionev10-uat-foundry'
param openAiEndpoint = 'https://ag-aionev10-uat-foundry.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aionev10-uat-search'
param serviceBusName = 'ag-aionev10-uat-service-bus'
param cosmosDbAccountName = 'ag-aionev10-uat-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aionev10-uat-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aionev10-uat-blob-eg'

param allowedOrigins = ['https://ag-aionev10-uat-webapp.azurewebsites.net']

param networkIsolation = true
param virtualNetworkName = 'ag-aionev10-uat-vnet'
