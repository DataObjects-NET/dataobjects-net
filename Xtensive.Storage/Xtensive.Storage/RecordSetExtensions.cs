// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.09

using System;
using System.Collections.Generic;
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
      Domain domain = Domain.Demand();
      var parser = domain.RecordSetParser;
      var session = Session.Current;
      int keyIndex = -1;
      foreach (var record in parser.Parse(source)) {
        if (keyIndex == -1)
          for (int i = 0; i < record.PrimaryKeys.Count; i++) {
            var key = record.PrimaryKeys[i];
            if (key != null && type.IsAssignableFrom(key.EntityType.UnderlyingType)) {
              keyIndex = i;
              break;
            }
          }
        var pk = record[keyIndex];
        var entity = null as Entity;
        if (pk != null)
          entity = pk.Resolve(session);
        yield return entity;
      }
    }

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
      Domain domain = Domain.Demand();
      var parser = domain.RecordSetParser;
      var session = Session.Current;
      foreach (var record in parser.Parse(source)) {
        var pk = record[primaryKeyIndex];
        var entity = null as Entity;
        if (pk != null)
          entity = pk.Resolve(session);
        yield return entity;
      }
    }

    public static IEnumerable<Record> Parse(this RecordSet source)
    {
      Domain domain = Domain.Demand();
      return domain.RecordSetParser.Parse(source);
    }

    public static Record ParseFirst(this RecordSet source)
    {
      Domain domain = Domain.Demand();
      return domain.RecordSetParser.ParseFirst(source);
    }
  }
}
