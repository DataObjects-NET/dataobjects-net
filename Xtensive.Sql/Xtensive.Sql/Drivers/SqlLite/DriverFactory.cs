// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.04.29

using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using Xtensive.Core;
using Xtensive.Sql.Drivers.SQLite.Resources;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.SQLite
{
    /// <summary>
    /// A <see cref="SqlDriver"/> factory for SQLite.
    /// </summary>
    public class DriverFactory : SqlDriverFactory
    {
        private static readonly Regex DataSourceExtractor = new Regex(
          @"(.*Data Source *= *)(.*)($|;.*)",
          RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private string GetDataSource(string connectionString)
        {
            var match = DataSourceExtractor.Match(connectionString);
            if (!match.Success)
                return "<unknown>";
            var dataSource = match.Groups[2].Captures[0].Value;
            if (dataSource.Length > 1 && dataSource.StartsWith("'") && dataSource.EndsWith("'"))
                dataSource = dataSource.Substring(1, dataSource.Length - 2);
            return dataSource;
        }

        /// <inheritdoc/>
        public override SqlDriver CreateDriver(string connectionString)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var version = new System.Version(connection.ServerVersion);

                var builder = new SQLiteConnectionStringBuilder(connectionString);
                builder.FailIfMissing = false;
                var dataSource = GetDataSource(connectionString);
                var coreServerInfo = new CoreServerInfo
                {
                    ServerLocation = new Location("sqlite", dataSource),
                    ServerVersion = version,
                    ConnectionString = connectionString,
                    DefaultSchemaName = "Main",
                    DatabaseName = dataSource,
                    MultipleActiveResultSets = false,
                };

                if (version.Major < 3)
                    throw new NotSupportedException(Strings.ExSqliteBelow3IsNotSupported);
                return new v3.Driver(coreServerInfo);
            }
        }

        /// <inheritdoc/>
        public override string BuildConnectionString(UrlInfo url)
        {
            SqlHelper.ValidateConnectionUrl(url);
            string result = string.Format("Data Source = {0}", url.Resource);

            if (!String.IsNullOrEmpty(url.Password))
                result += String.Format("; Password = '{0}'", url.Password);

            return result;
        }
    }
}