// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.07

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    #region OnXxxEnumerate methods (to override)

    /// <summary>
    /// Called when enumerator is created on this provider.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    protected virtual void OnBeforeEnumerate(EnumerationContext context)
    {
      foreach (var source in Sources) {
        var ep = source as ExecutableProvider;
        if (ep!=null)
          ep.OnBeforeEnumerate(context);
      }
    }

    /// <summary>
    /// Called when enumeration is finished.
    /// </summary>
    /// <param name="context">The enumeration context.</param>
    protected virtual void OnAfterEnumerate(EnumerationContext context)
    {
      foreach (var source in Sources) {
        var ep = source as ExecutableProvider;
        if (ep != null)
          ep.OnAfterEnumerate(context);
      }
    }

    protected abstract TupleReader OnEnumerate(EnumerationContext context);

    protected virtual Task<TupleReader> OnEnumerateAsync(EnumerationContext context, CancellationToken token)
    {
      //Default version is synchronous
      token.ThrowIfCancellationRequested();
      return Task.FromResult(OnEnumerate(context));
    }

    #endregion

    #region Caching related methods

    protected T GetValue<T>(EnumerationContext context, string name)
      where T : class
    {
      return context.GetValue<T>(this, name);
    }

    protected void SetValue<T>(EnumerationContext context, string name, T value)
      where T : class
    {
      context.SetValue(this, name, value);
    }

    #endregion

    #region IEnumerable<...> methods

    public ExecutableProviderTupleReader GetReader(EnumerationContext context) =>
      new ExecutableProviderTupleReader(context, this);

    public readonly struct ExecutableProviderTupleReader: IEnumerable<Tuple>, IAsyncEnumerable<Tuple>
    {
      private readonly EnumerationContext context;
      private readonly ExecutableProvider provider;

      /// <inheritdoc/>
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

      /// <inheritdoc/>
      public IEnumerator<Tuple> GetEnumerator()
      {
        const string enumerationMarker = "Enumerated";
        var enumerated = context.GetValue<bool>(this, enumerationMarker);
        if (!enumerated) {
          provider.OnBeforeEnumerate(context);
        }

        try {
          context.SetValue(this, enumerationMarker, true);
          var tupleReader = provider.OnEnumerate(context);
          foreach (var tuple in tupleReader) {
            yield return tuple;
          }
        }
        finally {
          if (!enumerated) {
            provider.OnAfterEnumerate(context);
          }
        }
      }

      public async IAsyncEnumerator<Tuple> GetAsyncEnumerator(CancellationToken token = default)
      {
        const string enumerationMarker = "Enumerated";
        var enumerated = context.GetValue<bool>(this, enumerationMarker);
        if (!enumerated) {
          provider.OnBeforeEnumerate(context);
        }

        try {
          context.SetValue(this, enumerationMarker, true);
          var tupleReader = await provider.OnEnumerateAsync(context, token);
          await foreach (var tuple in tupleReader.WithCancellation(token)) {
            yield return tuple;
          }
        }
        finally {
          if (!enumerated) {
            provider.OnAfterEnumerate(context);
          }
        }
      }

      public ExecutableProviderTupleReader(EnumerationContext context, ExecutableProvider provider)
      {
        this.context = context;
        this.provider = provider;
      }
    }


    // public async Task<IEnumerator<Tuple>> GetEnumeratorAsync(EnumerationContext context, CancellationToken token)
    // {
    //   const string enumerationMarker = "Enumerated";
    //   var enumerated = context.GetValue<bool>(this, enumerationMarker);
    //   bool onEnumerationExecuted = false;
    //   if (!enumerated)
    //     OnBeforeEnumerate(context);
    //   try {
    //     context.SetValue(this, enumerationMarker, true);
    //     var enumerator = (await OnEnumerateAsync(context, token).ConfigureAwait(false))
    //       .ToEnumerator(
    //         () => {
    //           if (!enumerated) {
    //             OnAfterEnumerate(context);
    //           }
    //         });
    //     onEnumerationExecuted = true;
    //     return enumerator;
    //   }
    //   finally {
    //     if (!enumerated && !onEnumerationExecuted)
    //       OnAfterEnumerate(context);
    //   }
    // }

    #endregion

    /// <exception cref="InvalidOperationException"><see cref="Origin"/> is <see langword="null" />.</exception>
    protected override RecordSetHeader BuildHeader()
    {
      return Origin.Header;
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
