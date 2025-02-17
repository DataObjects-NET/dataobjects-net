// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Default implementation of SQL temporary data provider.
  /// </summary>
  public sealed class SqlStoreProvider : SqlTemporaryDataProvider
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
    protected internal override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      LockAndStore(context, Source.ToEnumerable(context));
    }

    /// <inheritdoc/>
    protected internal override async Task OnBeforeEnumerateAsync(Rse.Providers.EnumerationContext context, CancellationToken token)
    {
      await base.OnBeforeEnumerateAsync(context, token).ConfigureAwaitFalse();
      await LockAndStoreAsync(context, Source.ToEnumerable(context), token).ConfigureAwaitFalse();
    }

    protected internal override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      _ = ClearAndUnlock(context);
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
      Initialize();
    }
  }
}
