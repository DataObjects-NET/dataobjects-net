// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.08

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Enumerates specified array of <see cref="Tuple"/> instances.
  /// </summary>
  [Serializable]
  public sealed class ExecutableRawProvider : ExecutableProvider<Providers.RawProvider>
  {
    private const string CachedSourceName = "CachedSource";

    /// <inheritdoc/>
    protected override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      context.SetValue(this, CachedSourceName, Origin.CompiledSource.Invoke());
    }

    /// <inheritdoc/>
    protected override async Task OnBeforeEnumerateAsync(EnumerationContext context, CancellationToken token)
    {
      await base.OnBeforeEnumerateAsync(context, token);
      context.SetValue(this, CachedSourceName, Origin.CompiledSource.Invoke());
    }

    /// <inheritdoc/>
    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return context.GetValue<IEnumerable<Tuple>>(this, CachedSourceName);
    }


    // Constructors

    public ExecutableRawProvider(Providers.RawProvider origin)
      : base(origin)
    {
    }
  }
}