using 'main.bicep'

param location = 'westus2'
param appName = 'budgettracker-ab'
param planName = 'budget-tracker-prod-plan'
param logAnalyticsName = 'budgettracker-law'
param appInsightsName = 'budgettracker'
param identityName = 'budgettracker-id'
param environmentName = 'prod'
param skuName = 'P1v3'
param skuCapacity = 1
param linuxFxVersion = 'DOTNETCORE|9.0'
param gitHubOrg = 'phollenback'
param gitHubRepo = 'budget-tracker'
param gitHubBranch = 'main'
param gitHubEnvironment = 'production'
param logRetentionInDays = 30
param yearlySlotName = 'yearly'
param bucketsSlotName = 'buckets'
param yearlyTrafficPercent = 50
param bucketsTrafficPercent = 50
