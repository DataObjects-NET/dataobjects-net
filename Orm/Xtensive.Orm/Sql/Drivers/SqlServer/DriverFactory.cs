// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql.Info;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer
{
  /// <summary>
  /// A <see cref="SqlDriver"/> factory for Microsoft SQL Server.
  /// </summary>
  public class DriverFactory : SqlDriverFactory
  {
    private const string DatabaseAndSchemaQuery =
      "select db_name(), coalesce(default_schema_name, 'dbo') from sys.database_principals where name=user";

    private const string MessagesQuery = @"Declare @MSGLANGID int; 
      Select @MSGLANGID = msglangid FROM [master].[sys].[syslanguages] lang
      WHERE lang.langid = @@LANGID;
      SELECT msg.error , msg.description 
      FROM [master].[sys].[sysmessages] msg
      WHERE   msg.msglangid = @MSGLANGID AND msg.error IN ( 2627, 2601, 515, 547 )";

    private static ErrorMessageParser CreateMessageParser(SqlServerConnection connection)
    {
      bool isEnglish;
      using (var command = connection.CreateCommand()) {
        command.CommandText = "SELECT @@LANGID";
        isEnglish = command.ExecuteScalar().ToString()=="0";
      }
      var templates = new Dictionary<int, string>();
      using (var command = connection.CreateCommand()) {
        command.CommandText = MessagesQuery;
        using (var reader = command.ExecuteReader())
          while (reader.Read())
            templates.Add(reader.GetInt32(0), reader.GetString(1));
      }
      return new ErrorMessageParser(templates, isEnglish);
    }

    private static bool IsAzure(SqlServerConnection connection)
    {
      using (var command = connection.CreateCommand()) {
        command.CommandText = "SELECT @@VERSION";
        return ((string) command.ExecuteScalar()).Contains("Azure");
      }
    }

    /// <inheritdoc/>
    protected override string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var builder = new SqlConnectionStringBuilder();
      
      // host, port, database
      if (url.Port==0)
        builder.DataSource = url.Host;
      else
        builder.DataSource = url.Host + "," + url.Port;
      builder.InitialCatalog = url.Resource ?? string.Empty;

      // user, password
      if (!String.IsNullOrEmpty(url.User)) {
        builder.UserID = url.User;
        builder.Password = url.Password;
      }
      else {
        builder.IntegratedSecurity = true;
        builder.PersistSecurityInfo = false;
      }

      // custom options
      foreach (var param in url.Params)
        builder[param.Key] = param.Value;

      return builder.ToString();
    }

    /// <inheritdoc/>
    protected override SqlDriver CreateDriver(string connectionString, SqlDriverConfiguration configuration)
    {
      using (var connection = new SqlServerConnection(connectionString)) {
        connection.Open();
        SqlHelper.ExecuteInitializationSql(connection, configuration);

        string versionString;
        bool isAzure;

        var forcedServerVersion = configuration.ForcedServerVersion;
        if (string.IsNullOrEmpty(forcedServerVersion)) {
          versionString = connection.ServerVersion;
          isAzure = IsAzure(connection);
        }
        else if (forcedServerVersion.Equals("azure", StringComparison.OrdinalIgnoreCase)) {
          versionString = "10.0.0.0";
          isAzure = true;
        }
        else {
          versionString = forcedServerVersion;
          isAzure = false;
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        var version = new Version(versionString);
        var defaultSchema = GetDefaultSchema(connection);
        var coreServerInfo = new CoreServerInfo {
          ServerVersion = version,
          ConnectionString = connectionString,
          MultipleActiveResultSets = builder.MultipleActiveResultSets,
          DatabaseName = defaultSchema.Database,
          DefaultSchemaName = defaultSchema.Schema,
        };
        if (isAzure)
          return new Azure.Driver(coreServerInfo, new ErrorMessageParser());
        if (version.Major < 9)
          throw new NotSupportedException(Strings.ExSqlServerBelow2005IsNotSupported);
        var parser = CreateMessageParser(connection);
        if (version.Major==9)
          return new v09.Driver(coreServerInfo, parser);
        if (version.Major==10)
          return new v10.Driver(coreServerInfo, parser);
        if (version.Major==11)
          return new v11.Driver(coreServerInfo, parser);
        if (version.Major==12)
          return new v12.Driver(coreServerInfo, parser);
        if (version.Major==13)
          return new v13.Driver(coreServerInfo, parser);
        return new v13.Driver(coreServerInfo, parser);
      }
    }

    /// <inheritdoc/>
    protected override DefaultSchemaInfo ReadDefaultSchema(DbConnection connection, DbTransaction transaction)
    {
      return SqlHelper.ReadDatabaseAndSchema(DatabaseAndSchemaQuery, connection, transaction);
    }
  }
}