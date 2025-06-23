using './reporting.bicep'

param teamName = ''
param reportingImage = ''
param sharedRgName = 'rg-hackathon-shared'
param containerEnvName = 'caenv-shared-westeurope'
param registryUsername = ''
param sharedUserAssignedIdentityResourceId = '/subscriptions/037e3bde-632b-48d8-85cb-91acee603751/resourcegroups/rg-hackathon-shared/providers/Microsoft.ManagedIdentity/userAssignedIdentities/shared-uami'
param containerAppSecrets = [
  {
    name: 'github-token'
    keyVaultUrl: 'https://kv-${teamName}-27410.vault.azure.net/secrets/github-token'
    identity: sharedUserAssignedIdentityResourceId
  }
  {
    name: 'open-ai-endpoint'
    keyVaultUrl: 'https://kv-${teamName}-27410.vault.azure.net/secrets/open-ai-endpoint'
    identity: sharedUserAssignedIdentityResourceId
  }
  {
    name: 'open-ai-api-key'
    keyVaultUrl: 'https://kv-${teamName}-27410.vault.azure.net/secrets/open-ai-api-key'
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
    name: 'ConnectionStrings__OpenAi'
    secretRef: 'open-ai-endpoint'
  }
  {
    name: 'ApiKeys__OpenAi'
    secretRef: 'open-ai-api-key'
  }
  {
    name: 'BaseUrls__SurveyApp'
    secretRef: 'blazor-survey-base-url'
  }
]

