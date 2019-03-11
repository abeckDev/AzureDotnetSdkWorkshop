using AbeckDev.AzureSdkDemos.CommandLine.Model;
using AbeckDev.AzureSdkDemos.Common.Service;
//using AbeckDev.AzureSdkDemos.CommandLine.Service;
using ConsoleTableExt;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Text;

namespace AbeckDev.AzureSdkDemos.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Establish Azure Connection");

            //Get Credentials from the User
            Console.Write("Enter Azure ClientId: ");
            string clientId = Console.ReadLine();
            Console.Write("Enter Azure ClientSecret: ");
            string clientSecret = GetConsolePassword();
            Console.Write("Enter Azure tenantId: ");
            string tenantId = Console.ReadLine();
            Console.Write("Enter Azure SubscriptionId: ");
            string subscriptionId = Console.ReadLine();
            Console.Write("Enter Azure cloud (global, german) [global]: ");
            AzureEnvironment azureEnvironment = GetAzureEnvironment(Console.ReadLine().ToString());

            try
            {
                //Connect to Azure
                IAzure azure = AzureService.GetAzureInterface(clientId, clientSecret, tenantId, subscriptionId);
                //Verify connection
                azure.ResourceGroups.List();

                Console.WriteLine("Connection to Azure established!");
                while (true)
                {
                    Console.WriteLine("List all Resource Groups [1]");
                    Console.WriteLine("Create a Resource Group [2]");
                    Console.WriteLine("Deploy a ARM Template [3]");
                    Console.WriteLine("Delete Resource Group [4]");
                    Console.WriteLine("Exit [exit]");
                    Console.Write("Whats next?: ");
                    string decision = Console.ReadLine();
                    
                    switch (decision.ToLower())
                    {
                        case "1":
                            var ResourceGroups = AzureService.ListResourceGroups(azure);
                            DataTable table = new DataTable();
                            table.Columns.Add("Id");
                            table.Columns.Add("Name");
                            table.Columns.Add("Provisioning state");
                            foreach (var ResourceGroup in ResourceGroups)
                            {
                                table.Rows.Add(ResourceGroup.Id, ResourceGroup.Name, ResourceGroup.ProvisioningState);
                            }
                            ConsoleTableBuilder.From(table)
                                .WithFormat(ConsoleTableBuilderFormat.Alternative)
                                .ExportAndWriteLine();
                            break;
                        case "2":

                            Console.Write("Name of the new ResourceGroup: ");
                            string ResourceGroupName = Console.ReadLine();
                            Console.Write("Region of the ResourceGroup: ");
                            string ResourceGroupRegion = Console.ReadLine();
                            try
                            {
                                var ResourceGroup = AzureService.CreateResourceGroup(azure, ResourceGroupName, ResourceGroupRegion);
                                Console.WriteLine("Done: " + ResourceGroup.Id);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error during creation of ResourceGroup");
                                Console.WriteLine(e.Message);
                            }
                            break;
                        case "3":
                            Console.Write("File Path to ARM File: ");
                            string FilePathArmFile = Console.ReadLine();
                            string ArmFileJson = null;
                            try
                            {
                                ArmFileJson = File.ReadAllText(FilePathArmFile);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error during reading the ARM File");
                                Console.WriteLine(e.Message);
                            }
                            Console.Write("ResourceGroupName: ");
                            string DeploymentResourceGroup = Console.ReadLine();
                            Console.Write("AdminUsername: ");
                            string adminUsername = Console.ReadLine();
                            Console.Write("AdminPassword: ");
                            string adminPassword = GetConsolePassword();
                            var Parameters = new VmParameter(adminUsername, adminPassword);
                            try
                            {
                                IDeployment deployment = AzureService.DeployArmTemplate(azure, DeploymentResourceGroup, ArmFileJson, Parameters);
                                Console.WriteLine("Deployment ("+ deployment.Name +") started: " + deployment.ProvisioningState);
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("Error during ARM deployment");
                                Console.WriteLine(e.Message);
                            }
                            break;
                        case "4":
                            Console.Write("Name of the ResourceGroup to delete: ");
                            string DeleteResourceGroupName = Console.ReadLine();
                            try
                            {
                                AzureService.DeleteResourceGroup(azure, DeleteResourceGroupName);
                                Console.WriteLine(DeleteResourceGroupName + "successfully deleted");
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("Error during deletetion of ResourceGroup: " + DeleteResourceGroupName);
                                Console.WriteLine(e.Message);
                            }

                            break;
                        case "exit":
                            System.Environment.Exit(0);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Damn something went wrong. :(");
                Console.WriteLine(e.Message);
            }

#if DEBUG
            Console.WriteLine("Program exited!");
            Console.ReadLine();
#endif
        }



        static AzureEnvironment GetAzureEnvironment(string environment)
        {
            switch (environment.ToString())
            {
                case "german":
                    return AzureEnvironment.AzureGermanCloud;
                default:
                    return AzureEnvironment.AzureGlobalCloud;
            }
        }
        private static string GetConsolePassword()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        Console.Write("\b\0\b");
                        sb.Length--;
                    }
                    continue;
                }
                Console.Write('*');
                sb.Append(cki.KeyChar);
            }
            return sb.ToString();
        }
    }
}
