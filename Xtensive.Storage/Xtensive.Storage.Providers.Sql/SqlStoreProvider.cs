// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using System;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlStoreProvider : SqlProvider,
    IHasNamedResult
  {
    private const string TemporaryTableLockName = "TemporaryTableLockName";

    private readonly TemporaryTableDescriptor descriptor;
    
    private new StoreProvider Origin { get { return (StoreProvider) base.Origin; } }
    private ExecutableProvider Source { get { return (ExecutableProvider) Sources[0]; } }
    private DomainHandler DomainHandler { get { return (DomainHandler) handlers.DomainHandler; } }

    #region IHasNamedResult members

    public TemporaryDataScope Scope { get { return Origin.Scope; } }
    public string Name { get { return Origin.Name; } }

    #endregion

    protected override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      var tableLock = DomainHandler.TemporaryTableManager.Acquire(descriptor);
      context.SetValue(this, TemporaryTableLockName, tableLock);
      var executor = handlers.SessionHandler.GetService<IQueryExecutor>();
      executor.Store(descriptor, Source);
    }

    protected override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      var tableLock = context.GetValue<IDisposable>(this, TemporaryTableLockName);
      if (tableLock!=null)
        using (tableLock)
          handlers.SessionHandler.GetService<IQueryExecutor>().Clear(descriptor);
      base.OnAfterEnumerate(context);
    }

    // Constructors

    public SqlStoreProvider(StoreProvider origin, TemporaryTableDescriptor descriptor, HandlerAccessor handlers, ExecutableProvider source)
     : base(origin, descriptor.QueryStatement, handlers, null, false, source)
    {
      AddService<IHasNamedResult>();
      this.descriptor = descriptor;
    }
  }
}