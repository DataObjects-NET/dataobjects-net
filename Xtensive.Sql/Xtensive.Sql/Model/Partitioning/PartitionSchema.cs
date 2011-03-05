// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a schema in the current <see cref="Catalog"/> that maps the partitions of a 
  /// partitioned table or index to filegroups. The number and domain of the partitions of a 
  /// partitioned table or index are determined in a <see cref="PartitionFunction"/>.
  /// </summary>
  [Serializable]
  public class PartitionSchema : CatalogNode
  {
    private PartitionFunction partitionFunction;
    private IList<string> filegroups = new Collection<string>();

    /// <summary>
    /// Specifies the names of the filegroups to hold the partitions specified
    /// by <see cref="PartitionFunction"/>. Filegroup name must already exist in the <see cref="Catalog"/>.
    /// </summary>
    /// <value>The filegroups.</value>
    public IList<string> Filegroups
    {
      get { return filegroups; }
    }

    /// <summary>
    /// Gets or sets the partition function.
    /// </summary>
    /// <value>The partition function.</value>
    public PartitionFunction PartitionFunction
    {
      get { return partitionFunction; }
      set {
        this.EnsureNotLocked();
        if (partitionFunction == value)
          return;
        if (value!=null && Catalog!=null && !Catalog.PartitionFunctions.Contains(value))
          Catalog.PartitionFunctions.Add(value);
        partitionFunction = value;
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
        Catalog.PartitionSchemas.Remove(this);
      if (value!=null)
        value.PartitionSchemas.Add(this);
    }

    #endregion

    #region Constructors

    internal PartitionSchema(Catalog catalog, string name, PartitionFunction partitionFunction,
                           params string[] filegroups) : base(catalog, name)
    {
      this.partitionFunction = partitionFunction;
      foreach (string filegroup in filegroups)
        this.filegroups.Add(filegroup);
    }

    #endregion
  }
}