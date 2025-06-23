using './survey.bicep'

param teamName = ''
param surveyImage = ''
param sharedRgName = 'rg-hackathon-shared'
param containerEnvName = 'caenv-shared-westeurope'
param registryUsername = ''
param sharedUserAssignedIdentityResourceId = '/subscriptions/037e3bde-632b-48d8-85cb-91acee603751/resourcegroups/rg-hackathon-shared/providers/Microsoft.ManagedIdentity/userAssignedIdentities/shared-uami'
param containerAppSecrets = [
  {
    name: 'app-db-connstr'
    keyVaultUrl: 'https://kv-${teamName}-27410.vault.azure.net/secrets/app-db-connstr'
    identity: sharedUserAssignedIdentityResourceId
  }
  {
    name: 'github-token'
    keyVaultUrl: 'https://kv-${teamName}-27410.vault.azure.net/secrets/github-token'
    identity: sharedUserAssignedIdentityResourceId
  }
  {
    name: 'acs-connstr'
    keyVaultUrl: 'https://kv-${teamName}-27410.vault.azure.net/secrets/acs-connstr'
    identity: sharedUserAssignedIdentityResourceId
  } 
  {
    name: 'acs-sender-email'
    keyVaultUrl: 'https://kv-${teamName}-27410.vault.azure.net/secrets/acs-sender-email'
    identity: sharedUserAssignedIdentityResourceId
  } 
  {
    name: 'blazor-survey-base-url'
    keyVaultUrl: 'https://kv-${teamName}-27410.vault.azure.net/secrets/blazor-survey-base-url'
    identity: sharedUserAssignedIdentityResourceId
  }
]
param environmentVariables = [
  {
    name: 'ConnectionStrings__DefaultConnection'
    secretRef: 'app-db-connstr'
  }
  {
    name: 'ConnectionStrings__CommunicationService'
    secretRef: 'acs-connstr'
  }
  {
    name: 'BaseUrls__SurveyApp'
    secretRef: 'blazor-survey-base-url'
  }
  {
    name: 'SenderEmails__CommunicationService'
    secretRef: 'acs-sender-email'
  }
]
