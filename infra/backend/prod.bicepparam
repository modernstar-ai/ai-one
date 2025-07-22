using './apiapp.bicep'

param environmentName = 'prod'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Production'
param appServicePlanName = 'ms-aione-prod-app'
param applicationInsightsName = 'ms-aione-prod-apiapp'

param logAnalyticsWorkspaceResourceId = '/subscriptions/a8d44b38-6c3d-459c-b6c5-8bc173941b44/resourcegroups/ms-rg-aione_prd1-ae/providers/microsoft.operationalinsights/workspaces/ms-aione-prod-la'

param keyVaultName = 'ms-aione-prod-kv'
param storageAccountName = 'msaioneprdsto'
param documentIntelligenceServiceName = 'ms-aione-prod-docintel'
param documentIntelligenceEndpoint = 'https://ms-aione-prod-docintel.cognitiveservices.azure.com/'
param openAiName = 'ms-aione-prod-foundry'
param openAiEndpoint = 'https://ms-aione-prod-foundry.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ms-aione-prod-search'
param serviceBusName = 'ms-aione-prod-service-bus'
param cosmosDbAccountName = 'ms-aione-prod-cosmos'
param cosmosDbAccountEndpoint = 'https://ms-aione-prod-cosmos.documents.azure.com:443/'
param eventGridName = 'ms-aione-prod-blob-eg'

param allowedOrigins = ['https://ms-aione-prod-webapp.azurewebsites.net']
