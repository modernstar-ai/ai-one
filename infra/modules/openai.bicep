@description('Name of the Azure OpenAI resource.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('SKU for Azure OpenAI resource')
param skuName string = 'S0'

resource azureopenai 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
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

output name string = azureopenai.name
output endpoint string = azureopenai.properties.endpoint
output resourceId string = azureopenai.id
