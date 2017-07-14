using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using System.Data.OleDb;
using System.Configuration;

namespace SqlTest
{
    class SqlTestServer
    {
        internal static Server GetSsisServer()
        {
            try
            {
                return new Server(ConfigurationManager.AppSettings["ssisServer"]);
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("App.Config setting not found 'ssisServer', be sure to add this to connectionStrings.");
            }
        }
    }
}
