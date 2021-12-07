// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      await base.OnBeforeEnumerateAsync(context, token).ConfigureAwait(false);
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