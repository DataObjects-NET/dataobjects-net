// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System.Collections.Generic;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal sealed class ColumnGroupMapping
  {
    private readonly Dictionary<ColumnInfo, MappedColumn> columnsMapping;
    private ThreadSafeList<TypeMapping> typeMappings = 
      ThreadSafeList<TypeMapping>.Create(new object());
    
    public DomainModel Model { get; private set; }

    public int TypeIdColumnIndex { get; private set; }

    public TypeMapping GetMapping(int typeId)
    {
      return typeMappings.GetValue(typeId,
        _typeId => {
          var type = Model.Types[typeId];
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
        });
    }


    // Constructors

    public ColumnGroupMapping(DomainModel model, int typeIdColumnIndex, Dictionary<ColumnInfo, MappedColumn> columnMapping)
    {
      Model = model;
      TypeIdColumnIndex = typeIdColumnIndex;
      columnsMapping = columnMapping;
    }
  }
}