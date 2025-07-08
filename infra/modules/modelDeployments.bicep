@description('AI Services resource name')
param aiFoundryServicesName string

import { deploymentsType } from './customTypes.bicep'
@description('Optional. Specifies the OpenAI deployments to create.')
param aiModelDeployments deploymentsType[] = []

resource foundryAccount 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' existing = {
  name: aiFoundryServicesName
}

@batchSize(1)
resource cognitiveService_deployments 'Microsoft.CognitiveServices/accounts/deployments@2025-04-01-preview' = [
  for (deployment, index) in (aiModelDeployments ?? []): {
    parent: foundryAccount
    name: deployment.?name
    properties: {
      model: deployment.model
      raiPolicyName: deployment.?raiPolicyName
      versionUpgradeOption: deployment.?versionUpgradeOption
    }
    sku: deployment.?sku
  }
]
