// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.IoC;
using Xtensive.Orm;
using Xtensive.Parameters;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract partial class SessionHandler : InitializableHandlerBase,
    IDisposable
  {
    private static readonly object CachingRegion = new object();

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
    /// Creates enumeration context.
    /// </summary>
    /// <returns>Created context.</returns>
    public virtual Rse.Providers.EnumerationContext CreateEnumerationContext()
    {
      return new EnumerationContext(this, GetEnumerationContextOptions());
    }

    /// <summary>
    /// Gets the enumeration context options.
    /// </summary>
    /// <returns>Options for new enumeration context.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><c>Session.Configuration.ReaderPreloading</c> is out of range.</exception>
    protected virtual EnumerationContextOptions GetEnumerationContextOptions()
    {
      var options = (EnumerationContextOptions) 0;
      switch (Session.Configuration.ReaderPreloading) {
      case ReaderPreloadingPolicy.Auto:
        bool marsSupported = Handlers.DomainHandler.ProviderInfo
          .Supports(ProviderFeatures.MultipleActiveResultSets);
        if (!marsSupported)
          options |= EnumerationContextOptions.GreedyEnumerator;
        break;
      case ReaderPreloadingPolicy.Always:
        options |= EnumerationContextOptions.GreedyEnumerator;
        break;
      case ReaderPreloadingPolicy.Never:
        break;
      default:
        throw new ArgumentOutOfRangeException("Session.Configuration.ReaderPreloading");
      }
      return options;
    }

    /// <summary>
    /// Executes the specified query tasks.
    /// </summary>
    /// <param name="queryTasks">The query tasks to execute.</param>
    /// <param name="allowPartialExecution">if set to <see langword="true"/> partial execution is allowed.</param>
    public virtual void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      foreach (var task in queryTasks) {
        using (CreateEnumerationContext().Activate())
        using (task.ParameterContext.ActivateSafely())
          task.Result = task.DataSource.ToList();
      }
    }

    /// <summary>
    /// Sets command timeout for all <see cref="IDbCommand"/> created within current instance.
    /// </summary>
    /// <param name="commandTimeout">The command timeout.</param>
    public abstract void SetCommandTimeout(int? commandTimeout);

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
      return;
    }

    #endregion

    #region IHasServices members

    /// <inheritdoc/>
    public T GetService<T>()
      where T : class
    {
      var service = GetRealHandler() as T;
      if (service!=null)
        return service;
      throw new NotSupportedException(string.Format(
        "Service '{0}' is not supported by '{1}'",
        typeof (T).GetShortName(),
        typeof(SessionHandler)));
    }

    #endregion

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      prefetchManager = new PrefetchManager(Session);

      var providerInfo = Handlers.DomainHandler.ProviderInfo;
      var configuration = Handlers.Domain.Configuration;

      persistRequiresTopologicalSort =
        (configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0
        && providerInfo.Supports(ProviderFeatures.ForeignKeyConstraints)
        && !providerInfo.Supports(ProviderFeatures.DeferrableConstraints);
    }

    // Disposing

    /// <inheritdoc/>
    public abstract void Dispose();
  }
}