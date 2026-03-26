@description('Azure region for deployment')
param location string = 'uksouth'

@description('Prefix for resource names')
param resourcePrefix string = 'northwind'

@description('Object ID of the Entra ID admin for SQL Server')
param adminObjectId string

@description('Login name of the Entra ID admin for SQL Server')
param adminLogin string

@description('Whether to deploy GenAI resources (Azure OpenAI + AI Search)')
param deployGenAI bool = false

// App Service + Managed Identity module
module appService 'app-service.bicep' = {
  name: 'appServiceDeployment'
  params: {
    location: location
    resourcePrefix: resourcePrefix
  }
}

// Azure SQL module
module azureSql 'azure-sql.bicep' = {
  name: 'azureSqlDeployment'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    adminObjectId: adminObjectId
    adminLogin: adminLogin
    managedIdentityPrincipalId: appService.outputs.managedIdentityPrincipalId
  }
}

// GenAI module (conditional)
module genai 'genai.bicep' = if (deployGenAI) {
  name: 'genaiDeployment'
  params: {
    location: location
    resourcePrefix: resourcePrefix
    managedIdentityPrincipalId: appService.outputs.managedIdentityPrincipalId
  }
}

output appServiceName string = appService.outputs.appServiceName
output appServiceHostname string = appService.outputs.appServiceHostname
output managedIdentityClientId string = appService.outputs.managedIdentityClientId
output managedIdentityPrincipalId string = appService.outputs.managedIdentityPrincipalId
output managedIdentityName string = appService.outputs.managedIdentityName
output sqlServerFqdn string = azureSql.outputs.sqlServerFqdn
output sqlServerName string = azureSql.outputs.sqlServerName
output openAIEndpoint string = deployGenAI ? genai.outputs.openAIEndpoint : ''
output openAIModelName string = deployGenAI ? genai.outputs.openAIModelName : ''
output openAIName string = deployGenAI ? genai.outputs.openAIName : ''
output searchEndpoint string = deployGenAI ? genai.outputs.searchEndpoint : ''
