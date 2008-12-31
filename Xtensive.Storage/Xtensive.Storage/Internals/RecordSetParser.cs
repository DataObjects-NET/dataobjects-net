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

    #region Deprecated

    /// <summary>
    /// Converts the <see cref="RecordSet"/> items to <see cref="Entity"/> instances.
    /// </summary>
    /// <param name="source">The <see cref="RecordSet"/> to process.</param>
    /// <param name="type">The type of <see cref="Entity"/> instances to get.</param>
    /// <returns>The sequence of <see cref="Entity"/> instances.</returns>
    public IEnumerable<Entity> ToEntities(RecordSet source, Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");

      var context = new RecordSetParserContext(source);
      var recordSetMapping  = GetMapping(source.Header);
      var groupMappings     = recordSetMapping.Mappings;
      var groupMappingCount = groupMappings.Length;
      if (groupMappingCount==0)
        yield break;
      var typeMappings = new TypeMapping[groupMappingCount];

      foreach (var tuple in source) {
        Entity entity = null;
        for (int i = 0; i < groupMappingCount; i++) {
          Key key = Parse(context, tuple, groupMappings[i], ref typeMappings[i]);
          if (entity==null && key!=null && type.IsAssignableFrom(key.Type.UnderlyingType))
            entity = key.Resolve();
        }
        yield return entity;
      }
    }

    public Key ParseFirstFast(RecordSet source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");

      Tuple record = source.FirstOrDefault();
      if (record == null)
        return null;

      var context = new RecordSetParserContext(source);
      var recordSetMapping = GetMapping(source.Header);
      var groupMappings    = recordSetMapping.Mappings;
      var typeMappings     = new TypeMapping[groupMappings.Length];
      return Parse(context, record, groupMappings[0], ref typeMappings[0]);
    }

    private static Key Parse(RecordSetParserContext context, Tuple record, ColumnGroupMapping columnGroupMapping, ref TypeMapping lastTypeMapping)
    {
      int typeIdColumnIndex = columnGroupMapping.TypeIdColumnIndex;
      int typeId = TypeInfo.NoTypeId;
      if (typeIdColumnIndex>=0)
        typeId = (int) record.GetValueOrDefault(typeIdColumnIndex);
      TypeMapping typeMapping;
      if (typeId!=TypeInfo.NoTypeId && lastTypeMapping!=null && typeId==lastTypeMapping.TypeId)
        typeMapping = lastTypeMapping;
      else {
        typeMapping = columnGroupMapping.GetMapping(typeId);
        if (typeMapping==null)
          return null;
        lastTypeMapping = typeMapping;
      }
      
      var keyTuple = typeMapping.KeyTransform.Apply(TupleTransformType.TransformedTuple, record);
      if (typeId==TypeInfo.NoTypeId) // No TypeId in this column group
        return Key.Create(context.Domain, columnGroupMapping.Hierarchy.Root, keyTuple, false, false);

      var key = Key.Create(context.Domain, typeMapping.Type, keyTuple, true, true);
      var entityTuple = typeMapping.Transform.Apply(TupleTransformType.Tuple, record);
      context.Session.UpdateEntityState(key, entityTuple);
      return key;
    }

    #endregion

    public IEnumerable<Record> Parse(RecordSet source)
    {
      var recordSetMapping = GetMapping(source.Header);

      throw new NotImplementedException();
    }

    private RecordSetMapping GetMapping(RecordSetHeader header)
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
          var columns = type.Hierarchy.KeyColumns;
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
              new MapTransform(true, type.Hierarchy.KeyTupleDescriptor, keyMap),
              new MapTransform(true, type.TupleDescriptor, typeMap));
            typeMappings.Add(type.TypeId, typeMapping);
          }
        }
        var mapping =  new ColumnGroupMapping(hierarchy, typeIdColumnIndex, typeMappings);
        mappings.Add(mapping);
      }
      result = new RecordSetMapping(header, mappings);
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