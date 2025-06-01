using './main.bicep'

param projectName = readEnvironmentVariable('PROJECT_NAME','ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param azureClientId = readEnvironmentVariable('AZURE_CLIENT_ID')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param environmentName = 'dev'

param openAILocation = 'australiaeast'
param aspCoreEnvironment = 'Development'
param semanticSearchSku = 'standard'


