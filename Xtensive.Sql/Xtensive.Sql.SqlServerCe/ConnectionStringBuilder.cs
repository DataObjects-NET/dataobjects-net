// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;

namespace Xtensive.Sql.SqlServerCe
{
  internal static class ConnectionStringBuilder
  {
    public static string Build(ConnectionInfo connectionInfo)
    {
      if (!string.IsNullOrEmpty(connectionInfo.ConnectionString))
        return connectionInfo.ConnectionString;
      var url = connectionInfo.ConnectionUrl;

      SqlHelper.ValidateConnectionUrl(url);

      string result = string.Format("Data Source = '{0}'", url.Resource);
      
      if (!string.IsNullOrEmpty(url.Password))
        result += string.Format("; Password = '{0}'", url.Password);
      
      return result;

//      var builder = new SqlConnectionStringBuilder();
//      
//      // host, port, database
//      if (url.Port==0)
//        builder.DataSource = url.Host;
//      else
//        builder.DataSource = url.Host + "," + url.Port;
//      builder.InitialCatalog = url.Resource ?? string.Empty;
//
//      // user, password
//      if (!string.IsNullOrEmpty(url.User)) {
//        builder.UserID = url.User;
//        builder.Password = url.Password;
//      }
//      else {
//        builder.IntegratedSecurity = true;
//        builder.PersistSecurityInfo = false;
//      }
//
//      // custom options
//      foreach (var param in url.Params)
//        builder[param.Key] = param.Value;
//
//      return builder.ToString();
    }
  }
}