using './apiapp.bicep'

param environmentName = 'prod'
param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Production'
param appServicePlanName = 'ag-aione-prod-app'
param applicationInsightsName = 'ag-aione-prod-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-prod-la'
param keyVaultName = 'ag-aione-prod-kv'
param storageName = 'agaioneprdsto'
param storageAccountName = 'agaioneprdsto'
param documentIntelligenceServiceName = 'ag-aione-prod-docintel'
param documentIntelligenceEndpoint = 'https://ag-aione-prod-docintel.cognitiveservices.azure.com/'
param openAiName = 'ag-aione-prod-aillm'
param openAiEndpoint = 'https://ag-aione-prod-aillm.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aione-prod-search'
param serviceBusName = 'ag-aione-prod-service-bus'
param cosmosDbAccountName = 'ag-aione-prod-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aione-prod-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aione-prod-blob-eg'

param allowedOrigins = ['https://ag-aione-prod-webapp.azurewebsites.net']

param adminEmailAddresses = ['adam-stephensen@agile-analytics.com.au']
