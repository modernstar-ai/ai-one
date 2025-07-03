using './apiapp.bicep'

param environmentName = 'uat'
param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'UAT'
param appServicePlanName = 'ag-aione-uat-app'
param applicationInsightsName = 'ag-aione-uat-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-uat-la'
param keyVaultName = 'ag-aione-uat-kv'
param storageName = 'agaioneuatsto'
param storageAccountName = 'agaioneuatsto'
param documentIntelligenceServiceName = 'ag-aione-uat-docintel'
param documentIntelligenceEndpoint = 'https://ag-aione-uat-docintel.cognitiveservices.azure.com/'
param openAiName = 'ag-aione-uat-aillm'
param openAiEndpoint = 'https://ag-aione-uat-aillm.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aione-uat-search'
param serviceBusName = 'ag-aione-uat-service-bus'
param cosmosDbAccountName = 'ag-aione-uat-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aione-uat-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aione-uat-blob-eg'

param allowedOrigins = ['https://ag-aione-uat-webapp.azurewebsites.net']

param adminEmailAddresses = ['adam-stephensen@agile-analytics.com.au']
