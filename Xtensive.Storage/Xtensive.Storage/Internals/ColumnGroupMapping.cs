// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System.Collections.Generic;
using log4net.Repository.Hierarchy;
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

    public HierarchyInfo Hierarchy { get; private set; }

    public int TypeIdColumnIndex { get; private set; }

    public TypeMapping GetMapping(int typeId)
    {
      return typeMappings.GetValue(typeId,
        _typeId => {
          var type = typeId==0 ? Hierarchy.Root : Model.Types[typeId];

          // Building typeMap
          var columnCount = type.Columns.Count;
          var typeMap = new int[columnCount];
          for (int i = 0; i < columnCount; i++) {
            var columnInfo = type.Columns[i];
            MappedColumn column;
            if (columnsMapping.TryGetValue(columnInfo, out column))
              typeMap[i] = column.Index;
            else
              typeMap[i] = MapTransform.NoMapping;
          }

          // Building keyMap
          var columns = type.Hierarchy.KeyColumns;
          columnCount = columns.Count;
          var keyMap = new int[columnCount];
          for (int i = 0; i < columnCount; i++) {
            var columnInfo = columns[i];
            MappedColumn column;
            if (columnsMapping.TryGetValue(columnInfo, out column))
              keyMap[i] = column.Index;
            else
              keyMap[i] = MapTransform.NoMapping;
          }
          return new TypeMapping(type,
            new MapTransform(true, type.Hierarchy.KeyTupleDescriptor, keyMap), 
            new MapTransform(true, type.TupleDescriptor, typeMap));
        });
    }


    // Constructors

    public ColumnGroupMapping(DomainModel model, HierarchyInfo hierarchy, int typeIdColumnIndex, Dictionary<ColumnInfo, MappedColumn> columnMapping)
    {
      Model = model;
      Hierarchy = hierarchy;
      TypeIdColumnIndex = typeIdColumnIndex;
      columnsMapping = columnMapping;
    }
  }
}