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
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;
using ColumnGroup=Xtensive.Storage.Rse.ColumnGroup;

namespace Xtensive.Storage
{
  public static class RecordSetExtensions
  {
    public static IEnumerable<T> ToEntities<T>(this RecordSet source) 
      where T : class, IEntity
    {
//      var result = new List<T>();
      foreach (var entity in ToEntities(source, typeof (T)))
        yield return entity as T;
//        result.Add(entity as T);
//      return result;
    }

    public static IEnumerable<Entity> ToEntities(this RecordSet source, Type type)
    {
      RecordSetHeaderParsingContext context = new RecordSetHeaderParsingContext(Session.Current, source.Header);
      RecordSetMapping mapping = GetRecordSetMapping(context);

//      var result = new List<Entity>();
      foreach (Tuple tuple in source) {
        Entity entity = null;
        foreach (ColumnGroupMapping columnGroupMapping in mapping.ColumnGroupMappings) {
          Key key = ProcessColumnGroup(context, columnGroupMapping, tuple);
          if (entity==null && type.IsAssignableFrom(key.Type.UnderlyingType))
            entity = key.Resolve();
        }
        yield return entity;
//        result.Add(entity);
      }
//      return result;
    }

    internal static void Process(this RecordSet source)
    {
      RecordSetHeaderParsingContext context = new RecordSetHeaderParsingContext(Session.Current, source.Header);
      RecordSetMapping mapping = GetRecordSetMapping(context);

      foreach (Tuple tuple in source)
        foreach (ColumnGroupMapping columnGroupMapping in mapping.ColumnGroupMappings)
          ProcessColumnGroup(context, columnGroupMapping, tuple);
    }

    private static Key ProcessColumnGroup(RecordSetHeaderParsingContext context, ColumnGroupMapping columnGroupMapping, Tuple tuple)
    {
      TypeMapping typeMapping = GetTypeMapping(context, columnGroupMapping, tuple);
      Tuple transformedTuple = typeMapping.Transform.Apply(TupleTransformType.TransformedTuple, tuple);
      Key key = context.Domain.KeyManager.Get(typeMapping.Type, transformedTuple);
      context.Session.DataCache.Update(key, transformedTuple);
      return key;
    }

    private static RecordSetMapping GetRecordSetMapping(RecordSetHeaderParsingContext context)
    {
      RecordSetMapping result = context.Domain.RecordSetMappings.GetValue(context.Header);
      if (result != null)
        return result;

      List<ColumnGroupMapping> mappings = new List<ColumnGroupMapping>();
      foreach (ColumnGroup group in context.Header.ColumnGroups) {
        ColumnGroupMapping mapping = GetColumnGroupMapping(context, group);
        if (mapping != null)
          mappings.Add(mapping);
      }
      result = new RecordSetMapping(context.Header, mappings);

      context.Domain.RecordSetMappings.SetValue(context.Header, result);
      return result;
    }

    private static ColumnGroupMapping GetColumnGroupMapping(RecordSetHeaderParsingContext context, ColumnGroup group)
    {
      int typeIdIndex = -1;
      Dictionary<ColumnInfo, Column> columnMapping = new Dictionary<ColumnInfo, Column>(group.Columns.Count);

      foreach (int columnIndex in group.Columns) {
        Column column = context.Header.Columns[columnIndex];
        ColumnInfo columnInfo = column.ColumnInfoRef.Resolve(context.Domain.Model);
        columnMapping[columnInfo] = column;
        if (columnInfo.Name==NameBuilder.TypeIdFieldName)
          typeIdIndex = column.Index;
      }

      if (typeIdIndex == -1)
        return null;

      return new ColumnGroupMapping(typeIdIndex, columnMapping);
    }

    private static TypeMapping GetTypeMapping(RecordSetHeaderParsingContext context, ColumnGroupMapping columnGroupMapping, Tuple tuple)
    {
      int typeId = tuple.GetValue<int>(columnGroupMapping.TypeIdIndex);
      TypeInfo type = context.Domain.Model.Types[typeId];
      TypeMapping typeMapping = columnGroupMapping.TypeMappings.GetValue(type);
      if (typeMapping==null) {
        typeMapping = BuildTypeMapping(columnGroupMapping, type);
        columnGroupMapping.TypeMappings.SetValue(type, typeMapping);
      }
      return typeMapping;
    }

    private static TypeMapping BuildTypeMapping(ColumnGroupMapping columnGroupMapping, TypeInfo type)
    {
      List<int> map = new List<int>(type.Columns.Count);
      foreach (ColumnInfo columnInfo in type.Columns) {
        Column column;
        if (columnGroupMapping.ColumnInfoMapping.TryGetValue(columnInfo, out column))
          map.Add(column.Index);
        else
          map.Add(MapTransform.NoMapping);
      }
      return new TypeMapping(type, new MapTransform(true, type.TupleDescriptor, map.ToArray()));
    }
  }
}
