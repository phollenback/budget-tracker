using 'main.bicep'

param location = 'westus2'
param appName = 'budgettracker-ab'
param environmentName = 'prod'
param skuName = 'P1v3'
param skuCapacity = 1
param linuxFxVersion = 'DOTNETCORE|9.0'
param gitHubOrg = 'phollenback'
param gitHubRepo = 'budget-tracker'
param gitHubBranch = 'main'
param gitHubEnvironment = 'production'
param logRetentionInDays = 30
