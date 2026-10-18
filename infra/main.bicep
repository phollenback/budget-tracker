targetScope = 'resourceGroup'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Globally unique App Service name. Must match GitHub workflow AZURE_WEBAPP_NAME.')
param appName string = 'budgettracker-ab'

@description('Short environment label used in tags and resource names.')
param environmentName string = 'prod'

@description('Linux App Service plan SKU name (P1v3 matches current prod).')
param skuName string = 'P1v3'

@minValue(1)
param skuCapacity int = 1

@description('App runtime stack.')
param linuxFxVersion string = 'DOTNETCORE|9.0'

@description('GitHub org or user that owns the repo (for OIDC federated identity).')
param gitHubOrg string = 'phollenback'

@description('GitHub repository name.')
param gitHubRepo string = 'budget-tracker'

@description('Branch allowed to request tokens via OIDC.')
param gitHubBranch string = 'main'

@description('GitHub Environment name used by the deploy job.')
param gitHubEnvironment string = 'production'

@description('Log Analytics retention in days.')
param logRetentionInDays int = 30

module abSlots 'ab-slots/main.bicep' = {
  name: 'ab-slots'
  params: {
    location: location
    appName: appName
    environmentName: environmentName
    skuName: skuName
    skuCapacity: skuCapacity
    linuxFxVersion: linuxFxVersion
    gitHubOrg: gitHubOrg
    gitHubRepo: gitHubRepo
    gitHubBranch: gitHubBranch
    gitHubEnvironment: gitHubEnvironment
    logRetentionInDays: logRetentionInDays
  }
}

output webAppName string = abSlots.outputs.webAppName
output webAppUrl string = abSlots.outputs.webAppUrl
output slotUrls array = abSlots.outputs.slotUrls
output identityClientId string = abSlots.outputs.identityClientId
output identityResourceId string = abSlots.outputs.identityResourceId
