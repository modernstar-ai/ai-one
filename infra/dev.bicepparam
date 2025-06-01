using './main.bicep'

param projectName = readEnvironmentVariable('PROJECT_NAME')
param azureClientId = readEnvironmentVariable('AZURE_CLIENT_ID')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param environmentName = 'dev'
param location = 'australiaeast'
param openAILocation = 'australiaeast'
param aspCoreEnvironment = 'Development'
param semanticSearchSku = 'standard'


