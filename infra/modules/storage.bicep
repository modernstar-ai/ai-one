@description('Storage account name')
param storageAccountName string

@description('Azure region for resource deployment')
param location string

@description('Resource tags')
param tags object

@description('SKU for Storage Account')
param storageServiceSku object

@description('The container name where files will be stored for folder search')
param storageServiceFoldersContainerName string

@description('The container name for images')
param storageServiceImageContainerName string

var validStorageServiceImageContainerName = toLower(replace(storageServiceImageContainerName, '-', ''))

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
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

output storageAccountName string = storage.name
output storageAccountId string = storage.id
output blobServicesId string = blobServices.id
output imagesContainerId string = imagesContainer.id
output indexContainerId string = indexContainer.id
