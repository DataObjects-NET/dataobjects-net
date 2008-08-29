// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.09

using System;
using System.Collections.Generic;
using Xtensive.Core;
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
      Session session = Session.Current;
      Domain domain = session.Domain;
      DomainModel model = domain.Model;
      foreach (Tuple tuple in source) {
        foreach (RecordColumnGroupMapping mapping in source.Header.ColumnGroupMappings) {
          TypeInfo type = null;
          Dictionary<ColumnInfo, int> columns = new Dictionary<ColumnInfo, int>(mapping.Columns.Count);
          foreach (int columnIndex in mapping.Columns) {
            RecordColumn rc = source.Header.Columns[columnIndex];
            ColumnInfo ci = rc.ColumnInfoRef.Resolve(model);
            columns.Add(ci, columnIndex);
            if (ci.Name == NameBuilder.TypeIdFieldName)
              type = model.Types[tuple.GetValue<int>(columnIndex)];
          }
          if (type == null)
            continue;
          List<int> map = new List<int>(type.Columns.Count);
          foreach (ColumnInfo column in type.Columns) {
            int index;
            if (columns.TryGetValue(column, out index))
              map.Add(index);
            else
              map.Add(-1);
          }
          MapTransform transform = new MapTransform(true, type.TupleDescriptor, map.ToArray());
          Tuple result = transform.Apply(TupleTransformType.TransformedTuple, tuple);
          Key key = domain.KeyManager.Get(type, result);
          session.DataCache.Update(key, result);
        }
      }
    }
  }
}