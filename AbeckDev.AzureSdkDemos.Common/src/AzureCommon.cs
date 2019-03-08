using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using System;

namespace AbeckDev.AzureSdkDemos.Common
{
    public static class AzureCommon
    {
        public static IAzure GetAzureInterface(string clientId, string clientSecret, string tenantId, string subscriptionId = null)
        {

            AzureCredentials azureCredentials = new AzureCredentialsFactory()
                .FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);

            var authenticated = Azure.Authenticate(azureCredentials);

            if (subscriptionId == null)
            {
                return authenticated.WithDefaultSubscription();
            }

            return authenticated.WithSubscription(subscriptionId);
        }
    }
}
