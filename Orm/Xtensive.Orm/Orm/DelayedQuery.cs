// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Materialization;

namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Abstract base for a future query and future scalar implementation.
  /// </summary>
  [Serializable]
  public abstract class DelayedQuery
  {
    protected readonly ParameterContext parameterContext;
    protected readonly Materializer materializer;

    /// <summary>
    /// Gets <see cref="Session"/> this instance is bound to.
    /// </summary>
    public Session Session { get; }

    /// <summary>
    /// Gets <see cref="StateLifetimeToken"/> this instance is bound to.
    /// </summary>
    public StateLifetimeToken LifetimeToken { get; }

    /// <summary>
    /// Gets the task for this future.
    /// </summary>
    public QueryTask Task { get; }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="translatedQuery">The translated query.</param>
    /// <param name="outerParameterContext">The parameter context.</param>
    internal DelayedQuery(Session session, TranslatedQuery translatedQuery, ParameterContext outerParameterContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, nameof(session));
      ArgumentValidator.EnsureArgumentNotNull(translatedQuery, nameof(translatedQuery));
      ArgumentValidator.EnsureArgumentNotNull(parameterContext, nameof(parameterContext));

      Session = session;
      LifetimeToken = session.GetLifetimeToken();

      materializer = translatedQuery.Materializer;
      parameterContext = new ParameterContext(outerParameterContext);
      foreach (var (parameter, tuple) in translatedQuery.TupleParameterBindings) {
        parameterContext.SetValue(parameter, tuple);
      }

      Task = new QueryTask(translatedQuery.DataSource, LifetimeToken, parameterContext);
    }
  }
}