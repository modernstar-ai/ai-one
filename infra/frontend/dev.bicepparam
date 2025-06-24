using './webapp.bicep'

param environmentName = 'dev'

param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ag-aione-v2-dev-app'
param apiAppName = 'ag-aione-v2-dev-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-v2-dev-la'
