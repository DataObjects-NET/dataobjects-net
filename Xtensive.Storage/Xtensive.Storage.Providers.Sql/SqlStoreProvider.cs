// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Services;
using Tuple = Xtensive.Core.Tuples.Tuple;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Default implementation of SQL temporary data provider.
  /// </summary>
  public class SqlStoreProvider : SqlTemporaryDataProvider,
    IHasNamedResult
  {
    private new StoreProvider Origin
    {
      get { return (StoreProvider) base.Origin; }
    }

    private ExecutableProvider Source
    {
      get { return (ExecutableProvider) Sources[0]; }
    }

    #region IHasNamedResult members

    /// <inheritdoc/>
    public TemporaryDataScope Scope
    {
      get { return Origin.Scope; }
    }

    /// <inheritdoc/>
    public string Name
    {
      get { return Origin.Name; }
    }

    #endregion

    /// <inheritdoc/>
    protected override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      LockAndStore(context, Source);
    }


    private void LockAndStore(Rse.Providers.EnumerationContext context, IEnumerable<Tuple> data)
    {
      var tableLock = DomainHandler.TemporaryTableManager.Acquire(TableDescriptor);
      if (tableLock==null)
        return;
      var transactionMonitor = handlers.SessionHandler.Session.Services.Get<TransactionMonitor>();
      transactionMonitor.SetValue(new Disposable(disposing => {
        using (tableLock)
          handlers.SessionHandler.GetService<IQueryExecutor>(true).Clear(TableDescriptor);
      }));
      var executor = handlers.SessionHandler.GetService<IQueryExecutor>(true);
      executor.Store(TableDescriptor, data);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="handlers">The handlers.</param>
    /// <param name="request">The request.</param>
    /// <param name="descriptor">The descriptor.</param>
    /// <param name="origin">The origin.</param>
    /// <param name="source">The source.</param>
    public SqlStoreProvider(
      HandlerAccessor handlers, QueryRequest request, TemporaryTableDescriptor descriptor,
      StoreProvider origin, ExecutableProvider source)
      : base(handlers, request, descriptor, origin, new[] {source})
    {
      AddService<IHasNamedResult>();
    }
  }
}