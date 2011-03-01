using System;
using System.Security;
using global::MySql.Data.MySqlClient;
using Xtensive.Core;


namespace Xtensive.Sql.Drivers.MySql
{
    using Xtensive.Sql.Drivers.MySql.Resources;
    using Xtensive.Sql.Info;

    /// <summary>
    /// A <see cref="SqlDriver"/> factory for MySQL.
    /// </summary>
    public class DriverFactory : SqlDriverFactory
    {
        private const string DataSourceFormat = "{0}:{1}/{2}";
        private const string DatabaseAndSchemaQuery = "select current_database(), current_schema()";

        /// <inheritdoc/>
#if NET40
        [SecuritySafeCritical]
#endif
        public override string BuildConnectionString(UrlInfo url)
        {
            SqlHelper.ValidateConnectionUrl(url);

            var builder = new MySqlConnectionStringBuilder();

            // host, port, database
            builder.Server = url.Host;
            if (url.Port != 0)
                builder.Port = (uint)url.Port;
            builder.Database = url.Resource ?? string.Empty;

            // user, password
            if (!String.IsNullOrEmpty(url.User))
            {
                builder.UserID = url.User;
                builder.Password = url.Password;
            }
            else
                throw new Exception(Strings.ExUserNameRequired);

            // custom options
            foreach (var param in url.Params)
                builder[param.Key] = param.Value;

            return builder.ToString();
        }

        /// <inheritdoc/>
#if NET40
        [SecuritySafeCritical]
#endif
        public override SqlDriver CreateDriver(string connectionString)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                //TODO: Ensure that 'connection.ServerVersion' does not return null
                var version = new System.Version(connection.ServerVersion);

                var builder = new MySqlConnectionStringBuilder(connectionString);
                var dataSource = string.Format(DataSourceFormat, builder.Server, builder.Port, builder.Database);
                var coreServerInfo = new CoreServerInfo
                {
                    ServerLocation = new Location("mysql", dataSource),
                    ServerVersion = version,
                    ConnectionString = connectionString,
                    MultipleActiveResultSets = false,
                };

                SqlHelper.ReadDatabaseAndSchema(connection, DatabaseAndSchemaQuery, coreServerInfo);
                if (version.Major < 5)
                    throw new NotSupportedException(Strings.ExMySqlBelow50IsNotSupported);
                if (version.Major == 5 && version.Minor == 0)
                    return new v5.Driver(coreServerInfo);
                return new v5.Driver(coreServerInfo);
            }
        }
    }
}
