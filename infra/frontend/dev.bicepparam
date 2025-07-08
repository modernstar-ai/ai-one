using './webapp.bicep'

param environmentName = 'dev'

param projectName = readEnvironmentVariable('PROJECT_NAME', 'ms-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ms-aione-dev-app'
param apiAppName = 'ms-aione-dev-apiapp'
param logAnalyticsWorkspaceName = 'ms-aione-dev-la'
