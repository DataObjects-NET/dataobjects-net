// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Common.Mssql
{
  /// <summary>
  /// An abstract class which represents RDBMS version.
  /// </summary>
  /// <remarks>
  /// Every driver should implement its own XXXVersion overriding set of 
  /// methods implemented in this class.
  /// </remarks>
  public class MssqlVersionInfo : VersionInfo
  {
    private string productLevel;
    private string editionName;
    private MssqlEdition edition;
    private MssqlEngineEdition engineEdition;

    /// <summary>
    /// Gets a level of MSSQL server instance, e.g. "RTM" or "SP2".
    /// </summary>
    public string ProductLevel
    {
      get { return productLevel; }
      internal set { productLevel = value; }
    }

    /// <summary>
    /// Gets a name of edition of MSSQL server instance.
    /// </summary>
    public string EditionName
    {
      get { return editionName; }
      internal set { editionName = value; }
    }

    /// <summary>
    /// Gets edition of MSSQL server instance.
    /// </summary>
    public MssqlEdition Edition
    {
      get { return edition; }
      internal set { edition = value; }
    }

    /// <summary>
    /// Gets edition of MSSQL server instance engine.
    /// </summary>
    public MssqlEngineEdition EngineEdition
    {
      get { return engineEdition; }
      internal set { engineEdition = value; }
    }

    /// <summary>
    /// Returns a <see cref="string"/> representation of the
    /// current <see cref="VersionInfo"/>.
    /// </summary>
    public override string ToString()
    {
      return "MSSQL v"+base.ToString()+" "+ProductLevel+" "+EditionName;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="productVersion"></param>
    public MssqlVersionInfo(Version productVersion) : base(productVersion)
    {
    }
  }
}