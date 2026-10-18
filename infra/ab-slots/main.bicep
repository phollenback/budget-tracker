targetScope = 'resourceGroup'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Globally unique App Service name. Must match GitHub workflow AZURE_WEBAPP_NAME.')
param appName string = 'budgettracker-ab'

param planName string = 'budget-tracker-prod-plan'
param logAnalyticsName string = 'budgettracker-law'
param appInsightsName string = 'budgettracker'
param identityName string = 'budgettracker-id'

@description('Short environment label used in tags.')
param environmentName string = 'prod'

param skuName string = 'P1v3'
param skuCapacity int = 1
param linuxFxVersion string = 'DOTNETCORE|9.0'
param gitHubOrg string = 'phollenback'
param gitHubRepo string = 'budget-tracker'
param gitHubBranch string = 'main'
param gitHubEnvironment string = 'production'
param logRetentionInDays int = 30

param yearlySlotName string = 'yearly'
param bucketsSlotName string = 'buckets'

@minValue(0)
@maxValue(100)
param yearlyTrafficPercent int = 50

@minValue(0)
@maxValue(100)
param bucketsTrafficPercent int = 50

module host '../modules/linux-web-app.bicep' = {
  name: 'ab-slots-host'
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
    }
    skuName: skuName
    skuCapacity: skuCapacity
    linuxFxVersion: linuxFxVersion
    gitHubOrg: gitHubOrg
    gitHubRepo: gitHubRepo
    gitHubBranch: gitHubBranch
    gitHubEnvironment: gitHubEnvironment
    logRetentionInDays: logRetentionInDays
    extraAppSettings: {}
    clientAffinityEnabled: true
    experimentSlots: [
      {
        name: yearlySlotName
        variant: 'Yearly'
        trafficPercent: yearlyTrafficPercent
      }
      {
        name: bucketsSlotName
        variant: 'Buckets'
        trafficPercent: bucketsTrafficPercent
      }
    ]
  }
}

output webAppName string = host.outputs.webAppName
output webAppUrl string = host.outputs.webAppUrl
output slotUrls array = host.outputs.slotUrls
output identityClientId string = host.outputs.identityClientId
output identityResourceId string = host.outputs.identityResourceId
