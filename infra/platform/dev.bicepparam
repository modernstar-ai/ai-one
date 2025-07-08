using './platform.bicep'

param environmentName = 'dev'
param projectName = readEnvironmentVariable('PROJECT_NAME', 'ms-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param azureClientId = readEnvironmentVariable('AZURE_CLIENT_ID')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param openAILocation = 'australiaeast'
param openAiName = 'ms-aione-dev-aillm'
param deployAIFoundryResources = true
param deployOpenAiModels = true

param semanticSearchSku = 'standard'
param networkIsolation = false
