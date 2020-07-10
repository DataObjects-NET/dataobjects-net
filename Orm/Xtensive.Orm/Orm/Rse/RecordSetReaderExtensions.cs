// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.09

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using EnumerationContext = Xtensive.Orm.Providers.EnumerationContext;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// <see cref="RecordSetReader"/> related extension methods.
  /// </summary>
  public static class RecordSetReaderExtensions
  {
    /// <summary>
    /// Converts the <see cref="RecordSetReader"/> items to <see cref="Entity"/> instances.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Entity"/> instances to get.</typeparam>
    /// <param name="source">The <see cref="RecordSetReader"/> to process.</param>
    /// <param name="primaryKeyIndex">Index of primary key within the <see cref="Record"/>.</param>
    /// <returns>The sequence of <see cref="Entity"/> instances.</returns>
    public static IEnumerable<T> ToEntities<T>(this RecordSetReader source, int primaryKeyIndex)
      where T : class, IEntity =>
      ToEntities(source, primaryKeyIndex).Cast<T>();

    /// <summary>
    /// Converts the <see cref="IEnumerable{T}"/> of tuples to <see cref="Entity"/> instances.
    /// </summary>
    /// <param name="source">The tuples to process.</param>
    /// <param name="header">The record set header.</param>
    /// <param name="session">A <see cref="Session"/> instance.</param>
    /// <param name="primaryKeyIndex">Index of primary key within the <see cref="Record"/>.</param>
    /// <returns>
    /// The sequence of <see cref="Entity"/> instances.
    /// </returns>
    public static IEnumerable<T> ToEntities<T>(this IEnumerable<Tuple> source, RecordSetHeader header, Session session, int primaryKeyIndex)
      where T : class, IEntity =>
      ToEntities(source, header, session, primaryKeyIndex).Cast<T>();

    /// <summary>
    /// Converts the <see cref="RecordSetReader"/> items to <see cref="Entity"/> instances.
    /// </summary>
    /// <param name="source">The <see cref="RecordSetReader"/> to process.</param>
    /// <param name="primaryKeyIndex">Index of primary key within the <see cref="Record"/>.</param>
    /// <returns>The sequence of <see cref="Entity"/> instances.</returns>
    public static IEnumerable<Entity> ToEntities(this RecordSetReader source, int primaryKeyIndex)
    {
      var session = ((EnumerationContext) source.Context).Session;
      var reader = session.Domain.EntityDataReader;
      foreach (var record in reader.Read(source.ToEnumerable(), source.Header, session)) {
        var key = record.GetKey(primaryKeyIndex);
        if (key==null)
          continue;
        var tuple = record.GetTuple(primaryKeyIndex);
        if (tuple!=null)
          yield return session.Handler.UpdateState(key, tuple).Entity;
        else
          yield return session.Query.SingleOrDefault(key);
      }
    }

    /// <summary>
    /// Converts the <see cref="IEnumerable{T}"/> of tuples to <see cref="Entity"/> instances.
    /// </summary>
    /// <param name="source">The tuples to process.</param>
    /// <param name="header">The record set header.</param>
    /// <param name="session">The session.</param>
    /// <param name="primaryKeyIndex">Index of primary key within the <see cref="Record"/>.</param>
    /// <returns>
    /// The sequence of <see cref="Entity"/> instances.
    /// </returns>
    public static IEnumerable<Entity> ToEntities(this IEnumerable<Tuple> source, RecordSetHeader header, Session session, int primaryKeyIndex)
    {
      var reader = session.Domain.EntityDataReader;
      foreach (var record in reader.Read(source, header, session)) {
        var key = record.GetKey(primaryKeyIndex);
        if (key == null)
          continue;
        var tuple = record.GetTuple(primaryKeyIndex);
        if (tuple!=null)
          yield return session.Handler.UpdateState(key, tuple).Entity;
        else
          yield return session.Query.SingleOrDefault(key);
      }
    }
  }
}
