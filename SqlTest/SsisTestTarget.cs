using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.IntegrationServices;
using System.Collections.ObjectModel;
using System.IO;

namespace SqlTest
{
    public class SsisTestTarget
    {
        public string PackageFolder { get; set; }
        public string ProjectName { get; set; }
        public bool UseEnvironment { get; set; }
        public string EnvironmentFolder { get; set; }
        public string EnvironmentName { get; set; }
        bool HasFailed { get; set; }

        public SsisTestTarget(string packageFolder, string projectName, bool useEnvironment, string environmentFolder = "", string environmentName = "")
        {
            this.PackageFolder = packageFolder;
            this.ProjectName = projectName;
            this.UseEnvironment = useEnvironment;
            this.EnvironmentFolder = environmentFolder;
            this.EnvironmentName = environmentName;
            this.HasFailed = false;
        }

        public static void ExecutePackage(FileInfo packagePath)
        {
            throw new NotImplementedException();
            //TODO: implement ExecutePackageFromFile
            /*
             * ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = $"/F {packagePath.FullName} /rep N";
            info.CreateNoWindow = true;
            info.FileName = "dtexec";
            info.UseShellExecute = false;
            int result;
            using (Process p = Process.Start(info))
            {
                p.WaitForExit();
                result = p.ExitCode;
            }

            return result;
            */
        }

        public void ExecutePackage(string packageName, bool failOnWarning = true)
        {
            try
            {
                IntegrationServices integrationServices = new IntegrationServices(SqlTestServer.GetSsisServer());
                PackageInfo package = GetPackage(integrationServices, packageName);
                Collection<PackageInfo.ExecutionValueParameterSet> paramSet = new Collection<PackageInfo.ExecutionValueParameterSet>();
                paramSet.Add(new PackageInfo.ExecutionValueParameterSet { ParameterName = "SYNCHRONIZED", ParameterValue = 1, ObjectType = 50 });

                EnvironmentReference env = null;
                if (this.UseEnvironment)
                {
                    env = GetEnvironment(package);
                }
                long executionId = package.Execute(false, env, paramSet);
                string errorsAndWarnings = GetMessages(integrationServices, executionId, failOnWarning);
                var status = integrationServices.Catalogs["SSISDB"].Executions[executionId].Status;

                if (this.HasFailed || status != Operation.ServerOperationStatus.Success)
                {
                    throw new Exception($"The package '{packageName}' has failed with the following warnings and errors: {errorsAndWarnings}");
                }
            }

            catch (Exception e)
            {
                StringBuilder errorMessage = new StringBuilder();
                errorMessage.AppendLine($"An error was thrown while executing a package with the following parameters:");
                errorMessage.AppendLine($"Folder: {this.PackageFolder}");
                errorMessage.AppendLine($"Project: {this.ProjectName}");
                errorMessage.AppendLine($"Package: {packageName}");
                errorMessage.AppendLine($"Environment Folder: {this.EnvironmentFolder}");
                errorMessage.AppendLine($"Environment: {this.EnvironmentName}");
                errorMessage.AppendLine($"Fail on Warning: {failOnWarning}");
                errorMessage.AppendLine($"Error Message: {e.Message}");
                throw new Exception(errorMessage.ToString());
            }
        }

        private string GetMessages(IntegrationServices integrationServices, long executionId, bool failOnWarning)
        {
            StringBuilder messages = new StringBuilder();
            foreach(var message in integrationServices.Catalogs["SSISDB"].Executions[executionId].Messages)
            {
                if(message.MessageType == 120)
                {
                    messages.AppendLine($"{message.MessageTime} - Error: {message.Message}");
                    this.HasFailed = true;
                }

                if(failOnWarning && message.MessageType == 110 && !message.Message.Contains("global shared memory"))
                {
                    messages.AppendLine($"{message.MessageTime} - Warning: {message.Message}");
                    this.HasFailed = true;
                }

                if (message.MessageType == 130)
                {
                    messages.AppendLine($"{message.MessageTime} - Task Failed: {message.Message}");
                }
            }

            return messages.ToString();
        }

        private PackageInfo GetPackage(IntegrationServices ssis, string packageName)
        {
            string package = packageName.Contains(".dtsx") ? packageName : $"{packageName}.dtsx";
            if(ssis.Catalogs["SSISDB"].Folders[this.PackageFolder] == null)
            {
                throw new Exception($"The folder '{this.PackageFolder}' was not found in the SSIS catalog.");
            }

            if(ssis.Catalogs["SSISDB"].Folders[this.PackageFolder].Projects[this.ProjectName] == null)
            {
                throw new Exception($"The project '{this.ProjectName}' was not found in the folder '{this.PackageFolder} in the SSIS Catalog");
            }

            if(ssis.Catalogs["SSISDB"].Folders[this.PackageFolder].Projects[this.ProjectName].Packages[package] == null)
            {
                throw new Exception($"The package '{package}' was not found in the project '{this.ProjectName}' in the SSIS Catalog");
            }
            return ssis.Catalogs["SSISDB"].Folders[this.PackageFolder].Projects[this.ProjectName].Packages[package];
        }

        private EnvironmentReference GetEnvironment(PackageInfo package)
        {
            string envFolder = this.EnvironmentFolder;
            if(package.Parent.References[this.EnvironmentName, envFolder] == null)
            {
                if (package.Parent.References[this.EnvironmentName, "."] == null)
                {
                    throw new Exception($"The environment '{this.EnvironmentName}' was not found in the folder '{envFolder}' or '.'");
                }
                else
                {
                    envFolder = ".";
                }
            }
            return package.Parent.References[this.EnvironmentName, envFolder];
        }
    }
}

