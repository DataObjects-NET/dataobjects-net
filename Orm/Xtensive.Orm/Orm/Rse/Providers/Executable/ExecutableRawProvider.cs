// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using Xtensive.Orm.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Enumerates specified array of <see cref="Tuple"/> instances.
  /// </summary>
  [Serializable]
  public sealed class ExecutableRawProvider : ExecutableProvider<Providers.RawProvider>
  {
    #region Cached properties

    private const string CachedSourceName = "CachedSource";

    #endregion

    /// <inheritdoc/>
    protected override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      var parameterContext = ((Xtensive.Orm.Providers.EnumerationContext) context).ParameterContext;
      SetValue(context, CachedSourceName, Origin.CompiledSource.Invoke(parameterContext));
    }

    /// <inheritdoc/>
    protected override TupleEnumerator OnEnumerate(EnumerationContext context)
    {
      return new TupleEnumerator(GetValue<IEnumerable<Tuple>>(context, CachedSourceName));
    }


    // Constructors

    public ExecutableRawProvider(Providers.RawProvider origin)
      : base(origin)
    {
    }
  }
}