// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single secondary index.
  /// </summary>
  [Serializable]
  public class SecondaryIndexInfo: IndexInfo
  {
    private ReadOnlyList<ColumnInfo> primaryKeyColumnsCache;
    private ReadOnlyList<ColumnInfo> secondaryKeyColumnsCache;
    private string primaryIndexName;
    private bool isUnique;

    /// <summary>
    /// Gets or sets the name of the primary index.
    /// </summary>
    /// <value>The name of the primary index.</value>
    public string PrimaryIndexName
    {
      [DebuggerStepThrough]
      get { return primaryIndexName; }
      [DebuggerStepThrough]
      set
      {
        this.EnsureNotLocked();
        primaryIndexName = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether index is unique.
    /// </summary>
    /// <value>
    /// 	<see langword="true"/> if this index is unique; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsUnique
    {
      get { return isUnique; }
      set
      {
        this.EnsureNotLocked();
        isUnique = value;
      }
    }

    /// <summary>
    /// Gets the primary key columns.
    /// </summary>
    /// <value>The primary key columns.</value>
    public ReadOnlyList<ColumnInfo> PrimaryKeyColumns
    {
      get
      {
        if (IsLocked)
          return primaryKeyColumnsCache;

          return GetPrimaryKeyColumns();
      }
    }

    /// <summary>
    /// Gets the secondary key columns.
    /// </summary>
    /// <value>The secondary key columns.</value>
    public ReadOnlyList<ColumnInfo> SecondaryKeyColumns
    {
      get
      {
        if (IsLocked)
          return secondaryKeyColumnsCache;

          return GetSecondaryKeyColumns();
      } 
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      primaryKeyColumnsCache = GetPrimaryKeyColumns();
      secondaryKeyColumnsCache = GetSecondaryKeyColumns();
    }

    #region Private methods

    private ReadOnlyList<ColumnInfo> GetPrimaryKeyColumns()
    {
      return new ReadOnlyList<ColumnInfo>(
          new List<ColumnInfo>(Columns.Where(ci => ci.Type == ColumnType.PrimaryKey)));
    }

    private ReadOnlyList<ColumnInfo> GetSecondaryKeyColumns()
    {
      return new ReadOnlyList<ColumnInfo>(
        new List<ColumnInfo>(Columns.Where(ci => ci.Type == ColumnType.SecondaryKey)));
    }

    #endregion

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage"><see cref="Storage"/> property value.</param>
    /// <param name="name">Initial <see cref="Node.Name"/> property value.</param>
    public SecondaryIndexInfo(StorageInfo storage, string name)
      : base(storage, name)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage">The storage.</param>
    /// <param name="name">The name.</param>
    /// <param name="primaryIndexName">Name of the primary index.</param>
    public SecondaryIndexInfo(StorageInfo storage, string name, string primaryIndexName)
      : base(storage, name)
    {
      this.primaryIndexName = primaryIndexName;
    }
  }
}