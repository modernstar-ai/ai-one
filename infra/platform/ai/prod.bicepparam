using './main.bicep'

param environmentName = 'prod'
var sharedVariables = loadJsonContent('../../shared-parameters.json')

param tags = loadJsonContent('../../tags.json')
param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param openAILocation = readEnvironmentVariable('AZURE_OPENAI_LOCATION', sharedVariables.openAILocation)

param logAnalyticsWorkspaceResourceId = '/subscriptions/a8d44b38-6c3d-459c-b6c5-8bc173941b44/resourcegroups/ms-rg-aione_prd1-ae/providers/microsoft.operationalinsights/workspaces/ms-aione-prod-la'

// OpenAI and AI Foundry configuration
param deployAIFoundryResources = true
param deployOpenAiModels = true

// Network isolation disabled for production environment
param networkIsolation = false
