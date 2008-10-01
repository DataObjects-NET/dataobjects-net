// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.09

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  /// <summary>
  /// <see cref="RecordSet"/> related extension methods.
  /// </summary>
  public static class RecordSetExtensions
  {
    /// <summary>
    /// Converts the <see cref="RecordSet"/> items to <see cref="Entity"/> instances.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Entity"/> instances to get.</typeparam>
    /// <param name="source">The <see cref="RecordSet"/> to process.</param>
    /// <returns>The sequence of <see cref="Entity"/> instances.</returns>
    public static IEnumerable<T> ToEntities<T>(this RecordSet source) 
      where T : class, IEntity
    {
      foreach (var entity in ToEntities(source, typeof (T)))
        yield return entity as T;
    }

    /// <summary>
    /// Converts the <see cref="RecordSet"/> items to <see cref="Entity"/> instances.
    /// </summary>
    /// <param name="source">The <see cref="RecordSet"/> to process.</param>
    /// <param name="type">The type of <see cref="Entity"/> instances to get.</param>
    /// <returns>The sequence of <see cref="Entity"/> instances.</returns>
    public static IEnumerable<Entity> ToEntities(this RecordSet source, Type type)
    {
      var context = new RecordSetHeaderParseContext(Session.Current, source.Header);
      var recordSetMapping = context.Domain.GetMapping(source.Header);
      var groupMappings    = recordSetMapping.Mappings;
      var typeMappings     = new TypeMapping[groupMappings.Length];
      foreach (Tuple tuple in source) {
        Entity entity = null;
        for (int i = 0; i < groupMappings.Length; i++) {
          Key key = ProcessColumnGroup(context, groupMappings[i], ref typeMappings[i], tuple);
          if (entity==null && type.IsAssignableFrom(key.Type.UnderlyingType))
            entity = key.Resolve();
        }
        yield return entity;
      }
    }

    public static int Parse(this RecordSet source)
    {
      var context = new RecordSetHeaderParseContext(Session.Current, source.Header);
      var recordSetMapping = context.Domain.GetMapping(source.Header);
      var groupMappings    = recordSetMapping.Mappings;
      var typeMappings     = new TypeMapping[groupMappings.Length];
      int recordCount = 0;
      foreach (Tuple tuple in source) {
        recordCount++;
        for (int i = 0; i < groupMappings.Length; i++)
          ProcessColumnGroup(context, groupMappings[i], ref typeMappings[i], tuple);
      }
      return recordCount;
    }

    private static Key ProcessColumnGroup(RecordSetHeaderParseContext context, ColumnGroupMapping columnGroupMapping, ref TypeMapping lastTypeMapping, Tuple record)
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
      Key key = context.KeyManager.Get(typeMapping.Type, entityTuple);
      context.Cache.Update(key, entityTuple, context.Session.Transaction);
      return key;
    }
  }
}
