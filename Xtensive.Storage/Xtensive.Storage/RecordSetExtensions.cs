// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.09

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using System.Linq;

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
      Session session = Session.Demand();
      var parser = session.Domain.RecordSetReader;
      int keyIndex = -1;
      foreach (var record in parser.Read(source)) {
        Key key;
        if (keyIndex == -1)
          for (int i = 0; i < record.Count; i++) {
            key = record.GetKey(i);
            if (key != null && type.IsAssignableFrom(key.Type.UnderlyingType)) {
              keyIndex = i;
              break;
            }
          }
        key = record.GetKey(keyIndex);
        var tuple = record.GetTuple(keyIndex);
        if (key==null || tuple==null)
          yield return null;
        else
          yield return session.UpdateEntityState(key, tuple).Entity;
      }
    }

    public static IEnumerable<Record> Read(this RecordSet source)
    {
      Domain domain = Domain.Demand();
      return domain.RecordSetReader.Read(source);
    }

    public static Record ReadSingleRow(this RecordSet source)
    {
      Domain domain = Domain.Demand();
      return domain.RecordSetReader.ReadSingleRow(source, null);
    }
  }
}
