// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Text;
using VistaDB.Provider;

namespace Xtensive.Sql.VistaDb
{
  internal static class ConnectionFactory 
  {
    public static VistaDBConnection CreateConnection(SqlConnectionUrl url)
    {
      var connectionString = BuildConnectionString(url);
      return new VistaDBConnection(connectionString);
    }

    private static string BuildConnectionString(SqlConnectionUrl url)
    {
      char[] forbiddenChars = new [] { '=', ';' };
      Exception e = new ArgumentException(@"Part of URL contains ""="" or "";"" characters.", "url");
      if (url.Host.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (url.Database.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (url.User.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (url.Password.IndexOfAny(forbiddenChars)>=0)
        throw e;

      var connectionString = new StringBuilder();

      connectionString.Append("Data Source="+url.Database+";");

      string openMode;
      url.Params.TryGetValue("OpenMode", out openMode);
      if (!string.IsNullOrEmpty(openMode))
        connectionString.Append("Open Mode="+openMode+";");

      string password;
      url.Params.TryGetValue("Password", out password);
      if (password!=null)
        connectionString.Append("Password="+openMode+";");

      string minPoolSize;
      url.Params.TryGetValue("MinPoolSize", out minPoolSize);
      if (minPoolSize!=null) {
        int mpsi;
        if (!Int32.TryParse(minPoolSize, out mpsi) || mpsi<0)
          throw new ArgumentException("URL contains invalid MinPoolSize specification.", "url");
        connectionString.Append("Min Pool Size="+mpsi+";");
      }

      string contextConnection;
      url.Params.TryGetValue("ContextConnection", out contextConnection);
      if (contextConnection!=null) {
        bool ccb;
        if (!Boolean.TryParse(contextConnection, out ccb))
          throw new ArgumentException("URL contains invalid ContextConnection specification.", "url");
        connectionString.Append("Context Connection="+(ccb ? "true" : "false")+";");
      }

      string isolatedStorage;
      url.Params.TryGetValue("IsolatedStorage", out isolatedStorage);
      if (isolatedStorage!=null) {
        bool isb;
        if (!Boolean.TryParse(isolatedStorage, out isb))
          throw new ArgumentException("URL contains invalid IsolatedStorage specification.", "url");
        connectionString.Append("Isolated Storage="+(isb ? "true" : "false")+";");
      }

      return connectionString.ToString();
    }
  }
}