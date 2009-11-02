// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Runtime.Serialization;
using Xtensive.Core;

namespace Xtensive.Sql
{
  /// <summary>
  /// Holds end-point URL and provides easy access to its defferent parts.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Please see <see cref="UrlInfo"/> class for complete description of URL format.
  /// </para>
  /// <para>
  /// Here you can see several valid URL samples:
  /// <pre>
  /// mssql://localhost/Demos
  /// mssql2005://server\sqlexpress/Demos
  /// oracle://admin:admin@localhost/Demos
  /// msaccess://localhost/C:\Debug\demos.mdb
  /// firebird://SYSDBA:masterkey@localhost/Demos
  /// oracle://ADMIN:admin@192.168.37.128/Demos
  /// mssql://admin:admin@localhost/Demos?ConnectionTimeout=60
  /// mssql://user@localhost/Demos?ApplicationName=MyApp
  /// </pre>
  /// </para>
  /// </remarks>
  [Serializable]
  public sealed class SqlConnectionUrl : UrlInfo
  {
    /// <summary>
    /// Gets the database name (<see cref="UrlInfo.Resource"/>) part of the current <see cref="UrlInfo.Url"/>
    /// (e.g. <b>"database"</b> is the database name part of the "mssql://admin:password@localhost/<b>database</b>" URL).
    /// </summary>
    public string Database { get { return Resource; } }
    
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="url">Initial <see cref="UrlInfo.Url"/> property value.</param>
    public SqlConnectionUrl(string url)
      : base(url)
    {
    }

    ///<summary>
    /// Deserilizing constructor.
    ///</summary>
    /// <param name="context">The source (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this deserialization. </param>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate the data from. </param>
    private SqlConnectionUrl(SerializationInfo info, StreamingContext context)
      : base(info, context) 
    {
    }
  }
}