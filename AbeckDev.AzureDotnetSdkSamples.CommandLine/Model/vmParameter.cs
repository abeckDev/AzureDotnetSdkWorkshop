using System;
using System.Collections.Generic;
using System.Text;

namespace AbeckDev.AzureDotnetSdkSamples.CommandLine.Model
{
    public class VmParameter
    {
        public VmParameter(string adminUsername, string adminPassword)
        {
            this.adminUsername.value = adminUsername;
            this.adminPassword.value = adminPassword;
        }
        public adminUsername adminUsername { get; set; } = new adminUsername();
        public adminPassword adminPassword { get; set; } = new adminPassword();
    }

    public class adminUsername
    {
        public string value { get; set; } = "";
    }

    public class adminPassword
    {
        public string value { get; set; } = "";
    }
}
