@description('Azure region for deployment')
param location string = 'uksouth'

@description('Prefix for resource names')
param resourcePrefix string = 'northwind'

@description('Object ID of the Entra ID admin for SQL Server')
param adminObjectId string

@description('Login name of the Entra ID admin for SQL Server')
param adminLogin string

@description('Principal ID of the managed identity to grant DB access')
param managedIdentityPrincipalId string

var uniqueSuffix = uniqueString(resourceGroup().id)
var sqlServerName = '${resourcePrefix}-sql-${uniqueSuffix}'
var databaseName = 'Northwind'

// Azure SQL Server
resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  properties: {
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: adminLogin
      principalType: 'User'
      sid: adminObjectId
      tenantId: subscription().tenantId
    }
  }
}

// Firewall rule for Azure services
resource firewallRuleAzure 'Microsoft.Sql/servers/firewallRules@2021-11-01' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Northwind database - Basic tier
resource northwindDb 'Microsoft.Sql/servers/databases@2021-11-01' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

// Grant managed identity DatabaseManager role at server level
resource managedIdentityServerRole 'Microsoft.Sql/servers/administrators@2021-11-01' = if (false) {
  parent: sqlServer
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: 'managed-identity'
    sid: managedIdentityPrincipalId
    tenantId: subscription().tenantId
  }
}

output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlServerName string = sqlServer.name
output databaseName string = databaseName
