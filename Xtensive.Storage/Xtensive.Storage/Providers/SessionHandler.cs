// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.IoC;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Base <see cref="Session"/> handler class.
  /// </summary>
  public abstract partial class SessionHandler : InitializableHandlerBase,
    IHasServices,
    IDisposable
  {
    private static readonly object CachingRegion = new object();
    
    /// <summary>
    /// The <see cref="object"/> to synchronize access to a connection.
    /// </summary>
    protected readonly object connectionSyncRoot = new object();

    /// <summary>
    /// Gets the current <see cref="Session"/>.
    /// </summary>
    public Session Session { get; internal set; }

    /// <summary>
    /// Gets the query provider.
    /// </summary>
    public virtual QueryProvider QueryProvider {get { return QueryProvider.Instance; }}
    
    /// <summary>
    /// Acquires the connection lock.
    /// </summary>
    /// <returns>An implementation of <see cref="IDisposable"/> which should be disposed 
    /// to release the connection lock.</returns>
    public IDisposable AcquireConnectionLock()
    {
      Monitor.Enter(connectionSyncRoot);
      return new Disposable<object>(connectionSyncRoot, (disposing, syncRoot) => Monitor.Exit(syncRoot));
    }

    /// <summary>
    /// Creates enumeration context.
    /// </summary>
    /// <returns>Created context.</returns>
    public virtual Rse.Providers.EnumerationContext CreateEnumerationContext()
    {
      return new EnumerationContext(GetEnumerationContextOptions());
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
        using (EnumerationScope.Open())
        using (task.ParameterContext.ActivateSafely())
          task.Result = task.DataSource.ToList();
      }
    }

    #region IoC support (Domain.Services)

    /// <summary>
    /// Creates parent service container 
    /// for <see cref="Storage.Session.Services"/> container.
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
    public virtual T GetService<T>()
      where T : class
    {
      var result = this as T;
      return result;
    }

    #endregion

    // Initialization

    /// <inheritdoc/>
    public override void Initialize()
    {
      prefetchManager = new PrefetchManager(Session);
      persistRequiresTopologicalSort =
        (Handlers.Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0 &&
          Handlers.Domain.Handler.ProviderInfo.Supports(ProviderFeatures.ForeignKeyConstraints) &&
            !Handlers.Domain.Handler.ProviderInfo.Supports(ProviderFeatures.DeferrableConstraints);
    }

    // Disposing

    /// <inheritdoc/>
    public abstract void Dispose();
  }
}