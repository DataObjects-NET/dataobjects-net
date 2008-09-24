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
using ColumnGroup=Xtensive.Storage.Rse.ColumnGroup;

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
      var mapping = GetRecordSetMapping(context);

      foreach (Tuple tuple in source) {
        Entity entity = null;
        for (int i = 0; i < mapping.ColumnGroupMappings.Length; i++) {
          var columnGroupMapping = mapping.ColumnGroupMappings[i];
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
      RecordSetMapping mapping = GetRecordSetMapping(context);
      int recordCount = 0;
      foreach (Tuple tuple in source) {
        recordCount++;
        for (int i = 0; i < mapping.ColumnGroupMappings.Length; i++) {
          ColumnGroupMapping columnGroupMapping = mapping.ColumnGroupMappings[i];
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

    private static RecordSetMapping GetRecordSetMapping(RecordSetHeaderParseContext context)
    {
      RecordSetMapping result;
      if (context.Domain.RecordSetMappings.TryGetValue(context.Header, out result))
        return result;

      var mappings = new List<ColumnGroupMapping>();
      foreach (ColumnGroup group in context.Header.ColumnGroups) {
        ColumnGroupMapping mapping = GetColumnGroupMapping(context, group);
        if (mapping != null)
          mappings.Add(mapping);
      }
      result = new RecordSetMapping(context.Header, mappings.ToArray());

      context.Domain.RecordSetMappings.SetValue(context.Header, result);
      return result;
    }

    private static ColumnGroupMapping GetColumnGroupMapping(RecordSetHeaderParseContext context, ColumnGroup group)
    {
      int typeIdIndex = -1;
      var columnMapping = new Dictionary<ColumnInfo, MappedColumn>(group.Columns.Count);

      foreach (int columnIndex in group.Columns) {
        var column = (MappedColumn) context.Header.Columns[columnIndex];
        ColumnInfo columnInfo = column.ColumnInfoRef.Resolve(context.Domain.Model);
        columnMapping[columnInfo] = column;
        if (columnInfo.Name==Session.Current.Domain.NameBuilder.TypeIdColumnName)
          typeIdIndex = column.Index;
      }

      if (typeIdIndex == -1)
        return null;

      return new ColumnGroupMapping(typeIdIndex, columnMapping);
    }

    private static TypeMapping GetTypeMapping(RecordSetHeaderParseContext context, ColumnGroupMapping columnGroupMapping, Tuple tuple)
    {
      int typeId = tuple.GetValueOrDefault<int>(columnGroupMapping.TypeIdIndex);
      TypeInfo type = context.Domain.Model.Types[typeId];
      TypeMapping typeMapping;
      if (columnGroupMapping.TypeMappings.TryGetValue(type, out typeMapping))
        return typeMapping;
      typeMapping = BuildTypeMapping(columnGroupMapping, type);
      columnGroupMapping.TypeMappings.SetValue(type, typeMapping);
      return typeMapping;
    }

    private static TypeMapping BuildTypeMapping(ColumnGroupMapping columnGroupMapping, TypeInfo type)
    {
      var map = new List<int>(type.Columns.Count);
      foreach (ColumnInfo columnInfo in type.Columns) {
        MappedColumn column;
        if (columnGroupMapping.ColumnInfoMapping.TryGetValue(columnInfo, out column))
          map.Add(column.Index);
        else
          map.Add(MapTransform.NoMapping);
      }
      return new TypeMapping(type, new MapTransform(true, type.TupleDescriptor, map.ToArray()));
    }

    #endregion
  }
}
