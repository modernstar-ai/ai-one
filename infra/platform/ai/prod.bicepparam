using './main.bicep'

param environmentName = 'prod'
var sharedVariables = loadJsonContent('../../shared-parameters.json')

param tags = loadJsonContent('../../tags.json')
param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param openAILocation = readEnvironmentVariable('AZURE_OPENAI_LOCATION', sharedVariables.openAILocation)

param logAnalyticsWorkspaceResourceId = '<REPLACE_WITH_YOUR_LOG_ANALYTICS_WORKSPACE_ID>'

// OpenAI and AI Foundry configuration
param deployAIFoundryResources = true
param deployOpenAiModels = true

// Network isolation disabled for production environment
param networkIsolation = false
