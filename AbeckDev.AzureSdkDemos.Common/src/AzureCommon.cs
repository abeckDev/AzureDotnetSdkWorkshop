using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Deployment.Definition;
using System;
using System.Collections.Generic;

namespace AbeckDev.AzureSdkDemos.Common
{
    public static class AzureCommon
    {
        /// <summary>
        /// Gets an <see cref="IAzure"/> by authenticating against Azure AD with a Service Principal
        /// </summary>
        /// <param name="clientId">The clientId from the AppRegistration in AzureAD</param>
        /// <param name="clientSecret">The clientSecret from the AppRegistration in AzureAD</param>
        /// <param name="tenantId">The Azure TenantId aka DirectoryId</param>
        /// <param name="subscriptionId">(optional) The Subscription to use. Default one is used if empty</param>
        /// <returns>An <see cref="IAzure"/> Interface for communication with the Azure Cloud</returns>
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

        /// <summary>
        /// Deploys an Azure Resource Manager (ARM) Template based on an existing Azure connection.
        /// </summary>
        /// <param name="azure">The <see cref="IAzure"/> Interface from the existing connection</param>
        /// <param name="ResourceGroupName">The Name of the ResourceGroup where to deploy the Resources. Will be created if not existing</param>
        /// <param name="templateJson">The ARM Template File in Json</param>
        /// <param name="parametersJson">The Parameters File in Json</param>
        /// <returns>An <see cref="IDeployment"/> Object with information of the deployment operation in Azure</returns>
        public static IDeployment DeployArmTemplate(IAzure azure, string ResourceGroupName, string templateJson, string parametersJson)
        {
            if (!azure.ResourceGroups.Contain(ResourceGroupName))
            {
                CreateResourceGroup(azure, ResourceGroupName);
            }
            string deploymentName = ResourceGroupName + "-" + Guid.NewGuid().ToString();
            azure.Deployments.Define(deploymentName)
                .WithExistingResourceGroup(ResourceGroupName)
                .WithTemplate(templateJson)
                .WithParameters(parametersJson)
                .WithMode(Microsoft.Azure.Management.ResourceManager.Fluent.Models.DeploymentMode.Incremental)
                .BeginCreate();
            var result = azure.Deployments
                .GetByName(deploymentName);
            return result;
        }

        public static IResourceGroup CreateResourceGroup(IAzure azure, string ResourceGroupName)
        {
            if (azure.ResourceGroups.Contain(ResourceGroupName))
            {
                //ResourceGroup exists abort
                throw new Exception("The ResourceGroup exist already");
            }

            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("Sample Tag Key", "Sample Tag Value");
            tags.Add("CreationDate", DateTime.Now.Date.ToLongDateString());

            return azure.ResourceGroups.Define(ResourceGroupName)
                .WithRegion(Region.EuropeWest)
                .WithTags(tags)
                .Create();
        }
    }
}
