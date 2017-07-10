using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SqlTest;

namespace SqlTest.Tests
{
    [TestFixture]
    public class Ssis
    {
        public SqlTest.Ssis ssisWithoutEnvironment;
        public SqlTest.Ssis ssisWithEnvironment;
        [SetUp]
        public void Setup()
        {
            ssisWithoutEnvironment = new SqlTest.Ssis(
                                        packageFolder: "ETL"
                                        , projectName: "SsisProjectDeployment"
                                        , useEnvironment: false);

            ssisWithEnvironment = new SqlTest.Ssis(
                                        packageFolder: "ETL"
                                        , projectName: "SsisProjectDeployment"
                                        , useEnvironment: true
                                        , environmentFolder: "ETL"
                                        , environmentName: "SsisProjectDeployment"
                                        );
        }

        [Test]
        public void Ssis_ExecutePackageThatSucceedsWithoutEnvironment_Succeeds()
        {
            Assert.DoesNotThrow(delegate { ssisWithoutEnvironment.ExecutePackage("PackageThatSucceeds"); });
        }

        [Test]
        public void Ssis_ExecutePackageThatFailsWithoutEnvironment_Fails()
        {
            Assert.Throws(typeof(Exception), 
                    delegate { ssisWithoutEnvironment.ExecutePackage("PackageThatFails"); });
        }

        [Test]
        public void Ssis_ExecutePackageWithEnvironment_Succeeds()
        {
            Assert.DoesNotThrow(delegate { ssisWithEnvironment.ExecutePackage("PackageWithEnvironment"); });
        }

        [Test]
        public void Ssis_FailOnWarningTrue_PackageFails()
        {
            Assert.Throws(typeof(Exception),
                    delegate { ssisWithEnvironment.ExecutePackage("PackageWithWarning", failOnWarning: true); });
        }

        [Test]
        public void Ssis_FailOnWarningFalse_PackageSucceeds()
        {
            Assert.DoesNotThrow(delegate { ssisWithEnvironment.ExecutePackage("PackageWithWarning", failOnWarning: false); });
        }
    }
}
