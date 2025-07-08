using './webapp.bicep'

param environmentName = 'prod'

param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ag-aione-prod-app'
param apiAppName = 'ag-aione-prod-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-prod-la'
