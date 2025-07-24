using './apiapp.bicep'

param environmentName = 'uat'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'UAT'
param appServicePlanName = 'ag-aionev14-uat-app'
param applicationInsightsName = 'ag-aionev14-uat-apiapp'
param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-private-dev/providers/microsoft.operationalinsights/workspaces/ag-aionev14-uat-la'

param keyVaultName = 'ag-aionev14-uat-kv'
param storageAccountName = 'agaionev14uatsto'
param maxServiceBusQueueMessageSizeInKilobytes = 1024
param documentIntelligenceServiceName = 'ag-aionev14-uat-docintel'
param documentIntelligenceEndpoint = 'https://ag-aionev14-uat-docintel.cognitiveservices.azure.com/'
param openAiName = 'ag-aionev14-uat-foundry'
param openAiEndpoint = 'https://ag-aionev14-uat-foundry.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aionev14-uat-search'
param serviceBusName = 'ag-aionev14-uat-service-bus'
param cosmosDbAccountName = 'ag-aionev14-uat-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aionev14-uat-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aionev14-uat-blob-eg'

param allowedOrigins = ['https://ag-aionev14-uat-webapp.azurewebsites.net']

param networkIsolation = true
param virtualNetworkName = 'ag-aionev14-uat-vnet'
param privateEndpointsSubnetName = 'PrivateEndpointsSubnet'
param allowPrivateAccessOnly = true

// Optional: Enable IP restrictions to allow access only from Application Gateway
// Uncomment and update the IP address after deploying the Application Gateway
// param enableIpRestrictions = true
// param allowedIpAddresses = ['<APPLICATION_GATEWAY_PUBLIC_IP>/32']

param enableIpRestrictions = true
param allowedIpAddresses = ['68.218.105.22/32']
