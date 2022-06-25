// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a function in the current catalog that maps the rows of a table or index
  /// into partitions based on the values of a specified column.
  /// </summary>
  [Serializable]
  public class PartitionFunction : CatalogNode
  {
    private SqlValueType dataType;
    private BoundaryType boundaryType = BoundaryType.Left;
    private string[] boundaryValues;

    /// <summary>
    /// Gets or sets the data type of the column used for partitioning.
    /// </summary>
    /// <value>The data type of the column used for partitioning.</value>
    public SqlValueType DataType
    {
      get { return dataType; }
      set
      {
        EnsureNotLocked();
        dataType = value;
      }
    }

    /// <summary>
    /// Gets or sets the boundary type that affects the partitioning behavior.
    /// </summary>
    /// <value>The boundary type.</value>
    public BoundaryType BoundaryType
    {
      get { return boundaryType; }
      set
      {
        EnsureNotLocked();
        boundaryType = value;
      }
    }

    /// <summary>
    /// Gets or sets the boundary values for each partition of a partitioned table or index 
    /// that uses this partition function.
    /// </summary>
    /// <value>The boundary values.</value>
    public string[] BoundaryValues
    {
      get { return boundaryValues; }
      set
      {
        EnsureNotLocked();
        boundaryValues = value;
      }
    }

    #region CatalogNode Members

    /// <summary>
    /// Changes the catalog.
    /// </summary>
    /// <param name="value">The new value of catalog property.</param>
    protected override void ChangeCatalog(Catalog value)
    {
      if (Catalog!=null)
        Catalog.PartitionFunctions.Remove(this);
      if (value!=null)
        value.PartitionFunctions.Add(this);
    }

    #endregion

    #region Constructors

    internal PartitionFunction(Catalog catalog, string name, SqlValueType dataType,
                             params string[] boundaryValues) : base(catalog, name)
    {
      this.dataType = dataType;
      this.boundaryValues = boundaryValues;
    }

    #endregion
  }
}