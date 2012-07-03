// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Default implementation of SQL temporary data provider.
  /// </summary>
  public class SqlStoreProvider : SqlTemporaryDataProvider
  {
    private new StoreProvider Origin
    {
      get { return (StoreProvider) base.Origin; }
    }

    private ExecutableProvider Source
    {
      get { return (ExecutableProvider) Sources[0]; }
    }

    /// <inheritdoc/>
    public override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      LockAndStore(context, Source);
    }

    public override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      ClearAndUnlock(context);
      base.OnAfterEnumerate(context);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
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
    }
  }
}