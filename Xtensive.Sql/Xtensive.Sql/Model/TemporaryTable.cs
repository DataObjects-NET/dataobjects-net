// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a temporary table object.
  /// </summary>
  [Serializable]
  public class TemporaryTable : Table
  {
    private bool isGlobal;
    private bool preserveRows;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is global. 
    /// If value is <see langword="false"/> the this instance is local.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance is global; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsGlobal
    {
      get { return isGlobal; }
      set
      {
        this.EnsureNotLocked();
        isGlobal = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether rows are preserved on commit.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if rows are preserved on commit; otherwise, <see langword="false"/>.
    /// </value>
    public bool PreserveRows
    {
      get { return preserveRows; }
      set
      {
        this.EnsureNotLocked();
        preserveRows = value;
      }
    }

   
    #region Constructors

    internal TemporaryTable(Schema schema, string name, bool isGlobal, bool preserveRows) : base(schema, name)
    {
      this.isGlobal = isGlobal;
      this.preserveRows = preserveRows;
    }

    internal TemporaryTable(Schema schema, string name) : this(schema, name, false, false)
    {
    }

    #endregion
  }
}
