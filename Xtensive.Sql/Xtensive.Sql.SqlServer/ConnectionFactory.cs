// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
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
      char[] forbiddenChars = new [] { '=', ';' };
      Exception e = new ArgumentException(@"Part of URL contains ""="" or "";"" characters.", "url");
      if (url.Host.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (url.Resource.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (url.User.IndexOfAny(forbiddenChars)>=0)
        throw e;
      if (url.Password.IndexOfAny(forbiddenChars)>=0)
        throw e;

      var sb = new SqlConnectionStringBuilder();

      sb.InitialCatalog = url.Resource ?? string.Empty;
      sb.MultipleActiveResultSets = true;
      if (url.Port==0)
        sb.DataSource = url.Host;
      else
        sb.DataSource = url.Host+","+url.Port;
      if (!string.IsNullOrEmpty(url.User)) {
        sb.UserID = url.User;
        sb.Password = url.Password;
      }
      else {
        sb.IntegratedSecurity = true;
        sb.PersistSecurityInfo = false;
      }
#if !MONO
      sb.Enlist = false;
#endif

      foreach (var param in url.Params)
        sb[param.Key] = param.Value;

      return sb.ToString();

//      string ct;
//      info.Params.TryGetValue("ConnectionTimeout", out ct);
//      if (ct!=null) {
//        int cti;
//        if (!Int32.TryParse(ct, out cti) || cti<0)
//          throw new ArgumentException("URL contains invalid ConnectionTimeout specification.", "info");
//        sbConnectionString.Append("Connect Timeout="+cti+";");
//      }
//
//      string applicationName;
//      info.Params.TryGetValue("ApplicationName", out applicationName);
//      if (applicationName!=null) {
//        applicationName = applicationName.Replace(";", "");
//        if (applicationName.Length>0)
//          sbConnectionString.Append("Application Name="+applicationName+";");
//      }
//
//      string attachDbFile;
//      info.Params.TryGetValue("AttachDbFile", out attachDbFile);
//      if (attachDbFile!=null) {
//        attachDbFile = attachDbFile.Replace(";", "");
//        if (attachDbFile.Length>0)
//          sbConnectionString.Append("AttachDBFilename="+attachDbFile+";");
//      }
//
//      string connectionStringSuffix;
//      info.Params.TryGetValue("ConnectionStringSuffix", out connectionStringSuffix);
//      if (connectionStringSuffix!=null) {
//        if (connectionStringSuffix.Length>=2) {
//          if ((connectionStringSuffix.StartsWith("'") && connectionStringSuffix.EndsWith("'"))
//              || (connectionStringSuffix.StartsWith("\"") && connectionStringSuffix.EndsWith("\"")))
//            connectionStringSuffix = connectionStringSuffix.Substring(1, connectionStringSuffix.Length - 2);
//        }
//        if (connectionStringSuffix.Length>0)
//          sbConnectionString.Append(connectionStringSuffix);
//      }
    }
  }
}