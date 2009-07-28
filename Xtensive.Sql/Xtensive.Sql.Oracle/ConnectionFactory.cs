// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.17

using Oracle.DataAccess.Client;
using Xtensive.Core;

namespace Xtensive.Sql.Oracle
{
  internal static class ConnectionFactory
  {
    private const int DefaultPort = 1521;
    private const string DataSourceFormat =
      "(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2})))";

    public static OracleConnection CreateConnection(UrlInfo url)
    {
      var connectionString = BuildConnectionString(url);
      return new OracleConnection(connectionString);
    }

    private static string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(url.Resource, "url.Resource");
      var builder = new OracleConnectionStringBuilder();
      var database = url.GetDatabase();
      if (url.Host!=string.Empty) {
        int port = url.Port!=0 ? url.Port : DefaultPort;
        builder.DataSource = string.Format(DataSourceFormat, url.Host, port, database);
      }
      else
        builder.DataSource = database;
      if (url.User!=string.Empty && url.Password!=string.Empty) {
        builder.UserID = url.User;
        builder.Password = url.Password;
      }
      else
        builder.UserID = "/";
      foreach (var parameter in url.Params)
        builder.Add(parameter.Key, parameter.Value);
      return builder.ToString();
    }
  }
}