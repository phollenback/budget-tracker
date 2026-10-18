targetScope = 'resourceGroup'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Globally unique App Service name. Must match GitHub workflow AZURE_WEBAPP_NAME.')
param appName string = 'budgettracker-oneapp'

param planName string = 'budget-tracker-oneapp-plan'
param logAnalyticsName string = 'budgettracker-oneapp-law'
param appInsightsName string = 'budgettracker-oneapp'
param identityName string = 'budgettracker-oneapp-id'

@description('Short environment label used in tags.')
param environmentName string = 'prod'

param skuName string = 'P1v3'
param skuCapacity int = 1
param linuxFxVersion string = 'DOTNETCORE|9.0'
param gitHubOrg string = 'phollenback'
param gitHubRepo string = 'budget-tracker'
param gitHubBranch string = 'main'
param gitHubEnvironment string = 'oneapp'
param logRetentionInDays int = 30

@description('Off, Yearly, Buckets, or All. All exposes both experiment pages on this one site.')
param experimentVariant string = 'All'

module host '../modules/linux-web-app.bicep' = {
  name: 'one-app-host'
  params: {
    location: location
    appName: appName
    planName: planName
    logAnalyticsName: logAnalyticsName
    appInsightsName: appInsightsName
    identityName: identityName
    tags: {
      env: environmentName
      app: appName
      deploy: 'one-app'
    }
    skuName: skuName
    skuCapacity: skuCapacity
    linuxFxVersion: linuxFxVersion
    gitHubOrg: gitHubOrg
    gitHubRepo: gitHubRepo
    gitHubBranch: gitHubBranch
    gitHubEnvironment: gitHubEnvironment
    logRetentionInDays: logRetentionInDays
    extraAppSettings: {
      BudgetExperiment__Variant: experimentVariant
    }
    clientAffinityEnabled: false
    experimentSlots: []
  }
}

output webAppName string = host.outputs.webAppName
output webAppUrl string = host.outputs.webAppUrl
output identityClientId string = host.outputs.identityClientId
output identityResourceId string = host.outputs.identityResourceId
