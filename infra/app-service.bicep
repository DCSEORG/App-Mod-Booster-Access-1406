@description('Location for all resources.')
param location string = 'uksouth'

@description('Name stamp for resource uniqueness based on resource group.')
var uniqueSuffix = uniqueString(resourceGroup().id)

@description('App Service Plan name (lowercase).')
var appServicePlanName = 'asp-expensemgmt-${uniqueSuffix}'

@description('App Service name (lowercase).')
var appServiceName = 'app-expensemgmt-${uniqueSuffix}'

@description('Managed Identity name (lowercase).')
param managedIdentityName string

// ---------------------------------------------------------
// User-Assigned Managed Identity
// ---------------------------------------------------------
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
}

// ---------------------------------------------------------
// App Service Plan (S1 SKU to avoid cold starts)
// ---------------------------------------------------------
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
  properties: {
    reserved: false
  }
}

// ---------------------------------------------------------
// App Service
// ---------------------------------------------------------
resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: appServiceName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: managedIdentity.properties.clientId
        }
        {
          name: 'ManagedIdentityClientId'
          value: managedIdentity.properties.clientId
        }
      ]
    }
  }
}

// ---------------------------------------------------------
// Outputs
// ---------------------------------------------------------
output appServiceName string = appService.name
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output managedIdentityId string = managedIdentity.id
output managedIdentityClientId string = managedIdentity.properties.clientId
output managedIdentityName string = managedIdentity.name
// principalId is accessed via managedIdentity.properties.principalId but not output directly
// to avoid issues with user-assigned identity principal ID availability at deploy time.
// It is passed via main.bicep after deployment using a separate lookup.
