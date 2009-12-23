// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.23

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Describes a full-text index in terms of storage.
  /// </summary>
  [Serializable]
  public class FullTextIndexInfo : Node 
  {
    private readonly IndexInfo primaryIndex;
    private readonly ColumnInfoCollection keyColumns = new ColumnInfoCollection();
    private readonly ColumnInfoCollection columns = new ColumnInfoCollection();
    private readonly ColumnInfoCollection includedColumns = new ColumnInfoCollection();

    /// <summary>
    /// Gets the primary index.
    /// </summary>
    public IndexInfo PrimaryIndex
    {
      get { return primaryIndex; }
    }

    /// <summary>
    /// Gets the key columns.
    /// </summary>
    public ColumnInfoCollection KeyColumns
    {
      get { return keyColumns; }
    }

    /// <summary>
    /// Gets the index columns.
    /// </summary>
    public ColumnInfoCollection Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets the included columns.
    /// </summary>
    public ColumnInfoCollection IncludedColumns
    {
      get { return includedColumns; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      KeyColumns.Lock(true);
      Columns.Lock(true);
      IncludedColumns.Lock(true);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public FullTextIndexInfo(IndexInfo primaryIndex, string name)
      : base(name)
    {
      this.primaryIndex=primaryIndex;
    }
  }
}