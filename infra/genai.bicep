@description('Azure OpenAI must be deployed to swedencentral for quota availability.')
param location string = 'swedencentral'

@description('Principal ID of the managed identity to grant Cognitive Services roles.')
param managedIdentityPrincipalId string

@description('Principal ID for AI Search role assignment (same managed identity).')
param managedIdentityId string

// ---------------------------------------------------------
// Derived names (lowercase to avoid Azure OpenAI customSubDomainName issues)
// ---------------------------------------------------------
var uniqueSuffix = toLower(uniqueString(resourceGroup().id))
var openAIName = 'aoai-expensemgmt-${uniqueSuffix}'
var searchName = 'srch-expensemgmt-${uniqueSuffix}'

// ---------------------------------------------------------
// Azure OpenAI (swedencentral, GPT-4o, capacity 8)
// ---------------------------------------------------------
resource openAI 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openAIName
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'OpenAI'
  properties: {
    customSubDomainName: openAIName
    publicNetworkAccess: 'Enabled'
  }
}

resource gpt4oDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: openAI
  name: 'gpt-4o'
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-08-06'
    }
    raiPolicyName: 'Microsoft.DefaultV2'
  }
  sku: {
    name: 'Standard'
    capacity: 8
  }
}

// ---------------------------------------------------------
// AI Search (S0 SKU)
// ---------------------------------------------------------
resource aiSearch 'Microsoft.Search/searchServices@2023-11-01' = {
  name: searchName
  location: 'uksouth'
  sku: {
    name: 'basic'
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    publicNetworkAccess: 'enabled'
  }
}

// ---------------------------------------------------------
// Role: Cognitive Services OpenAI User -> managed identity
// ---------------------------------------------------------
var cognitiveServicesOpenAIUserRoleId = '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
resource openAIRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(openAI.id, managedIdentityPrincipalId, cognitiveServicesOpenAIUserRoleId)
  scope: openAI
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', cognitiveServicesOpenAIUserRoleId)
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ---------------------------------------------------------
// Role: Search Index Data Contributor -> managed identity
// ---------------------------------------------------------
var searchIndexDataContributorRoleId = '8ebe5a00-799e-43f5-93ac-243d3dce84a7'
resource searchRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aiSearch.id, managedIdentityPrincipalId, searchIndexDataContributorRoleId)
  scope: aiSearch
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', searchIndexDataContributorRoleId)
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ---------------------------------------------------------
// Role: Search Service Contributor -> managed identity
// ---------------------------------------------------------
var searchServiceContributorRoleId = '7ca78c08-252a-4471-8644-bb5ff32d4ba0'
resource searchServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aiSearch.id, managedIdentityPrincipalId, searchServiceContributorRoleId)
  scope: aiSearch
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', searchServiceContributorRoleId)
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ---------------------------------------------------------
// Outputs
// ---------------------------------------------------------
output openAIEndpoint string = openAI.properties.endpoint
output openAIModelName string = 'gpt-4o'
output openAIName string = openAI.name
output searchEndpoint string = 'https://${aiSearch.name}.search.windows.net'
output searchName string = aiSearch.name
