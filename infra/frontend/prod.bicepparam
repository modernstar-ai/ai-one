using './webapp.bicep'

param environmentName = 'prd'

param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ag-aione-prd-app'
param apiAppName = 'ag-aione-prd-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-prd-la'
