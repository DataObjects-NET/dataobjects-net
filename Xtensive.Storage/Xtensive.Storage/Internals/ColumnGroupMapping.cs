// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  [DebuggerDisplay("Hierarchy = {Hierarchy.Name}, TypeIdColumnIndex = {TypeIdColumnIndex}")]
  internal sealed class ColumnGroupMapping
  {
    private readonly Dictionary<int, TypeMapping> typeMappings;

    public HierarchyInfo Hierarchy { get; private set; }

    public int TypeIdColumnIndex { get; private set; }

    public TypeMapping GetMapping(int typeId)
    {
      typeId = typeId==TypeInfo.NoTypeId ? Hierarchy.Root.TypeId : typeId;
      TypeMapping result;
      if (typeMappings.TryGetValue(typeId, out result))
        return result;
      return null;
    }

//    public TypeMapping GetMapping(int typeId)
//    {
//      return typeMappings.GetValue(typeId,
//        _typeId => {
//          var type = typeId==TypeInfo.NoTypeId ? Hierarchy.Root : model.Types[typeId];
//
//          // Building typeMap
//          var columnCount = type.Columns.Count;
//          var typeMap = new int[columnCount];
//          for (int i = 0; i < columnCount; i++) {
//            var columnInfo = type.Columns[i];
//            MappedColumn column;
//            if (columnsMapping.TryGetValue(columnInfo, out column))
//              typeMap[i] = column.Index;
//            else
//              typeMap[i] = MapTransform.NoMapping;
//          }
//
//          // Building keyMap
//          var columns = type.Hierarchy.KeyColumns;
//          columnCount = columns.Count;
//          var keyMap = new int[columnCount];
//          bool hasKey = false;
//          for (int i = 0; i < columnCount; i++) {
//            var columnInfo = columns[i];
//            MappedColumn column;
//            if (columnsMapping.TryGetValue(columnInfo, out column)) {
//              keyMap[i] = column.Index;
//              hasKey = true;
//            }
//            else
//              keyMap[i] = MapTransform.NoMapping;
//          }
//          if (!hasKey)
//            return null;
//          else
//            return new TypeMapping(type,
//              new MapTransform(true, type.Hierarchy.KeyTupleDescriptor, keyMap), 
//              new MapTransform(true, type.TupleDescriptor, typeMap));
//        });
//    }


    // Constructors

    public ColumnGroupMapping(HierarchyInfo hierarchy, int typeIdColumnIndex, Dictionary<int, TypeMapping> typeMappings)
    {
      this.typeMappings = typeMappings;
      Hierarchy = hierarchy;
      TypeIdColumnIndex = typeIdColumnIndex;
    }
  }
}