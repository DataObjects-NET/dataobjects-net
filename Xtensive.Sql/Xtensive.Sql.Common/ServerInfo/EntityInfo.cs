// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes a common database entity.
  /// </summary>
  public class EntityInfo : LockableBase
  {
    private int maxIdentifierLength;
    private DdlStatements allowedDdlStatements;
    public static EntityInfo Empty = new EntityInfo();

    /// <summary>
    /// Gets or sets maximal identifier length.
    /// </summary>
    public int MaxIdentifierLength
    {
      get { return maxIdentifierLength; }
      set
      {
        this.EnsureNotLocked();
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
        this.EnsureNotLocked();
        allowedDdlStatements = value;
      }
    }

  }
}
