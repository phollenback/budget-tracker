using 'main.bicep'

param location = 'westus2'
param appName = 'budgettracker-oneapp'
param planName = 'budget-tracker-oneapp-plan'
param logAnalyticsName = 'budgettracker-oneapp-law'
param appInsightsName = 'budgettracker-oneapp'
param identityName = 'budgettracker-oneapp-id'
param environmentName = 'prod'
param skuName = 'P1v3'
param skuCapacity = 1
param linuxFxVersion = 'DOTNETCORE|9.0'
param gitHubOrg = 'phollenback'
param gitHubRepo = 'budget-tracker'
param gitHubBranch = 'main'
param gitHubEnvironment = 'oneapp'
param logRetentionInDays = 30
param experimentVariant = 'All'
