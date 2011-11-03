// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Orm.Services;
using Xtensive.Storage.Services;
using Tuple = Xtensive.Tuples.Tuple;

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