@description('Location for the main resources (App Service, SQL).')
param location string = 'uksouth'

@description('Principal Object ID of the Entra ID admin for SQL Server.')
param adminObjectId string

@description('User Principal Name of the Entra ID admin (e.g. user@tenant.onmicrosoft.com).')
param adminLogin string

@description('User-assigned managed identity name. Format: mid-AppModAssist-DD-MM-HH')
param managedIdentityName string

@description('Set to true to deploy GenAI resources (Azure OpenAI + AI Search).')
param deployGenAI bool = false

// ---------------------------------------------------------
// App Service + Managed Identity module
// ---------------------------------------------------------
module appService 'app-service.bicep' = {
  name: 'appServiceDeploy'
  params: {
    location: location
    managedIdentityName: managedIdentityName
  }
}

// ---------------------------------------------------------
// Azure SQL module
// ---------------------------------------------------------
module azureSql 'azure-sql.bicep' = {
  name: 'azureSqlDeploy'
  params: {
    location: location
    adminObjectId: adminObjectId
    adminLogin: adminLogin
    managedIdentityPrincipalId: appService.outputs.managedIdentityClientId
    managedIdentityClientId: appService.outputs.managedIdentityClientId
    managedIdentityName: appService.outputs.managedIdentityName
  }
}

// ---------------------------------------------------------
// GenAI module (conditional)
// ---------------------------------------------------------
module genAI 'genai.bicep' = if (deployGenAI) {
  name: 'genAIDeploy'
  params: {
    managedIdentityPrincipalId: appService.outputs.managedIdentityClientId
    managedIdentityId: appService.outputs.managedIdentityId
  }
}

// ---------------------------------------------------------
// Outputs
// ---------------------------------------------------------
output appServiceName string = appService.outputs.appServiceName
output appServiceUrl string = appService.outputs.appServiceUrl
output managedIdentityClientId string = appService.outputs.managedIdentityClientId
output managedIdentityName string = appService.outputs.managedIdentityName
output sqlServerFqdn string = azureSql.outputs.sqlServerFqdn
output sqlServerName string = azureSql.outputs.sqlServerName
output databaseName string = azureSql.outputs.databaseName
output openAIEndpoint string = deployGenAI ? genAI.outputs.openAIEndpoint ?? '' : ''
output openAIModelName string = deployGenAI ? genAI.outputs.openAIModelName ?? '' : ''
output searchEndpoint string = deployGenAI ? genAI.outputs.searchEndpoint ?? '' : ''
