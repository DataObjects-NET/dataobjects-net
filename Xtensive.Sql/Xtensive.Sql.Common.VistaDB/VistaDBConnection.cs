// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data.Common;
using System.Text;
using VistaDBProvider = VistaDB.Provider;

namespace Xtensive.Sql.Common.VistaDB
{
  public class VistaDBConnection : Connection
  {
    public static DbConnection GetRealConnection(ConnectionInfo info)
    {
      char[] forbiddenChars = new char[] { '=', ';' };
      Exception e = new ArgumentException(@"Part of URL contains ""="" or "";"" characters.", "info");
      if (info.Host.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (info.Database.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (info.User.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (info.Password.IndexOfAny(forbiddenChars)>=0)
        throw e;

      StringBuilder sbConnectionString = new StringBuilder();

      sbConnectionString.Append("Data Source="+info.Database+";");

      string openMode;
      info.Params.TryGetValue("OpenMode", out openMode);
      if (openMode!=null && openMode.Length>0)
        sbConnectionString.Append("Open Mode="+openMode+";");

      string password;
      info.Params.TryGetValue("Password", out password);
      if (password!=null)
        sbConnectionString.Append("Password="+openMode+";");

      string minPoolSize;
      info.Params.TryGetValue("MinPoolSize", out minPoolSize);
      if (minPoolSize!=null) {
        int mpsi;
        if (!Int32.TryParse(minPoolSize, out mpsi) || mpsi<0)
          throw new ArgumentException("URL contains invalid MinPoolSize specification.", "info");
        sbConnectionString.Append("Min Pool Size="+mpsi+";");
      }

      string contextConnection;
      info.Params.TryGetValue("ContextConnection", out contextConnection);
      if (contextConnection!=null) {
        bool ccb;
        if (!Boolean.TryParse(contextConnection, out ccb))
          throw new ArgumentException("URL contains invalid ContextConnection specification.", "info");
        sbConnectionString.Append("Context Connection="+(ccb ? "true" : "false")+";");
      }

      string isolatedStorage;
      info.Params.TryGetValue("IsolatedStorage", out isolatedStorage);
      if (isolatedStorage!=null) {
        bool isb;
        if (!Boolean.TryParse(isolatedStorage, out isb))
          throw new ArgumentException("URL contains invalid IsolatedStorage specification.", "info");
        sbConnectionString.Append("Isolated Storage="+(isb ? "true" : "false")+";");
      }

      return new global::VistaDB.Provider.VistaDBConnection(sbConnectionString.ToString());
    }

    public VistaDBConnection(Driver driver, ConnectionInfo info)
      : base(driver, GetRealConnection(info), info)
    {
    }
  }
}
