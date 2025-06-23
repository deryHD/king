targetScope = 'resourceGroup'

param teamName            string
param sharedRgName        string
param containerEnvName    string
param location            string = resourceGroup().location
param registryUsername    string
param reportingImage      string
param containerAppSecrets array = []
param environmentVariables array = []
param sharedUserAssignedIdentityResourceId string

resource caEnv 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerEnvName
  scope: resourceGroup(sharedRgName)
}

module reportingApp 'modules/application/container-app.bicep' = {
  name: 'deploy-report-${teamName}'
  scope: resourceGroup()
  params: {
    location:                  location
    containerAppEnvironmentId: caEnv.id
    name:                      'report-${teamName}'
    additionalTags:            { team: teamName, role: 'report' }
    costcenter:                ''
    registry:                  {
      server:            'ghcr.io'
      username:          registryUsername
      passwordSecretRef: 'github-token'
    }
    containerImageWithVersion: reportingImage
    targetPort:               8080
    cpu:                      '0.5'
    memory:                   '1Gi'
    stickySessions:           'none'
    secrets:                  containerAppSecrets
    environmentVariables:     environmentVariables
    userAssignedIdentityResourceId: sharedUserAssignedIdentityResourceId
  }
}
