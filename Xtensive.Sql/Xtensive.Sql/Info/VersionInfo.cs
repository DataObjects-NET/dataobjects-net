// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// A class which represents RDBMS version.
  /// </summary>
  public class VersionInfo
  {
    private Version productVersion;

    /// <summary>
    /// A version of RDBMS.
    /// </summary>
    public Version ProductVersion
    {
      get { return productVersion; }
    }

    /// <summary>
    /// Returns a <see cref="string"/> representation of the
    /// current <see cref="VersionInfo"/>.
    /// </summary>
    public override string ToString()
    {
      return productVersion.ToString();
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="productVersion"></param>
    public VersionInfo(Version productVersion)
    {
      this.productVersion = productVersion;
    }
  }
}