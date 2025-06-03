@description('Name of the Azure OpenAI resource.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('SKU for Azure OpenAI resource')
param skuName string = 'S0'

@description('Optional. Array of deployments about cognitive service accounts to create.')
param deployments array = []

resource cognitiveService 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: name
  location: location
  tags: tags
  kind: 'OpenAI'
  properties: {
    customSubDomainName: name
    publicNetworkAccess: 'Enabled'
  }
  sku: {
    name: skuName
  }
}

@batchSize(1)
resource cognitiveService_deployments 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = [
  for (deployment, index) in (deployments ?? []): {
    parent: cognitiveService
    name: deployment.?name ?? '${name}-deployments'
    properties: {
      model: deployment.model
      // raiPolicyName: deployment.?raiPolicyName
      // versionUpgradeOption: deployment.?versionUpgradeOption
    }
    sku: deployment.sku
  }
]

output name string = cognitiveService.name
output endpoint string = cognitiveService.properties.endpoint
output resourceId string = cognitiveService.id
