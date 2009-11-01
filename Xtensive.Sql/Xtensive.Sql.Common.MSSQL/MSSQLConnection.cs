// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Xtensive.Sql.Common.Mssql
{
  public class MssqlConnection : Connection
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

      if (info.Port==0)
        sbConnectionString.Append("Data Source="+info.Host+";");
      else
        sbConnectionString.Append("Data Source="+info.Host+","+info.Port+";");
      sbConnectionString.Append("Initial Catalog="+(info.Database==null ? "" : info.Database)+";");
      if (info.User!="") {
        sbConnectionString.Append("User ID="+info.User+";");
        sbConnectionString.Append("Password="+info.Password+";");
      }
      else
        sbConnectionString.Append("Integrated Security=SSPI;Persist Security Info=False;");
#if !MONO
      sbConnectionString.Append("Enlist=False;");
#endif

      string ct;
      info.Params.TryGetValue("ConnectionTimeout", out ct);
      if (ct!=null) {
        int cti;
        if (!Int32.TryParse(ct, out cti) || cti<0)
          throw new ArgumentException("URL contains invalid ConnectionTimeout specification.", "info");
        sbConnectionString.Append("Connect Timeout="+cti+";");
      }

      string applicationName;
      info.Params.TryGetValue("ApplicationName", out applicationName);
      if (applicationName!=null) {
        applicationName = applicationName.Replace(";", "");
        if (applicationName.Length>0)
          sbConnectionString.Append("Application Name="+applicationName+";");
      }

      string attachDbFile;
      info.Params.TryGetValue("AttachDbFile", out attachDbFile);
      if (attachDbFile!=null) {
        attachDbFile = attachDbFile.Replace(";", "");
        if (attachDbFile.Length>0)
          sbConnectionString.Append("AttachDBFilename="+attachDbFile+";");
      }

      string connectionStringSuffix;
      info.Params.TryGetValue("ConnectionStringSuffix", out connectionStringSuffix);
      if (connectionStringSuffix!=null) {
        if (connectionStringSuffix.Length>=2) {
          if ((connectionStringSuffix.StartsWith("'") && connectionStringSuffix.EndsWith("'"))
              || (connectionStringSuffix.StartsWith("\"") && connectionStringSuffix.EndsWith("\"")))
            connectionStringSuffix = connectionStringSuffix.Substring(1, connectionStringSuffix.Length - 2);
        }
        if (connectionStringSuffix.Length>0)
          sbConnectionString.Append(connectionStringSuffix);
      }

      return new SqlConnection(sbConnectionString.ToString());
    }

    public MssqlConnection(ConnectionInfo info, Driver driver)
      : base(driver, GetRealConnection(info), info)
    {
    }
  }
}