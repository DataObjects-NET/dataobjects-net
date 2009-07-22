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
        var rootType = groupMapping.Hierarchy.Root;
        int? typeId = ExtractTypeId(rootType, tuple, groupMapping.TypeIdColumnIndex);
        var typeMapping = typeId.HasValue ? groupMapping.GetMapping(typeId.GetValueOrDefault()) : null;
        if (typeMapping != null) {
          Key key;
          bool exactType = typeId.GetValueOrDefault() != TypeInfo.NoTypeId;
          var entityType = exactType ? typeMapping.Type : rootType;
          if (typeMapping.KeyTransform.Descriptor.Count <= Key.MaxGenericKeyLength)
            key = Key.Create(session.Domain, entityType, tuple, typeMapping.KeyIndexes, exactType, exactType);
          else {
            var keyTuple = typeMapping.KeyTransform.Apply(TupleTransformType.TransformedTuple, tuple);
            key = Key.Create(session.Domain, entityType, keyTuple, null, exactType, exactType);
          }
          if (exactType) {
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

    internal static int? ExtractTypeId(TypeInfo typeInfo, Tuple tuple, int typeIdIndex)
    {
      if (typeIdIndex <= 0)
        return TypeInfo.NoTypeId;
      if (typeInfo.IsLeaf)
        return tuple.HasValue(typeIdIndex) ? (int?)typeInfo.TypeId : null;
      else {
        int typeId = tuple.GetValueOrDefault<int>(typeIdIndex);
        return typeId==0 ? null : (int?) typeId; // Hack here: 0 = TypeInfo.NoTypeId
      }
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
      var typeIdColumnName = Domain.NameBuilder.TypeIdColumnName;

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


    // Constructors

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