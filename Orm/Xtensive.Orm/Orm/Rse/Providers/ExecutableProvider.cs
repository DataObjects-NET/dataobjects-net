// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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

    protected internal abstract DataReader OnEnumerate(EnumerationContext context);

    protected internal virtual Task<DataReader> OnEnumerateAsync(EnumerationContext context, CancellationToken token)
    {
      //Default version is synchronous
      token.ThrowIfCancellationRequested();
      return Task.FromResult(OnEnumerate(context));
    }

    #endregion

    #region Caching related methods

    protected T GetValue<T>(EnumerationContext context, string name)
      where T : class =>
      context.GetValue<T>(this, name);

    protected void SetValue<T>(EnumerationContext context, string name, T value)
      where T : class =>
      context.SetValue(this, name, value);

    #endregion

    public IEnumerable<Tuple> ToEnumerable(EnumerationContext context)
    {
      using var tupleReader = RecordSetReader.Create(context, this);
      while (tupleReader.MoveNext()) {
        yield return tupleReader.Current;
      }
    }

    /// <summary>
    /// Gets <see cref="RecordSetReader"/> bound to the specified <paramref name="provider"/>.
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
    /// Asynchronously gets <see cref="RecordSetReader"/> bound to the specified <paramref name="provider"/>.
    /// </summary>
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
      return await RecordSetReader.CreateAsync(enumerationContext, this, token);
    }

    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="origin">The <see cref="Origin"/> property value.</param>
    /// <param name="sources">The <see cref="Provider.Sources"/> property value.</param>
    protected ExecutableProvider(CompilableProvider origin, params ExecutableProvider[] sources)
      : base(origin.Type, sources)
    {
      Origin = origin;
    }
  }
}
