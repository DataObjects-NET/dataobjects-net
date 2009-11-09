// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Data.SqlClient;
using Xtensive.Core;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.SqlServer
{
  internal static class ConnectionFactory
  {
    public static SqlServerConnection CreateConnection(UrlInfo url)
    {
      var builder = GetBuilder(url);
      return new SqlServerConnection(builder.ToString());
    }

    public static SqlServerConnection CreateConnection(SqlDriver driver, UrlInfo url)
    {
      var builder = GetBuilder(url);
      builder.MultipleActiveResultSets = driver.ServerInfo.MultipleActiveResultSets;
      return new SqlServerConnection(builder.ToString());
    }

    private static SqlConnectionStringBuilder GetBuilder(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new SqlConnectionStringBuilder();

      builder.InitialCatalog = url.Resource ?? string.Empty;

      if (url.Port==0)
        builder.DataSource = url.Host;
      else
        builder.DataSource = url.Host + "," + url.Port;
      if (!string.IsNullOrEmpty(url.User)) {
        builder.UserID = url.User;
        builder.Password = url.Password;
      }
      else {
        builder.IntegratedSecurity = true;
        builder.PersistSecurityInfo = false;
      }

      foreach (var param in url.Params)
        builder[param.Key] = param.Value;

      return builder;
    }
  }
}