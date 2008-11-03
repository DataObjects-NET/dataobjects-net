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
      var recordSetMapping = GetMapping(source.Header);
      var groupMappings    = recordSetMapping.Mappings;
      var typeMappings     = new TypeMapping[groupMappings.Length];
      foreach (Tuple tuple in source) {
        Entity entity = null;
        for (int i = 0; i < groupMappings.Length; i++) {
          Key key = Parse(context, tuple, groupMappings[i], ref typeMappings[i]);
          if (entity==null && type.IsAssignableFrom(key.Type.UnderlyingType))
            entity = key.Resolve();
        }
        yield return entity;
      }
    }

    public int Parse(RecordSet source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");

      var context = new RecordSetParserContext(source);
      var recordSetMapping = GetMapping(source.Header);
      var groupMappings    = recordSetMapping.Mappings;
      var typeMappings     = new TypeMapping[groupMappings.Length];
      int recordCount = 0;

      foreach (Tuple tuple in source) {
        recordCount++;
        for (int i = 0; i < groupMappings.Length; i++)
          Parse(context, tuple, groupMappings[i], ref typeMappings[i]);
      }
      return recordCount;
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
      int typeId = (int) record.GetValueOrDefault(columnGroupMapping.TypeIdColumnIndex);
      TypeMapping typeMapping;
      if (lastTypeMapping!=null && typeId==lastTypeMapping.TypeId)
        typeMapping = lastTypeMapping;
      else {
        typeMapping = columnGroupMapping.GetMapping(typeId);
        lastTypeMapping = typeMapping;
      }
      Tuple entityTuple = typeMapping.Transform.Apply(TupleTransformType.TransformedTuple, record);
      Key key = new Key(typeMapping.Type, entityTuple);
      context.Cache.Add(key, entityTuple);
      var keyCache = context.KeyCache;
      lock (keyCache) {
        key = keyCache.Add(key, false);
      }
      return key;
    }

    private RecordSetMapping GetMapping(RecordSetHeader header)
    {
      RecordSetMapping result;
      lock (cache) {
        result = cache[header, true];
        if (result!=null)
          return result;
      }
      var mappings = new List<ColumnGroupMapping>();
      foreach (var group in header.ColumnGroups) {
        var mapping = BuildColumnGroupMapping(header, group);
        if (mapping!=null)
          mappings.Add(mapping);
      }
      result = new RecordSetMapping(header, mappings);
      lock (cache) {
        cache.Add(result);
      }
      return result;
    }

    private ColumnGroupMapping BuildColumnGroupMapping(RecordSetHeader header, ColumnGroup group)
    {
      int typeIdColumnIndex = -1;
      var typeIdColumnName = Domain.NameBuilder.TypeIdColumnName;
      var columnMapping = new Dictionary<ColumnInfo, MappedColumn>(group.Columns.Count);

      foreach (int columnIndex in group.Columns) {
        var column = (MappedColumn)header.Columns[columnIndex];
        ColumnInfo columnInfo = column.ColumnInfoRef.Resolve(Domain.Model);
        columnMapping[columnInfo] = column;
        if (columnInfo.Name == typeIdColumnName)
          typeIdColumnIndex = column.Index;
      }

      if (typeIdColumnIndex == -1)
        return null;

      return new ColumnGroupMapping(Domain.Model, typeIdColumnIndex, columnMapping);
    }


    // Constructor

    internal RecordSetParser(Domain domain)
      : base(domain)
    {
      cache = 
        new LruCache<RecordSetHeader, RecordSetMapping>(domain.Configuration.RecordSetMappingCacheSize, 
          m => m.Header,
          new WeakestCache<RecordSetHeader, RecordSetMapping>(false, false, m => m.Header));
    }
  }
}