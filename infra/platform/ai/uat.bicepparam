using './main.bicep'

param environmentName = 'uat'
var sharedVariables = loadJsonContent('../../shared-parameters.json')

param tags = loadJsonContent('../../tags.json')
param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param openAILocation = readEnvironmentVariable('AZURE_OPENAI_LOCATION', sharedVariables.openAILocation)

param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-private-dev/providers/microsoft.operationalinsights/workspaces/ag-aionev10-uat-la'

// OpenAI and AI Foundry configuration
param deployAIFoundryResources = true
param deployOpenAiModels = true

param networkIsolation = true
