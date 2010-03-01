// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlStoreProvider : SqlTemporaryDataProvider,
    IHasNamedResult
  {
    private new StoreProvider Origin { get { return (StoreProvider) base.Origin; } }
    private ExecutableProvider Source { get { return (ExecutableProvider) Sources[0]; } }

    #region IHasNamedResult members

    public TemporaryDataScope Scope { get { return Origin.Scope; } }
    public string Name { get { return Origin.Name; } }

    #endregion

    protected override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      LockAndStore(context, Source);
    }

    protected override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      ClearAndUnlock(context);
      base.OnAfterEnumerate(context);
    }


    // Constructors

    public SqlStoreProvider(
      HandlerAccessor handlers, QueryRequest request, TemporaryTableDescriptor descriptor,
      StoreProvider origin, ExecutableProvider source)
     : base(handlers, request, descriptor, origin, new [] {source})
    {
      AddService<IHasNamedResult>();
    }
  }
}