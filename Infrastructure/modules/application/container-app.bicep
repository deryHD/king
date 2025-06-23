param location                         string = 'westeurope'
param containerAppEnvironmentId        string
param name                             string
param additionalTags                   object = {}
param costcenter                       string = ''
param registry                         object
param containerImageWithVersion        string
param targetPort                       int
param cpu                              string = '0.25'
param memory                           string = '0.5Gi'
param stickySessions                   string = 'none'
param secrets                          array = []
param environmentVariables             array = []
param userAssignedIdentityResourceId   string

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityResourceId}': {}
    }
  }
  properties: {
    environmentId: containerAppEnvironmentId
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: targetPort
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
        stickySessions: {
          affinity: stickySessions
        }
      }
      secrets: secrets
      registries: [
        registry
      ]
    }
    template: {
      containers: [
        {
          name: name
          image: containerImageWithVersion
          resources: {
            cpu:    json(cpu)
            memory: memory
          }
          env: environmentVariables
        }
      ]
    }
  }
  tags: union(additionalTags, { costcenter: costcenter })
}
