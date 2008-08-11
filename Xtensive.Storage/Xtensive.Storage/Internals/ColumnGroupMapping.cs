// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal class ColumnGroupMapping
  {
    private ThreadSafeDictionary<TypeInfo, TypeMapping> typeMappings;

    public int TypeIdIndex { get; private set; }

    public IDictionary<ColumnInfo, Column> ColumnInfoMapping { get; private set; }

    public ThreadSafeDictionary<TypeInfo, TypeMapping> TypeMappings
    {
      get { return typeMappings; }
    }


    // Constructors

    public ColumnGroupMapping(int typeIdIndex, Dictionary<ColumnInfo, Column> columnMapping)
    {
      TypeIdIndex = typeIdIndex;
      typeMappings = ThreadSafeDictionary<TypeInfo, TypeMapping>.Create(this);
      ColumnInfoMapping = new ReadOnlyDictionary<ColumnInfo, Column>(columnMapping);
    }
  }
}