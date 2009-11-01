// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Common;

namespace Xtensive.Sql.Dom
{
  /// <summary>
  /// The <see cref="SqlConnectionProvider"/> is responsible for identification
  /// of suitable RDBMS driver by the specified <see cref="ConnectionInfo">conection URL</see>
  /// and for creating valid <see cref="SqlConnection"/> instance via selected driver.
  /// </summary>
  public class SqlConnectionProvider : ConnectionProvider
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnectionProvider"/> class.
    /// </summary>
    /// <param name="path">The path to search for drivers.</param>
    /// <remarks>
    ///   <para>This version of constructor creates a <see cref="SqlConnectionProvider"/>
    /// instance that will lookup for direct non <see langword="abstract"/>
    ///     <see cref="SqlDriver"/> descendants while searching a driver
    /// by the specified connection URL.</para>
    /// </remarks>
    public SqlConnectionProvider(string path) : base(typeof(SqlDriver), path)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnectionProvider"/> class.
    /// </summary>
    public SqlConnectionProvider() : this(AppDomain.CurrentDomain.BaseDirectory)
    {
    }
  }
}