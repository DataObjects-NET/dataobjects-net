// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.09

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  public static class RecordSetExtensions
  {
    public static IEnumerable<T> AsEntities<T>(this RecordSet source) 
      where T : Entity
    {
      foreach (Entity entity in AsEntities(source, typeof (T)))
        yield return entity as T;
    }

    public static IEnumerable<Entity> AsEntities(this RecordSet source, Type entityType) 
    {
      SessionScope scope = SessionScope.Current;
      if (scope == null)
        throw new InvalidOperationException();
      Session session = scope.Session;
      if (session == null)
        throw new InvalidOperationException();

      TypeInfo type = session.Handlers.Domain.Model.Types[entityType];
      var keyColumns = type.Indexes.PrimaryIndex.KeyColumns;

      Tuple t = Tuple.Create(type.Hierarchy.TupleDescriptor);
      int[] columnsMap = new int[keyColumns.Count];
      for (int j = 0; j < columnsMap.Length; j++)
        columnsMap[j] = source.IndexOf(keyColumns[j].Key.Name);
      foreach (Tuple tuple in source) {
        for (int i = 0; i < t.Count; i++)
          t.SetValue(i, tuple.GetValue(columnsMap[i]));
        Key key = Key.Get(entityType, t);
        session.DataCache.Update(key, tuple);
        yield return key.Resolve();
      }
    }

    public static void Parse(this RecordSet source)
    {
      Domain domain = Session.Current.Domain;
      DomainModel model = domain.Model;
      foreach (Tuple tuple in source) {
        foreach (RecordColumnGroupMapping mapping in source.Header.ColumnGroupMappings) {
          Dictionary<RecordColumn, ColumnInfo> columns = new Dictionary<RecordColumn, ColumnInfo>();
          RecordColumn typeIdColumn = null;
          foreach (int i in mapping.Columns) {
            RecordColumn c = source.Header.Columns[i];
            ColumnInfo ci = c.ColumnInfoRef.Resolve(model);
            columns[c] = ci;
            if (ci.Name == NameBuilder.TypeIdFieldName)
              typeIdColumn = c;
          }
          if (typeIdColumn == null)
            continue;
          TypeInfo type = model.Types[tuple.GetValue<int>(typeIdColumn.Index)];
          List<int> map = new List<int>(columns.Count);
          foreach (var pair in columns) {
            if (type.Columns.Contains(pair.Value.Name))
              map.Add(pair.Key.Index);
          }
          MapTransform transform = new MapTransform(true, type.TupleDescriptor, map.ToArray());
          Tuple transformedTuple = transform.Apply(TupleTransformType.TransformedTuple, tuple);
          Key key = domain.KeyManager.Get(type, transformedTuple);
          Session.Current.DataCache.Update(key, transformedTuple);
        }
      }
    }
  }
}