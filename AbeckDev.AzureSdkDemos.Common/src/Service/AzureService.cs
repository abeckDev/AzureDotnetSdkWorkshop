using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Deployment.Definition;
using System;
using System.Collections.Generic;

namespace AbeckDev.AzureSdkDemos.Common.Service
{
    public static class AzureService
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
        /// List all existing ResourceGroups in an Azure Subscription
        /// </summary>
        /// <param name="azure">The <see cref="IAzure"/> Interface used to communicate with Azure</param>
        /// <returns>Returns an <see cref="IEnumerable{IResourceGroup}"/></returns>
        public static IEnumerable<IResourceGroup> ListResourceGroups(IAzure azure)
        {
            return azure.ResourceGroups.List();
        }


        /// <summary>
        /// Creates a Azure Resource Group based on the input
        /// </summary>
        /// <param name="azure">The <see cref="IAzure"/> Interface used to communicate with Azure</param>
        /// <param name="ResourceGroupName">The name of the Resource Group</param>
        /// <param name="ResourceGroupRegion">The Region of the Resource Group</param>
        /// <returns>Returns an <see cref="IResourceGroup"/> object with information about the created ResourceGroup</returns>
        /// <exception cref="Exception">Throws an exception if the Resource Group already exists</exception>
        public static IResourceGroup CreateResourceGroup(IAzure azure, string ResourceGroupName, string ResourceGroupRegion)
        {
            return azure.ResourceGroups.Define(ResourceGroupName)
                .WithRegion(ResourceGroupRegion)
                .Create();
        }



        /// <summary>
        /// Deploys an Azure Resource Manager (ARM) Template based on an existing Azure connection.
        /// </summary>
        /// <param name="azure">The <see cref="IAzure"/> Interface from the existing connection</param>
        /// <param name="ResourceGroupName">The Name of the ResourceGroup where to deploy the Resources. Will be created if not existing</param>
        /// <param name="templateJson">The ARM Template File in Json</param>
        /// <param name="parametersJson">The Parameters File in Json</param>
        /// <returns>An <see cref="IDeployment"/> Object with information of the deployment operation in Azure</returns>
        public static IDeployment DeployArmTemplate(IAzure azure, string ResourceGroupName, string templateJson, object parameters)
        {
            string DeploymentName = "Deployment-" + Guid.NewGuid().ToString();
            var deployment = azure.Deployments.Define(DeploymentName)
                .WithExistingResourceGroup(ResourceGroupName)
                .WithTemplate(templateJson)
                .WithParameters(parameters)
                .WithMode(Microsoft.Azure.Management.ResourceManager.Fluent.Models.DeploymentMode.Incremental)
                .BeginCreate();

            return azure.Deployments.GetByName(DeploymentName);
        }

        /// <summary>
        /// Deletes a Resource Group by its name
        /// </summary>
        /// <param name="azure">The <see cref="IAzure"/> Interface from the existing connection</param>
        /// <param name="ResourceGroupName">The Name of the Resource Group</param>
        public static void DeleteResourceGroup(IAzure azure, string ResourceGroupName)
        {
            azure.ResourceGroups.DeleteByName(ResourceGroupName);
        }
    }
}
