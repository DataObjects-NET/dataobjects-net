// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a common database entity.
  /// </summary>
  public class EntityInfo : LockableBase
  {
    private int maxIdentifierLength;
    private DdlStatements allowedDdlStatements;

    /// <summary>
    /// Gets or sets maximal identifier length.
    /// </summary>
    public int MaxIdentifierLength
    {
      get { return maxIdentifierLength; }
      set
      {
        EnsureNotLocked();
        maxIdentifierLength = value;
      }
    }

    /// <summary>
    /// Gets or sets allowed DDL statements for this instance.
    /// </summary>
    /// <value>Allowed DDL statements.</value>
    public DdlStatements AllowedDdlStatements
    {
      get { return allowedDdlStatements; }
      set
      {
        EnsureNotLocked();
        allowedDdlStatements = value;
      }
    }

  }
}
