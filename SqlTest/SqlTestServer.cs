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
        internal static Server GetServerName()
        {
            try
            {
                string conn = ConfigurationManager.ConnectionStrings["testTarget"].ConnectionString;
                OleDbConnectionStringBuilder sb = new OleDbConnectionStringBuilder(conn);
                return new Server(sb.DataSource);
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("App.Config setting not found 'testTarget', be sure to add this to connectionStrings.");
            }
        }

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
