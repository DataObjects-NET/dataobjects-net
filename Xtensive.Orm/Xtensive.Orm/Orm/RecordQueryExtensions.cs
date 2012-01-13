// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.13

using System;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm
{
  /// <summary>
  /// <see cref="RecordQuery"/> related extension methods.
  /// </summary>
  public static class RecordQueryExtensions
  {
    /// <summary>
    /// Compiles provided <paramref name="query"/> and returns new <see cref="RecordSet"/> bound to provided <paramref name="session"/>.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="session">The session.</param>
    /// <returns>New <see cref="RecordSet"/> bound to provided <paramref name="session"/>.</returns>
    public static RecordSet ToRecordSet (this RecordQuery query, Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      var executableProvider = session.CompilationService.Compile(query.Provider);
      return new RecordSet(session.CreateEnumerationContext(), executableProvider);
    }

    /// <summary>
    /// Compiles the specified query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="session">The session.</param>
    /// <returns>Compiled <see cref="ExecutableProvider"/>.</returns>
    public static ExecutableProvider Compile(this RecordQuery query, Session session)
    {
      return session.CompilationService.Compile(query.Provider);
    }

    /// <summary>
    /// Creates new <see cref="RecordQuery"/> that calculates count of elements of provided <paramref name="recordQuery"/>, compiles and executes them.
    /// </summary>
    /// <param name="recordQuery">The record query.</param>
    /// <param name="session">The session.</param>
    public static long Count(this RecordQuery recordQuery, Session session)
    {
      var resultQuery = recordQuery.Aggregate(null, 
        new AggregateColumnDescriptor("$Count", 0, AggregateType.Count));
      var recordSet = resultQuery.ToRecordSet(session);
      return recordSet.First().GetValue<long>(0);
    }
  }
}