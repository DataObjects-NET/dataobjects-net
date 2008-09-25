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
using Xtensive.Storage.Model;
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
      var mapping = context.Domain.GetMapping(source.Header);
      foreach (Tuple tuple in source) {
        Entity entity = null;
        for (int i = 0; i < mapping.GroupMappings.Count; i++) {
          var columnGroupMapping = mapping.GroupMappings[i];
          Key key = ProcessColumnGroup(context, columnGroupMapping, tuple);
          if (entity==null && type.IsAssignableFrom(key.Type.UnderlyingType))
            entity = key.Resolve();
        }
        yield return entity;
      }
    }

    public static int Parse(this RecordSet source)
    {
      var context = new RecordSetHeaderParseContext(Session.Current, source.Header);
      var mapping = context.Domain.GetMapping(source.Header);
      int recordCount = 0;
      foreach (Tuple tuple in source) {
        recordCount++;
        for (int i = 0; i < mapping.GroupMappings.Count; i++) {
          ColumnGroupMapping columnGroupMapping = mapping.GroupMappings[i];
          ProcessColumnGroup(context, columnGroupMapping, tuple);
        }
      }
      return recordCount;
    }

    #region Private \ internal methods

    private static Key ProcessColumnGroup(RecordSetHeaderParseContext context, ColumnGroupMapping columnGroupMapping, Tuple tuple)
    {
      TypeMapping typeMapping = GetTypeMapping(context, columnGroupMapping, tuple);
      Tuple transformedTuple = typeMapping.Transform.Apply(TupleTransformType.TransformedTuple, tuple);
      Key key = context.Domain.KeyManager.Get(typeMapping.Type, transformedTuple);
      context.Session.Cache.Update(key, transformedTuple, context.Session.Transaction);
      return key;
    }

    private static TypeMapping GetTypeMapping(RecordSetHeaderParseContext context, ColumnGroupMapping columnGroupMapping, Tuple tuple)
    {
      int typeId = tuple.GetValueOrDefault<int>(columnGroupMapping.TypeIdIndex);
      TypeInfo type = context.Domain.Model.Types[typeId];
      return columnGroupMapping.GetTypeMapping(type);
    }
    #endregion
  }
}
