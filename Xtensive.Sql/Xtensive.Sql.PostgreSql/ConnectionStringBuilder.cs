// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Npgsql;
using Xtensive.Core;

namespace Xtensive.Sql.PostgreSql
{
  internal static class ConnectionStringBuilder
  {
    public static string Build(ConnectionInfo connectionInfo)
    {
      if (!string.IsNullOrEmpty(connectionInfo.ConnectionString))
        return connectionInfo.ConnectionString;
      var url = connectionInfo.ConnectionUrl;

      SqlHelper.ValidateConnectionUrl(url);

      var builder = new NpgsqlConnectionStringBuilder();
      
      // host, port, database
      builder.Host = url.Host;
      if (url.Port!=0)
        builder.Port = url.Port;
      builder.Database = url.Resource ?? string.Empty;

      // user, password
      if (!string.IsNullOrEmpty(url.User)) {
        builder.UserName = url.User;
        builder.Password = url.Password;
      }
      else
        builder.IntegratedSecurity = true;

      // custom options
      foreach (var param in url.Params)
        builder[param.Key] = param.Value;

      return builder.ToString();
    }
  }
}