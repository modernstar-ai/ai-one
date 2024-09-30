targetScope = 'resourceGroup'

@minLength(1)
@maxLength(9)
@description('The name of the solution.')
param projectName string 

@minLength(1)
@maxLength(4)
@description('The type of environment. e.g. local, dev, uat, prod.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

//other
var tags = { 'azd-env-name': environmentName }


module resources 'resources.bicep' = {
  name: 'all-resources'
  params: {
    projectName: projectName
    environmentName:environmentName
    tags: tags  
    location: location
  }
}

output APP_URL string = resources.outputs.url
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
