// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

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
  /// Describes a single index.
  /// </summary>
  [Serializable]
  public class IndexInfo : Node<IndexInfo, StorageInfo>
  {
    private ReadOnlyList<ColumnInfo> keyColumnsCache;
    private ReadOnlyList<ColumnInfo> valueColumnsCache;
    private int pageSize;

    /// <summary>
    /// Gets or sets the columns.
    /// </summary>
    /// <value>The columns.</value>
    public ColumnInfoCollection Columns
    {
      [DebuggerStepThrough]
      get;
      [DebuggerStepThrough]
      private set;
    }

    /// <summary>
    /// Gets the key columns.
    /// </summary>
    /// <value>The key columns.</value>
    public ReadOnlyList<ColumnInfo> KeyColumns
    {
      get
      {
        if (IsLocked)
          return keyColumnsCache;

        return GetKeyColumns();
      }
    }

    /// <summary>
    /// Gets the value columns.
    /// </summary>
    /// <value>The value columns.</value>
    public ReadOnlyList<ColumnInfo> ValueColumns
    {
      get
      {
        if (IsLocked)
          return valueColumnsCache;

        return GetValueColumns();
      }
    }

    /// <summary>
    /// Gets or sets the size of the page.
    /// </summary>
    /// <value>The size of the page.</value>
    public int PageSize
    {
      get { return pageSize; }
      set
      {
        this.EnsureNotLocked();
        pageSize = value;
      }
    }

    /// <inheritdoc/>
    protected override NodeCollection<IndexInfo, StorageInfo> GetParentNodeCollection()
    {
      return Parent==null ? null : Parent.Indexes;
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      Columns.Lock(recursive);
      keyColumnsCache = GetKeyColumns();
      valueColumnsCache = GetValueColumns();
    }

    #region Private methods
    
    private ReadOnlyList<ColumnInfo> GetKeyColumns()
    {
      return new ReadOnlyList<ColumnInfo>(
          new List<ColumnInfo>(Columns.Where(ci => ci.Type != ColumnType.Value)));
    }

    private ReadOnlyList<ColumnInfo> GetValueColumns()
    {
      return new ReadOnlyList<ColumnInfo>(
          new List<ColumnInfo>(Columns.Where(ci => ci.Type == ColumnType.Value)));
    }
    
    #endregion

    // Consturctors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="storage"><see cref="Storage"/> property value.</param>
    /// <param name="name">Initial <see cref="Node.Name"/> property value.</param>
    public IndexInfo(StorageInfo storage, string name)
      : base(storage, name)
    {
      Columns = new ColumnInfoCollection(this);
    }
  }
}