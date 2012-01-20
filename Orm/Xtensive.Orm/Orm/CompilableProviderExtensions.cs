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
  /// <see cref="CompilableProvider"/> related extension methods.
  /// </summary>
  public static class CompilableProviderExtensions
  {
    /// <summary>
    /// Compiles provided <paramref name="query"/> and returns new <see cref="RecordSet"/> bound to provided <paramref name="session"/>.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="session">The session.</param>
    /// <returns>New <see cref="RecordSet"/> bound to provided <paramref name="session"/>.</returns>
    public static RecordSet GetRecordSet(this CompilableProvider query, Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      var executableProvider = session.CompilationService.Compile(query);
      return new RecordSet(session.CreateEnumerationContext(), executableProvider);
    }

    /// <summary>
    /// Compiles the specified query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="session">The session.</param>
    /// <returns>Compiled <see cref="ExecutableProvider"/>.</returns>
    public static ExecutableProvider Compile(this CompilableProvider query, Session session)
    {
      return session.CompilationService.Compile(query);
    }

    /// <summary>
    /// Calculates count of elements of provided <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The record query.</param>
    /// <param name="session">The session.</param>
    public static long Count(this CompilableProvider query, Session session)
    {
      return query
        .Aggregate(null, new AggregateColumnDescriptor("$Count", 0, AggregateType.Count))
        .GetRecordSet(session)
        .First()
        .GetValue<long>(0);
    }
  }
}