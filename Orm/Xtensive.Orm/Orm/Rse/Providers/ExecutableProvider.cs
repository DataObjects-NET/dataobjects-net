// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.07.07

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any query provider that can be directly executed.
  /// </summary>
  [Serializable]
  public abstract class ExecutableProvider : Provider
  {
    /// <summary>
    /// Gets the provider this provider is compiled from.
    /// </summary>
    public CompilableProvider Origin { get; private set; }

    /// <exception cref="InvalidOperationException"><see cref="Origin"/> is <see langword="null" />.</exception>
    protected override RecordSetHeader BuildHeader() => Origin.Header;

    #region OnXxxEnumerate methods (to override)

    /// <summary>
    /// Called when enumerator is created on this provider.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    protected internal virtual void OnBeforeEnumerate(EnumerationContext context)
    {
      foreach (var source in Sources) {
        if (source is ExecutableProvider ep) {
          ep.OnBeforeEnumerate(context);
        }
      }
    }

    /// <summary>
    /// Called when enumerator is created on this provider.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    /// <param name="token">The cancellation token for operation</param>
    protected internal virtual async Task OnBeforeEnumerateAsync(EnumerationContext context, CancellationToken token)
    {
      token.ThrowIfCancellationRequested();
      foreach (var source in Sources) {
        if (source is ExecutableProvider ep) {
          await ep.OnBeforeEnumerateAsync(context, token).ConfigureAwait(false);
        }
      }
    }

    /// <summary>
    /// Called when enumeration is finished.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    protected internal virtual void OnAfterEnumerate(EnumerationContext context)
    {
      foreach (var source in Sources) {
        if (source is ExecutableProvider ep) {
          ep.OnAfterEnumerate(context);
        }
      }
    }

    /// <summary>
    /// Starts enumeration of the given <see cref="ExecutableProvider"/>.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    /// <returns><see cref="DataReader"/> ready to be iterated.</returns>
    protected internal abstract DataReader OnEnumerate(EnumerationContext context);

    /// <summary>
    /// Asynchronously starts enumeration of the given <see cref="ExecutableProvider"/>.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    /// <param name="token">The <see cref="CancellationToken"/> to interrupt execution if necessary.</param>
    /// <returns><see cref="DataReader"/> ready to be iterated.</returns>
    protected internal virtual Task<DataReader> OnEnumerateAsync(EnumerationContext context, CancellationToken token)
    {
      //Default version is synchronous
      token.ThrowIfCancellationRequested();
      return Task.FromResult(OnEnumerate(context));
    }

    #endregion

    /// <summary>
    /// Gets value of type <typeparamref name="T"/> previously cached in
    /// <see cref="EnumerationContext"/> by its <paramref name="name"/>.
    /// </summary>
    /// <param name="context"><see cref="EnumerationContext"/> instance where value cache resides.</param>
    /// <param name="name">The name of the required cached value.</param>
    /// <typeparam name="T">The type of the value in cache.</typeparam>
    /// <returns> Cached value with the specified key;
    /// or <see langword="null"/>, if no cached value is found, or it has already expired.
    /// </returns>
    protected T GetValue<T>(EnumerationContext context, string name)
      where T : class =>
      context.GetValue<T>(this, name);

    /// <summary>
    /// Puts specified <paramref name="value"/> into the cache residing in the provided
    /// <see cref="EnumerationContext"/> instance using the <paramref name="name"/> as the key.
    /// </summary>
    /// <param name="context">The <see cref="EnumerationContext"/> where to cache <paramref name="value"/>.</param>
    /// <param name="name">The name of the <paramref name="value"/> to be cached.</param>
    /// <param name="value">The value of <typeparamref name="T"/> type to be cached.</param>
    /// <typeparam name="T">The type of the provided <paramref name="value"/>.</typeparam>
    protected void SetValue<T>(EnumerationContext context, string name, T value)
      where T : class =>
      context.SetValue(this, name, value);

    /// <summary>
    /// Transforms current <see cref="ExecutableProvider"/> instance to <see cref="IEnumerable{T}"/>
    /// sequence of <see cref="Tuple"/>s.
    /// </summary>
    /// <param name="context">The <see cref="EnumerationContext"/> instance where to perform enumeration.</param>
    /// <returns>Sequence of <see cref="Tuple"/>s.</returns>
    public IEnumerable<Tuple> ToEnumerable(EnumerationContext context)
    {
      using var tupleReader = RecordSetReader.Create(context, this);
      while (tupleReader.MoveNext()) {
        yield return tupleReader.Current;
      }
    }

    /// <summary>
    /// Gets <see cref="RecordSetReader"/> bound to the specified <paramref name="session"/>.
    /// </summary>
    /// <param name="session">Session to bind.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    /// <returns>New <see cref="RecordSetReader"/> bound to specified <paramref name="session"/>.</returns>
    public RecordSetReader GetRecordSetReader(Session session, ParameterContext parameterContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, nameof(session));
      var enumerationContext = session.CreateEnumerationContext(parameterContext);
      return RecordSetReader.Create(enumerationContext, this);
    }

    /// <summary>
    /// Asynchronously gets <see cref="RecordSetReader"/> bound to the specified <paramref name="session"/>.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="session">Session to bind.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing this operation.</returns>
    public async Task<RecordSetReader> GetRecordSetReaderAsync(
      Session session, ParameterContext parameterContext, CancellationToken token)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, nameof(session));
      var enumerationContext =
        await session.CreateEnumerationContextAsync(parameterContext, token).ConfigureAwait(false);
      return await RecordSetReader.CreateAsync(enumerationContext, this, token).ConfigureAwait(false);
    }

    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="origin">The <see cref="Origin"/> property value.</param>
    /// <param name="sources">The <see cref="Provider.Sources"/> property value.</param>
    protected ExecutableProvider(CompilableProvider origin, IReadOnlyList<ExecutableProvider> sources)
      : base(origin.Type, sources)
    {
      Origin = origin;
    }
  }
}
