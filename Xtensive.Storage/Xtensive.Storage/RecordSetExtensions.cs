// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.09

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
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
      if (scope.Session == null)
        throw new InvalidOperationException();

      TypeInfo type = scope.Session.HandlerAccessor.Domain.Model.Types[entityType];
      var keyColumns = type.Indexes.PrimaryIndex.KeyColumns;

      object[] keyValue = new object[keyColumns.Count];
      int[] columnsMap = new int[keyColumns.Count];
      for (int j = 0; j < columnsMap.Length; j++)
        columnsMap[j] = source.Map(keyColumns[j].Key.Name);
      foreach (Tuple tuple in source) {
        for (int i = 0; i < keyValue.Length; i++)
          keyValue[i] = tuple.GetValue(columnsMap[i]);
        Key key = scope.Session.HandlerAccessor.Domain.KeyManager.Build(entityType, keyValue);
        yield return key.Resolve();
      }
    }
  }
}