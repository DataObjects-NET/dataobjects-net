// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;
using System.Collections.Generic;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Table.
  /// </summary>
  [Serializable]
  public sealed class TableInfo : NodeBase<StorageInfo>
  {
    private PrimaryIndexInfo primaryIndex;

    /// <summary>
    /// Gets columns.
    /// </summary>
    [Property(Priority = -2000)]
    public ColumnInfoCollection Columns { get; private set; }

    /// <summary>
    /// Gets or sets the primary index.
    /// </summary>
    [Property(Priority = -1200, IsImmutable = true, RecreateParent = true)]
    public PrimaryIndexInfo PrimaryIndex {
      get { return primaryIndex; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("PrimaryIndex", value)) {
          primaryIndex = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets secondary indexes.
    /// </summary>
    [Property(Priority = -1100, IsImmutable = true)]
    public SecondaryIndexInfoCollection SecondaryIndexes { get; private set; }

    /// <summary>
    /// Gets foreign keys.
    /// </summary>
    [Property(Priority = -1000, IsImmutable = true, DependencyRootType = typeof (TableInfoCollection))]
    public ForeignKeyCollection ForeignKeys { get; private set; }

    /// <summary>
    /// Gets full-text indexes.
    /// </summary>
    [Property(Priority = -900, IsImmutable = true)]
    public FullTextIndexInfoCollection FullTextIndexes { get; private set;}

    /// <summary>
    /// Gets all indexes belongs to the table.
    /// </summary>
    /// <returns><see cref="IndexInfo"/> iterator.</returns>
    public IEnumerable<IndexInfo> AllIndexes
    {
      get
      {
        yield return PrimaryIndex;
        foreach (var indexInfo in SecondaryIndexes)
          yield return indexInfo;
      }
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<TableInfo, StorageInfo, TableInfoCollection>(this, "Tables");
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();

      if (Columns==null)
        Columns = new ColumnInfoCollection(this);
      if (SecondaryIndexes == null)
        SecondaryIndexes = new SecondaryIndexInfoCollection(this);
      if (FullTextIndexes == null)
        FullTextIndexes = new FullTextIndexInfoCollection(this);
      if (ForeignKeys == null)
        ForeignKeys = new ForeignKeyCollection(this);
    }


    // Constructors

    /// <inheritdoc/>
    public TableInfo(StorageInfo parent, string name)
      : base(parent, name)
    {
    }
  }
}