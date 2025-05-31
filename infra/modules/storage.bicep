@description('Name of the Storage Account.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('Resource ID of the Log Analytics workspace to use for diagnostic settings.')
param logAnalyticsWorkspaceResourceId string

@description('SKU for Storage Account')
param storageServiceSku object

@description('The container name where files will be stored for folder search')
param storageServiceFoldersContainerName string

@description('The container name for images')
param storageServiceImageContainerName string

var validStorageServiceImageContainerName = toLower(replace(storageServiceImageContainerName, '-', ''))

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: name
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: storageServiceSku
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storage
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

resource imagesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobServices
  name: validStorageServiceImageContainerName
}

resource indexContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobServices
  name: storageServiceFoldersContainerName
  properties: {
    publicAccess: 'None'
  }
}

resource storageDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (!empty(logAnalyticsWorkspaceResourceId)) {
  name: '${storage.name}-diagnostic'
  scope: storage
  properties: {
    workspaceId: logAnalyticsWorkspaceResourceId
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
    // metrics: [
    //   {
    //     category: 'Transaction'
    //     enabled: true
    //     retentionPolicy: {
    //       enabled: false
    //       days: 0
    //     }
    //   }
    //   {
    //     category: 'Capacity'
    //     enabled: true
    //     retentionPolicy: {
    //       enabled: false
    //       days: 0
    //     }
    //   }
    ]
  }
}

output storageAccountName string = storage.name
output storageAccountId string = storage.id
