@description('Location for resources.')
param location string = 'uksouth'

@description('Principal Object ID of the Entra ID admin for SQL Server.')
param adminObjectId string

@description('User Principal Name of the Entra ID admin for SQL Server.')
param adminLogin string

@description('Principal ID of the managed identity to grant DB Manager role.')
param managedIdentityPrincipalId string

@description('Client ID of the managed identity (used in connection strings).')
param managedIdentityClientId string

@description('Name of the managed identity (used as SQL user).')
param managedIdentityName string

// ---------------------------------------------------------
// Derived names
// ---------------------------------------------------------
var uniqueSuffix = uniqueString(resourceGroup().id)
var sqlServerName = 'sql-expensemgmt-${uniqueSuffix}'
var databaseName = 'ExpenseMgmt'

// ---------------------------------------------------------
// SQL Server (Azure AD-only auth, MCAPS compliant)
// ---------------------------------------------------------
resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  properties: {
    administrators: {
      administratorType: 'ActiveDirectory'
      login: adminLogin
      sid: adminObjectId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    }
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// ---------------------------------------------------------
// Allow Azure services to access SQL Server
// ---------------------------------------------------------
resource allowAzureServicesFirewallRule 'Microsoft.Sql/servers/firewallRules@2021-11-01' = {
  parent: sqlServer
  name: 'AllowAllAzureIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ---------------------------------------------------------
// Database
// ---------------------------------------------------------
resource database 'Microsoft.Sql/servers/databases@2021-11-01' = {
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

// ---------------------------------------------------------
// Outputs
// ---------------------------------------------------------
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlServerName string = sqlServer.name
output databaseName string = database.name
