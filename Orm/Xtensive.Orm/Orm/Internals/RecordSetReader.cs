// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.02

using System.Collections.Generic;
using System.Linq;
using Xtensive.Caching;
using Xtensive.Core;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
using Xtensive.Tuples;
using Xtensive.Tuples.Transform;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals
{
  internal class RecordSetReader : DomainBound
  {
    private class RecordPartMapping
    {
      public int TypeIdColumnIndex { get; private set; }
      public Pair<int>[] Columns { get; private set; }
      public TypeInfo ApproximateType { get; private set; }

      public RecordPartMapping(int typeIdColumnIndex, Pair<int>[] columns, TypeInfo approximateType)
      {
        TypeIdColumnIndex = typeIdColumnIndex;
        Columns = columns;
        ApproximateType = approximateType;
      }
    }

    private class CacheItem
    {
      public RecordSetHeader Header { get; private set; }
      public RecordPartMapping[] Mappings { get; private set; }

      public CacheItem(RecordSetHeader header, RecordPartMapping[] mappings)
      {
        Header = header;
        Mappings = mappings;
      }
    }

    private readonly ICache<RecordSetHeader, CacheItem> cache;
    private readonly object _lock = new object();
    
    public IEnumerable<Record> Read(IEnumerable<Tuple> source, RecordSetHeader header, Session session)
    {
      CacheItem cacheItem;
      var recordPartCount = header.ColumnGroups.Count;
      var context = new MaterializationContext(session, recordPartCount);
      lock (_lock) {
        if (!cache.TryGetItem(header, false, out cacheItem)) {
          var typeIdColumnName = Domain.Handlers.NameBuilder.TypeIdColumnName;
          var model = context.Model;
          var mappings = new RecordPartMapping[recordPartCount];
          for (int i = 0; i < recordPartCount; i++) {
            var columnGroup = header.ColumnGroups[i];
            var approximateType = columnGroup.TypeInfoRef.Resolve(model);
            var columnMapping = new List<Pair<int>>();
            var typeIdColumnIndex = -1;
            foreach (var columnIndex in columnGroup.Columns) {
              var column = (MappedColumn) header.Columns[columnIndex];
              var columnInfo = column.ColumnInfoRef.Resolve(model);
              FieldInfo fieldInfo;
              if (!approximateType.Fields.TryGetValue(columnInfo.Field.Name, out fieldInfo))
                continue;
              var targetColumnIndex = fieldInfo.MappingInfo.Offset;
              if (columnInfo.Name==typeIdColumnName)
                typeIdColumnIndex = column.Index;
              columnMapping.Add(new Pair<int>(targetColumnIndex, columnIndex));
            }
            mappings[i] = new RecordPartMapping(typeIdColumnIndex, columnMapping.ToArray(), approximateType);
          }
          cacheItem = new CacheItem(header, mappings);
          cache.Add(cacheItem);
        }
      }
      return source.Select(tuple => ParseRow(tuple, context, cacheItem.Mappings));
    }

    private Record ParseRow(Tuple tuple, MaterializationContext context, RecordPartMapping[] recordPartMappings)
    {
      var count = recordPartMappings.Length;

      if (count==1)
        return new Record(tuple, ParseColumnGroup(tuple, context, 0, recordPartMappings[0]));

      var pairs = new List<Pair<Key, Tuple>>(
        recordPartMappings
          .Select((recordPartMapping, i) => ParseColumnGroup(tuple, context, i, recordPartMapping)));
      return new Record(tuple, pairs);
    }

    private Pair<Key, Tuple> ParseColumnGroup(Tuple tuple, MaterializationContext context, int groupIndex, RecordPartMapping mapping)
    {
      TypeReferenceAccuracy accuracy;
      int typeId = ExtractTypeId(mapping.ApproximateType, context.TypeIdRegistry, tuple, mapping.TypeIdColumnIndex, out accuracy);
      var typeMapping = typeId==TypeInfo.NoTypeId ? null : context.GetTypeMapping(groupIndex, mapping.ApproximateType, typeId, mapping.Columns);
      if (typeMapping==null)
        return new Pair<Key, Tuple>(null, null);

      bool canCache = accuracy==TypeReferenceAccuracy.ExactType;
      Key key;
      if (typeMapping.KeyTransform.Descriptor.Count <= WellKnown.MaxGenericKeyLength)
        key = KeyFactory.Materialize(Domain, context.Session.StorageNodeId, typeMapping.Type, tuple, accuracy, canCache, typeMapping.KeyIndexes);
      else {
        var keyTuple = typeMapping.KeyTransform.Apply(TupleTransformType.TransformedTuple, tuple);
        key = KeyFactory.Materialize(Domain, context.Session.StorageNodeId, typeMapping.Type, keyTuple, accuracy, canCache, null);
      }
      if (accuracy == TypeReferenceAccuracy.ExactType) {
        var entityTuple = typeMapping.Transform.Apply(TupleTransformType.Tuple, tuple);
        return new Pair<Key, Tuple>(key, entityTuple);
      }
      return new Pair<Key, Tuple>(key, null);
    }

    public static int ExtractTypeId(TypeInfo type, TypeIdRegistry typeIdRegistry, Tuple tuple, int typeIdIndex, out TypeReferenceAccuracy accuracy)
    {
      accuracy = TypeReferenceAccuracy.Hierarchy;
      if (type.IsInterface)
        accuracy = TypeReferenceAccuracy.BaseType;

      if (typeIdIndex < 0) {
        if (type.IsLeaf)
          accuracy = TypeReferenceAccuracy.ExactType;
        return typeIdRegistry.GetTypeId(type);
      }

      accuracy = TypeReferenceAccuracy.ExactType;
      TupleFieldState state;
      var value = tuple.GetValue<int>(typeIdIndex, out state);
      if (type.IsLeaf)
        return state.HasValue() ? typeIdRegistry.GetTypeId(type) : TypeInfo.NoTypeId;
      // Hack here: 0 (default) = TypeInfo.NoTypeId
      return value;
    }


    // Constructors

    internal RecordSetReader(Domain domain)
      : base(domain)
    {
      cache = new LruCache<RecordSetHeader, CacheItem>(domain.Configuration.RecordSetMappingCacheSize,
          m => m.Header, new WeakestCache<RecordSetHeader, CacheItem>(false, false, m => m.Header));
    }
  }
}