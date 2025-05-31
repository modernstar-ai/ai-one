@description('Name of the Storage Account.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('The name of the workload\'s existing Log Analytics workspace.')
param logWorkspaceName string

@description('SKU for Storage Account')
param skuName object

@description('Array of blob container names to be created')
param blobContainerCollection array = []

resource logWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: logWorkspaceName
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: name
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: skuName
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource storageDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${storageAccount.name}-diagnostic'
  scope: storageAccount
  properties: {
    workspaceId: logWorkspace.id
    logs: [
      //   {
      //     category: 'StorageRead'
      //     enabled: true
      //     retentionPolicy: {
      //       enabled: false
      //       days: 0
      //     }
      //   }
      //   {
      //     category: 'StorageWrite'
      //     enabled: true
      //     retentionPolicy: {
      //       enabled: false
      //       days: 0
      //     }
      //   }
      //   {
      //     category: 'StorageDelete'
      //     enabled: true
      //     retentionPolicy: {
      //       enabled: false
      //       days: 0
      //     }
      //   }
      // ]
    ]
  }
}

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [
  for container in blobContainerCollection: {
    parent: blobServices
    name: container.name
    properties: {
      publicAccess: container.publicAccess
    }
  }
]

output name string = storageAccount.name
output resourceId string = storageAccount.id
