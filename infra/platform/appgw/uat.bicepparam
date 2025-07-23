using './main.bicep'

param environmentName = 'uat'
var sharedVariables = loadJsonContent('../../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../../tags.json')

param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-private-dev/providers/microsoft.operationalinsights/workspaces/ag-aionev12-uat-la'

param subnetAddressPrefix = '10.3.11.0/24'
param keyVaultName = 'ag-aionev12-uat-kv'
param sslCertificateSecretName = 'agile-chat-ssl-cert'
param enableHttps = true
