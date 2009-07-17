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
      var connectionString = BuildConnectionString(url);
      return new SqlServerConnection(connectionString);
    }

    private static string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new SqlConnectionStringBuilder();

      builder.InitialCatalog = url.Resource ?? string.Empty;
      builder.MultipleActiveResultSets = true;
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

      return builder.ToString();
    }
  }
}