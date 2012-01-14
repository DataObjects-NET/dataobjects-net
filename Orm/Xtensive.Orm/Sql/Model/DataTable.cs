// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// An base class for <see cref="Table"/> and <see cref="View"/> types.
  /// </summary>
  [Serializable]
  public abstract class DataTable : SchemaNode
  {
    private PairedNodeCollection<DataTable, Index> indexes;

    /// <summary>
    /// Creates the index.
    /// </summary>
    /// <param name="name">The name.</param>
    public Index CreateIndex(string name)
    {
      return new Index(this, name);
    }

    /// <summary>
    /// Creates the full-text index.
    /// </summary>
    /// <param name="name">The name.</param>
    public FullTextIndex CreateFullTextIndex(string name)
    {
      return new FullTextIndex(this, name);
    }

    /// <summary>
    /// Creates the spatial index.
    /// </summary>
    /// <param name="name">The name.</param>
    public SpatialIndex CreateSpatialIndex(string name)
    {
      return new SpatialIndex(this, name);
    }

    /// <summary>
    /// Gets the indexes.
    /// </summary>
    /// <value>The indexes.</value>
    public PairedNodeCollection<DataTable, Index> Indexes
    {
      get { return indexes; }
    }

    #region DataTable Members

    /// <summary>
    /// Gets the columns.
    /// </summary>
    /// <value>The columns.</value>
    public abstract IList<DataTableColumn> Columns { get; }
    
    #endregion

    #region ILockable Members

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked too.</param>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      indexes.Lock(recursive);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTable"/> class.
    /// </summary>
    /// <param name="schema">The schema.</param>
    /// <param name="name">The name.</param>
    protected DataTable(Schema schema, string name) : base(schema, name)
    {
      indexes = new PairedNodeCollection<DataTable, Index>(this, "Indexes");
    }

    #endregion
  }
}
