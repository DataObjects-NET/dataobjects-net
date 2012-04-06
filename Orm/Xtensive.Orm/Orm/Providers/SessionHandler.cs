// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Data;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract partial class SessionHandler : IDisposable
  {
    private static readonly object CachingRegion = new object();

    /// <summary>
    /// Gets <see cref="HandlerAccessor"/>.
    /// </summary>
    protected HandlerAccessor Handlers { get; private set; }

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    /// <summary>
    /// Gets the real session handler (the final handler in chain of all <see cref="ChainingSessionHandler"/>s).
    /// </summary>
    /// <returns>The real session handler.</returns>
    public SessionHandler GetRealHandler()
    {
      var handler = this;
      while (handler is ChainingSessionHandler)
        handler = (handler as ChainingSessionHandler).ChainedHandler;
      return handler;
    }

    /// <summary>
    /// Executes the specified query tasks.
    /// </summary>
    /// <param name="queryTasks">The query tasks to execute.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> partial execution is allowed.</param>
    public abstract void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution);

    /// <summary>
    /// Sets command timeout for all <see cref="IDbCommand"/> created within current instance.
    /// </summary>
    /// <param name="commandTimeout">The command timeout.</param>
    public abstract void SetCommandTimeout(int? commandTimeout);

    /// <summary>
    /// Persists changed entities.
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> dirty flush is allowed.</param>
    public abstract void Persist(EntityChangeRegistry registry, bool allowPartialExecution);

    #region IoC support (Domain.Services)

    /// <summary>
    /// Creates parent service container 
    /// for <see cref="Orm.Session.Services"/> container.
    /// </summary>
    /// <returns>Container providing base services.</returns>
    public virtual IServiceContainer CreateBaseServices()
    {
      var registrations = new List<ServiceRegistration>{
        new ServiceRegistration(typeof (Session), Session),
        new ServiceRegistration(typeof (SessionConfiguration), Session.Configuration),
        new ServiceRegistration(typeof (SessionHandler), this),
      };
      AddBaseServiceRegistrations(registrations);
      return new ServiceContainer(registrations, Session.Domain.Services);
    }

    /// <summary>
    /// Adds base service registration entries into the list of
    /// registrations used by <see cref="CreateBaseServices"/>
    /// method.
    /// </summary>
    /// <param name="registrations">The list of service registrations.</param>
    protected virtual void AddBaseServiceRegistrations(List<ServiceRegistration> registrations)
    {
    }

    #endregion

    // Initialization

    public void Initialize(HandlerAccessor handlers, Session session)
    {
      if (Handlers!=null)
        throw new InvalidOperationException();

      Handlers = handlers;
      Session = session;

      Initialize();
    }

    /// <inheritdoc/>
    protected virtual void Initialize()
    {
    }

    // Disposing

    /// <inheritdoc/>
    public abstract void Dispose();
  }
}