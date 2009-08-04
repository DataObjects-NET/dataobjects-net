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
    /// <param name="primaryKeyIndex">Index of primary key within the <see cref="Record"/>.</param>
    /// <returns>The sequence of <see cref="Entity"/> instances.</returns>
    public static IEnumerable<T> ToEntities<T>(this RecordSet source, int primaryKeyIndex)
      where T : class, IEntity
    {
      foreach (var entity in ToEntities(source, primaryKeyIndex))
        yield return entity as T;
    }

    /// <summary>
    /// Converts the <see cref="RecordSet"/> items to <see cref="Entity"/> instances.
    /// </summary>
    /// <param name="source">The <see cref="RecordSet"/> to process.</param>
    /// <param name="primaryKeyIndex">Index of primary key within the <see cref="Record"/>.</param>
    /// <returns>The sequence of <see cref="Entity"/> instances.</returns>
    public static IEnumerable<Entity> ToEntities(this RecordSet source, int primaryKeyIndex)
    {
      var session = Session.Demand();
      var reader = session.Domain.RecordSetReader;
      foreach (var record in reader.Read(source)) {
        var key = record.GetKey(primaryKeyIndex);
        var tuple = record.GetTuple(primaryKeyIndex);
        if (key!=null && tuple!=null)
          yield return session.UpdateEntityState(key, tuple).Entity;
      }
    }

    public static IEnumerable<Record> Read(this RecordSet source)
    {
      return Domain.Demand().RecordSetReader.Read(source);
    }

    public static Record ReadSingleRow(this RecordSet source)
    {
      return Domain.Demand().RecordSetReader.ReadSingleRow(source, null);
    }
  }
}
