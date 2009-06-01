// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.02

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal class RecordSetParser : DomainBound
  {
    private readonly ICache<RecordSetHeader, RecordSetMapping> cache;

    public IEnumerable<Record> Parse(RecordSet source)
    {
      var session = Session.Current;
      var recordSetMapping = GetMapping(source.Header);
      foreach (Tuple tuple in source)
        yield return ParseSingleRow(tuple, recordSetMapping, session);
    }

    public Record ParseFirst(RecordSet source)
    {
      Tuple tuple = source.FirstOrDefault();
      if (tuple == null)
        return null;
      var session = Session.Current;
      var recordSetMapping = GetMapping(source.Header);
      return ParseSingleRow(tuple, recordSetMapping, session);
    }

    private static Record ParseSingleRow(Tuple tuple, RecordSetMapping mapping, Session session)
    {
      var keyList = new List<Key>(mapping.Mappings.Count);
      foreach (var groupMapping in mapping.Mappings) {
        int typeIdColumnIndex = groupMapping.TypeIdColumnIndex;
        int typeId = TypeInfo.NoTypeId;
        if (typeIdColumnIndex >= 0 && tuple.HasValue(typeIdColumnIndex))
          typeId = tuple.GetValueOrDefault<int>(typeIdColumnIndex);
        var typeMapping = groupMapping.GetMapping(typeId);
        if (typeMapping != null) {
          var keyTuple = typeMapping.KeyTransform.Apply(TupleTransformType.TransformedTuple, tuple);
          Key key;
          if (typeId == TypeInfo.NoTypeId) // No TypeId in this column group
          {
            key = Key.Create(mapping.CurrentDomain, groupMapping.Hierarchy.Root, keyTuple, false, false);
          }
          else {
            key = Key.Create(mapping.CurrentDomain, typeMapping.Type, keyTuple, true, true);
            var entityTuple = typeMapping.Transform.Apply(TupleTransformType.Tuple, tuple);
            session.UpdateEntityState(key, entityTuple);
          }
          keyList.Add(key);
        }
        else
          keyList.Add(null);
      }
      return new Record(tuple, keyList);
    }

    internal RecordSetMapping GetMapping(RecordSetHeader header)
    {
      RecordSetMapping result;
      lock (cache) {
        result = cache[header, true];
        if (result != null)
          return result;
      }
      var mappings = new List<ColumnGroupMapping>();
      var typeIdColumnName = Domain.NameBuilder.TypeIdColumn;

      foreach (var group in header.ColumnGroups) {
        var model = Domain.Model;
        int typeIdColumnIndex = -1;
        var columnMapping = new Dictionary<ColumnInfo, MappedColumn>(group.Columns.Count);
        var hierarchy = group.HierarchyInfoRef.Resolve(model);

        foreach (int columnIndex in group.Columns) {
          var column = (MappedColumn)header.Columns[columnIndex];
          ColumnInfo columnInfo = column.ColumnInfoRef.Resolve(model);
          columnMapping[columnInfo] = column;
          if (columnInfo.Name == typeIdColumnName)
            typeIdColumnIndex = column.Index;
        }

        var typeMappings = new Dictionary<int, TypeMapping>(hierarchy.Types.Count + 1);
        foreach (TypeInfo type in hierarchy.Types) {
          // Building typeMap
          var columnCount = type.Columns.Count;
          var typeMap = new int[columnCount];
          for (int i = 0; i < columnCount; i++) {
            var columnInfo = type.Columns[i];
            MappedColumn column;
            if (columnMapping.TryGetValue(columnInfo, out column))
              typeMap[i] = column.Index;
            else
              typeMap[i] = MapTransform.NoMapping;
          }
  
          // Building keyMap
          var columns = type.Hierarchy.KeyInfo.Columns;
          columnCount = columns.Count;
          var keyMap = new int[columnCount];
          bool hasKey = false;
          for (int i = 0; i < columnCount; i++) {
            var columnInfo = columns[i];
            MappedColumn column;
            if (columnMapping.TryGetValue(columnInfo, out column)) {
              keyMap[i] = column.Index;
              hasKey = true;
            }
            else
              keyMap[i] = MapTransform.NoMapping;
          }
          if (hasKey) {
            var typeMapping = new TypeMapping(type,
              new MapTransform(true, type.Hierarchy.KeyInfo.TupleDescriptor, keyMap),
              new MapTransform(true, type.TupleDescriptor, typeMap));
            typeMappings.Add(type.TypeId, typeMapping);
          }
        }
        var mapping =  new ColumnGroupMapping(hierarchy, typeIdColumnIndex, typeMappings);
        mappings.Add(mapping);
      }
      result = new RecordSetMapping(Domain, header, mappings);
      lock (cache) {
        cache.Add(result);
      }
      return result;
    }


    // Constructor

    internal RecordSetParser(Domain domain)
      : base(domain)
    {
      cache = 
        new LruCache<RecordSetHeader, RecordSetMapping>(
          domain.Configuration.RecordSetMappingCacheSize, 
          m => m.Header,
          new WeakestCache<RecordSetHeader, RecordSetMapping>(false, false, 
            m => m.Header));
    }
  }
}