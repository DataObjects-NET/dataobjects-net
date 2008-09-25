// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal sealed class ColumnGroupMapping
  {
    private readonly Dictionary<TypeInfo, TypeMapping> typeMappings = new Dictionary<TypeInfo, TypeMapping>();
    private readonly Dictionary<ColumnInfo, MappedColumn> columnsMapping;
    
    public int TypeIdIndex { get; private set; }

    public TypeMapping GetTypeMapping(TypeInfo type)
    {
      TypeMapping mapping;
      if (typeMappings.TryGetValue(type, out mapping))
        return mapping;
      lock(typeMappings) {
        if (!typeMappings.TryGetValue(type, out mapping)) {
          mapping = BuildTypeMapping(type);
          typeMappings[type] = mapping;
        }
      }
      return mapping;
    }

    private TypeMapping BuildTypeMapping(TypeInfo type)
    {
      var map = new int[type.Columns.Count];
      for (int i = 0; i < type.Columns.Count; i++) {
        ColumnInfo columnInfo = type.Columns[i];
        MappedColumn column;
        if (columnsMapping.TryGetValue(columnInfo, out column))
          map[i] = column.Index;
        else
          map[i] = MapTransform.NoMapping;
      }
      return new TypeMapping(type, new MapTransform(true, type.TupleDescriptor, map));
    }


    // Constructors

    public ColumnGroupMapping(int typeIdIndex, Dictionary<ColumnInfo, MappedColumn> columnMapping)
    {
      TypeIdIndex = typeIdIndex;
      columnsMapping = columnMapping;
    }
  }
}