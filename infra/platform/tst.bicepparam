using './platform.bicep'

param environmentName = 'tst'
param projectName = readEnvironmentVariable('PROJECT_NAME','ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param azureClientId = readEnvironmentVariable('AZURE_CLIENT_ID')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param openAILocation = 'australiaeast' 
param deployAIFoundryResources = true
param deployOpenAiModels = true

param semanticSearchSku = 'standard'
param networkIsolation = false
