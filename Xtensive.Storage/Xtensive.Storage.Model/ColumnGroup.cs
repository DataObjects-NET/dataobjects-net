// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.01

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class ColumnGroup
  {
    private readonly HierarchyInfo hierarchy;
    private readonly CollectionBaseSlim<ColumnInfo> keyColumns;
    private readonly CollectionBaseSlim<ColumnInfo> columns;

    public HierarchyInfo Hierarchy
    {
      get { return hierarchy; }
    }

    public CollectionBaseSlim<ColumnInfo> KeyColumns
    {
      get { return keyColumns; }
    }

    public CollectionBaseSlim<ColumnInfo> Columns
    {
      get { return columns; }
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ColumnGroup(HierarchyInfo hierarchyInfo, IEnumerable<ColumnInfo> keyColumns, IEnumerable<ColumnInfo> columns)
    {
      this.hierarchy = hierarchyInfo;
      this.keyColumns = new CollectionBaseSlim<ColumnInfo>(keyColumns);
      this.columns = new CollectionBaseSlim<ColumnInfo>(columns);
    }
  }
}