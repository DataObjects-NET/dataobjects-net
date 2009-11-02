// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Info;

namespace Xtensive.Sql.SqlServer
{
  /// <summary>
  /// An abstract class which represents RDBMS version.
  /// </summary>
  /// <remarks>
  /// Every driver should implement its own XXXVersion overriding set of 
  /// methods implemented in this class.
  /// </remarks>
  public class SqlServerVersionInfo : VersionInfo
  {
    /// <summary>
    /// Gets a level of MSSQL server instance, e.g. "RTM" or "SP2".
    /// </summary>
    public string ProductLevel { get; internal set; }

    /// <summary>
    /// Gets a name of edition of MSSQL server instance.
    /// </summary>
    public string EditionName { get; internal set; }

    /// <summary>
    /// Gets edition of MSSQL server instance.
    /// </summary>
    public SqlServerEdition Edition { get; internal set; }

    /// <summary>
    /// Gets edition of MSSQL server instance engine.
    /// </summary>
    public SqlServerEngineEdition EngineEdition { get; internal set; }

    /// <summary>
    /// Returns a <see cref="string"/> representation of the
    /// current <see cref="VersionInfo"/>.
    /// </summary>
    public override string ToString()
    {
      return "MSSQL v" + base.ToString() + " " + ProductLevel + " " + EditionName;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="productVersion"></param>
    public SqlServerVersionInfo(Version productVersion)
      : base(productVersion)
    {
    }
  }
}