using './apiapp.bicep'

param environmentName = 'prd'
param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Production'
param appServicePlanName = 'ag-aione-prd-app'
param applicationInsightsName = 'ag-aione-prd-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-prd-la'
param keyVaultName = 'ag-aione-prd-kv'
param storageName = 'agaioneprdsto'
param storageAccountName = 'agaioneprdsto'
param formRecognizerName = 'ag-aione-prd-form'
param openAiName = 'ag-aione-prd-aillm'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aione-prd-search'
param serviceBusName = 'ag-aione-prd-service-bus'
param cosmosDbAccountName = 'ag-aione-prd-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aione-prd-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aione-prd-blob-eg'

param allowedOrigins = ['https://ag-aione-prd-webapp.azurewebsites.net']

param adminEmailAddresses = ['adam-stephensen@agile-analytics.com.au']
