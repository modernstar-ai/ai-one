using './main.bicep'

param environmentName = 'dev'
var sharedVariables = loadJsonContent('../../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../../tags.json')

param openAILocation = readEnvironmentVariable('AZURE_OPENAI_LOCATION', 'australiaeast')

param logAnalyticsWorkspaceResourceId = '/subscriptions/a8d44b38-6c3d-459c-b6c5-8bc173941b44/resourcegroups/ms-rg-aione_dev1-ae/providers/microsoft.operationalinsights/workspaces/ms-aione-dev-la'

// OpenAI and AI Foundry configuration
param deployAIFoundryResources = true
param deployOpenAiModels = true

// Network isolation disabled for standard dev environment
param networkIsolation = false
