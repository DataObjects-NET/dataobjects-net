// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.02

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Internals
{
  internal class RecordSetReader : DomainBound
  {
    private readonly ICache<RecordSetHeader, RecordSetMapping> cache;

    public Record ReadSingleRow(IEnumerable<Tuple> source, RecordSetHeader header, Key key)
    {
      var item = source.FirstOrDefault();
      if (item==null)
        return null;

      if (key != null && key.HasExactType) {
        var typeMapping = GetMapping(header).Mappings[0].GetTypeMapping(key.Type.TypeId);
        var entityTuple = typeMapping.Transform.Apply(TupleTransformType.Tuple, item);
        return new Record(item, new Pair<Key, Tuple>(key, entityTuple));
      }
      return ParseRow(item, GetMapping(header));
    }

    public IEnumerable<Record> Read(IEnumerable<Tuple> source, RecordSetHeader header)
    {
      var mapping = GetMapping(header);
      foreach (var item in source)
        yield return ParseRow(item, mapping);
    }

    private Record ParseRow(Tuple tuple, RecordSetMapping mapping)
    {
      var count = mapping.Mappings.Count;

      if (count == 1)
        return new Record(tuple, ParseColumnGroup(tuple, mapping.Mappings[0]));

      var pairs = new List<Pair<Key, Tuple>>(count);
      foreach (var groupMapping in mapping.Mappings)
        pairs.Add(ParseColumnGroup(tuple, groupMapping));

      return new Record(tuple, pairs);
    }

    private Pair<Key, Tuple> ParseColumnGroup(Tuple tuple, ColumnGroupMapping groupMapping)
    {
      var rootType = groupMapping.Type;
      TypeReferenceAccuracy accuracy;
      int typeId = ExtractTypeId(rootType, tuple, groupMapping.TypeIdColumnIndex, out accuracy);
      var typeMapping = typeId==TypeInfo.NoTypeId ? null : groupMapping.GetTypeMapping(typeId);
      if (typeMapping==null)
        return new Pair<Key, Tuple>(null, null);

      bool canCache = accuracy==TypeReferenceAccuracy.ExactType;
      Key key;
      if (typeMapping.KeyTransform.Descriptor.Count <= WellKnown.MaxGenericKeyLength)
        key = KeyFactory.Create(Domain.Demand(), typeMapping.Type, tuple, accuracy, canCache, typeMapping.KeyIndexes);
      else {
        var keyTuple = typeMapping.KeyTransform.Apply(TupleTransformType.TransformedTuple, tuple);
        key = KeyFactory.Create(Domain.Demand(), typeMapping.Type, keyTuple, accuracy, canCache, null);
      }
      if (accuracy == TypeReferenceAccuracy.ExactType) {
        var entityTuple = typeMapping.Transform.Apply(TupleTransformType.Tuple, tuple);
        return new Pair<Key, Tuple>(key, entityTuple);
      }
      return new Pair<Key, Tuple>(key, null);
    }

    internal static int ExtractTypeId(TypeInfo type, Tuple tuple, int typeIdIndex, out TypeReferenceAccuracy accuracy)
    {
      accuracy = TypeReferenceAccuracy.Hierarchy;
      if (type.IsInterface)
        accuracy = TypeReferenceAccuracy.BaseType;

      if (typeIdIndex < 0) {
        if (type.IsLeaf)
          accuracy = TypeReferenceAccuracy.ExactType;
        return type.TypeId;
      }

      accuracy = TypeReferenceAccuracy.ExactType;
      TupleFieldState state;
      var value = tuple.GetValue<int>(typeIdIndex, out state);
      if (type.IsLeaf)
        return state.HasValue() ? type.TypeId : TypeInfo.NoTypeId;
      // Hack here: 0 (default) = TypeInfo.NoTypeId
      return value;
    }

    //TODO: Refactor this to support interface fetching and do not cache mapping by header!!!! Similar functionality is in MaterializationContext
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
        var type = group.TypeInfoRef.Resolve(model);
        var columnMapping = new List<Pair<FieldInfo, MappedColumn>>(group.Columns.Count);
        var keyMapping = new List<Pair<FieldInfo, MappedColumn>>(group.Keys.Count);

        foreach (int columnIndex in group.Columns) {
          var column = (MappedColumn)header.Columns[columnIndex];
          var columnInfo = column.ColumnInfoRef.Resolve(model);
          var field = columnInfo.Field;
          if (type.IsInterface && !field.IsDeclared)
            field = field.DeclaringType.Fields[field.Name];
          columnMapping.Add(new Pair<FieldInfo, MappedColumn>(field, column));
          if (columnInfo.Name == typeIdColumnName)
            typeIdColumnIndex = column.Index;
        }

        foreach (int columnIndex in group.Keys) {
          var column = (MappedColumn)header.Columns[columnIndex];
          var columnInfo = column.ColumnInfoRef.Resolve(model);
          var field = columnInfo.Field;
          if (type.IsInterface && !field.IsDeclared)
            field = field.DeclaringType.Fields[field.Name];
          keyMapping.Add(new Pair<FieldInfo, MappedColumn>(field, column));
        }

        var implementors = (type.IsInterface 
          ? type.GetImplementors(true)
          : type.GetDescendants(true)).ToList();
        if (!type.IsAbstract && !type.IsInterface)
          implementors.Add(type);
        var typeMappings = new IntDictionary<TypeMapping>(implementors.Count + 1);
        foreach (TypeInfo childType in implementors) {
          // Building typeMap
          var typeMap = Enumerable.Repeat(MapTransform.NoMapping, childType.Columns.Count).ToArray();
          foreach (var pair in columnMapping) {
            var childTypeField = type.IsInterface
              ? childType.FieldMap[pair.First]
              : childType.Fields[pair.First.Name];
            typeMap[childTypeField.MappingInfo.Offset] = pair.Second.Index;
          }

          // Building keyMap
          var keyMap = Enumerable.Repeat(MapTransform.NoMapping, type.KeyProviderInfo.Length).ToArray();
          foreach (var pair in keyMapping) {
            var childTypeField = type.IsInterface
              ? childType.FieldMap[pair.First]
              : childType.Fields[pair.First.Name];
            keyMap[childTypeField.MappingInfo.Offset] = pair.Second.Index;
          }
          var typeMapping = new TypeMapping(childType,
            new MapTransform(true, childType.KeyProviderInfo.TupleDescriptor, keyMap),
            new MapTransform(true, childType.TupleDescriptor, typeMap));
          typeMappings.Add(childType.TypeId, typeMapping);
        }
        var mapping =  new ColumnGroupMapping(type, typeIdColumnIndex, typeMappings);
        mappings.Add(mapping);
      }
      result = new RecordSetMapping(Domain, header, mappings);
      lock (cache) {
        cache.Add(result);
      }
      return result;
    }


    // Constructors

    internal RecordSetReader(Domain domain)
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