@description('Azure region for GenAI resources (OpenAI in swedencentral, Search in uksouth)')
param location string = 'uksouth'

@description('Principal ID of the managed identity')
param managedIdentityPrincipalId string

@description('Prefix for resource names')
param resourcePrefix string = 'northwind'

var uniqueSuffix = uniqueString(resourceGroup().id)
var openAIAccountName = '${resourcePrefix}-oai-${uniqueSuffix}'
var searchServiceName = '${resourcePrefix}-srch-${uniqueSuffix}'
var openAILocation = 'swedencentral'

// Azure OpenAI account in swedencentral
resource openAIAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openAIAccountName
  location: openAILocation
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: openAIAccountName
    publicNetworkAccess: 'Enabled'
  }
}

// GPT-4o model deployment
resource gpt4oDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: openAIAccount
  name: 'gpt-4o'
  sku: {
    name: 'Standard'
    capacity: 8
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-08-06'
    }
  }
}

// Azure AI Search in uksouth
resource searchService 'Microsoft.Search/searchServices@2022-09-01' = {
  name: searchServiceName
  location: location
  sku: {
    name: 'standard'
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
  }
}

// Role: Cognitive Services OpenAI User on OpenAI account
var cognitiveServicesOpenAIUserRoleId = '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
resource openAIManagedIdentityRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(openAIAccount.id, managedIdentityPrincipalId, cognitiveServicesOpenAIUserRoleId)
  scope: openAIAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', cognitiveServicesOpenAIUserRoleId)
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Role: Search Index Data Contributor on AI Search
var searchIndexDataContributorRoleId = '8ebe5a00-799e-43f5-93ac-243d3dce84a7'
resource searchManagedIdentityRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(searchService.id, managedIdentityPrincipalId, searchIndexDataContributorRoleId)
  scope: searchService
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', searchIndexDataContributorRoleId)
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output openAIEndpoint string = openAIAccount.properties.endpoint
output openAIModelName string = 'gpt-4o'
output openAIName string = openAIAccount.name
output searchEndpoint string = 'https://${searchService.name}.search.windows.net'
