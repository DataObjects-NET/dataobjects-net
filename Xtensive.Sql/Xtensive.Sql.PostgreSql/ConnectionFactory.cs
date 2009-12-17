using Npgsql;
using Xtensive.Core;

namespace Xtensive.Sql.PostgreSql
{
  internal static class ConnectionFactory
  {
    public static NpgsqlConnection CreateConnection(UrlInfo url)
    {
      var connectionString = BuildConnectionString(url);
      return new NpgsqlConnection(connectionString);
    }

    private static string BuildConnectionString(UrlInfo url)
    {
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