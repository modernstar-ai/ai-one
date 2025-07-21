using './main.bicep'

param environmentName = 'tst'
var sharedVariables = loadJsonContent('../../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../../tags.json')
