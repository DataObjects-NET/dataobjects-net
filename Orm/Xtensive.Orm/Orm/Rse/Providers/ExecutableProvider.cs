// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.07

using System;
using System.Collections;
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
    protected virtual void OnBeforeEnumerate(EnumerationContext context)
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
    protected virtual void OnAfterEnumerate(EnumerationContext context)
    {
      foreach (var source in Sources) {
        if (source is ExecutableProvider ep) {
          ep.OnAfterEnumerate(context);
        }
      }
    }

    protected abstract TupleEnumerator OnEnumerate(EnumerationContext context);

    protected virtual Task<TupleEnumerator> OnEnumerateAsync(EnumerationContext context, CancellationToken token)
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

    /// <summary>
    /// Gets <see cref="RecordSet"/> bound to the specified <paramref name="provider"/>.
    /// </summary>
    /// <param name="session">Session to bind.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    /// <returns>New <see cref="RecordSet"/> bound to specified <paramref name="session"/>.</returns>
    public RecordSet GetRecordSet(Session session, ParameterContext parameterContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, nameof(session));
      var enumerationContext = session.CreateEnumerationContext(parameterContext);
      return RecordSet.Create(enumerationContext, this);
    }

    /// <summary>
    /// Asynchronously gets <see cref="RecordSet"/> bound to the specified <paramref name="provider"/>.
    /// </summary>
    /// <param name="session">Session to bind.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance with
    /// the values of query parameters.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing this operation.</returns>
    public async Task<RecordSet> GetRecordSetAsync(
      Session session, ParameterContext parameterContext, CancellationToken token)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, nameof(session));
      var enumerationContext =
        await session.CreateEnumerationContextAsync(parameterContext, token).ConfigureAwait(false);
      return await RecordSet.CreateAsync(enumerationContext, this, token);
    }

    public class RecordSet: IEnumerator<Tuple>, IAsyncEnumerator<Tuple>
    {
      private const string enumerationMarker = "Enumerated";
      private enum State
      {
        New,
        Initialized,
        Finished
      }

      private readonly EnumerationContext context;
      private readonly ExecutableProvider provider;
      private readonly CancellationToken token;
      private readonly bool isGreedy;

      private State state = State.New;
      private bool enumerated;
      private TupleEnumerator tupleEnumerator;
      private ICompletableScope enumerationScope;

      private void Prepare()
      {
        throw new NotImplementedException();
      }

      private async ValueTask PrepareAsync()
      {
        throw new NotImplementedException();
      }

      public bool MoveNext()
      {
        switch (state) {
          case State.New:
            _ = StartEnumeration(false);

            state = State.Initialized;
            goto case State.Initialized;
          case State.Initialized:
            try {
              if (tupleEnumerator.MoveNext()) {
                return true;
              }
            }
            catch {
              FinishEnumeration(true);

              throw;
            }

            FinishEnumeration(false);

            state = State.Finished;
            goto case State.Finished;
          case State.Finished:
          default:
            return false;
        }
      }

      public async ValueTask<bool> MoveNextAsync()
      {
        switch (state) {
          case State.New:
            await StartEnumeration(true);

            state = State.Initialized;
            goto case State.Initialized;
          case State.Initialized:
            try {
              if (await tupleEnumerator.MoveNextAsync()) {
                return true;
              }
            }
            catch {
              FinishEnumeration(true);

              throw;
            }

            FinishEnumeration(false);
            state = State.Finished;
            goto case State.Finished;
          case State.Finished:
          default:
            return false;
        }
      }

      private async ValueTask StartEnumeration(bool executeAsync)
      {
        enumerationScope = context.BeginEnumeration();
        enumerated = context.GetValue<bool>(provider, enumerationMarker);
        if (!enumerated) {
          provider.OnBeforeEnumerate(context);
          context.SetValue(provider, enumerationMarker, true);
        }

        try {
          tupleEnumerator = executeAsync
            ? await provider.OnEnumerateAsync(context, token)
            : provider.OnEnumerate(context);

          if (isGreedy && !tupleEnumerator.IsInMemory) {
            var tuples = new List<Tuple>();
            if (executeAsync) {
              await using (tupleEnumerator) {
                while (await tupleEnumerator.MoveNextAsync()) {
                  tuples.Add(tupleEnumerator.Current);
                }
              }
            }
            else {
              using (tupleEnumerator) {
                while (tupleEnumerator.MoveNext()) {
                  tuples.Add(tupleEnumerator.Current);
                }
              }
            }
            tupleEnumerator = new TupleEnumerator(tuples);
          }
        }
        catch {
          FinishEnumeration(true);
          throw;
        }
      }

      private void FinishEnumeration(bool isError)
      {
        if (!enumerated) {
          provider.OnAfterEnumerate(context);
        }

        if (!isError) {
          enumerationScope?.Complete();
        }
      }

      public Tuple Current => tupleEnumerator.Current;

      object IEnumerator.Current => Current;

      void IEnumerator.Reset() => throw new NotSupportedException();

      public void Dispose()
      {
        if (state != State.New) {
          tupleEnumerator.Dispose();
        }
        enumerationScope?.Dispose();
      }

      public async ValueTask DisposeAsync()
      {
        if (state != State.New) {
          await tupleEnumerator.DisposeAsync();
        }
        enumerationScope?.Dispose();
      }

      private RecordSet(EnumerationContext context, ExecutableProvider provider, CancellationToken token = default)
      {
        this.context = context;
        this.provider = provider;
        this.token = token;
        isGreedy = context.CheckOptions(EnumerationContextOptions.GreedyEnumerator);
      }

      public static RecordSet Create(EnumerationContext context, ExecutableProvider provider)
      {
        var recordSet = new RecordSet(context, provider);
        recordSet.Prepare();
        return recordSet;
      }

      public static async ValueTask<RecordSet> CreateAsync(
        EnumerationContext context, ExecutableProvider provider, CancellationToken token)
      {
        var recordSet = new RecordSet(context, provider, token);
        await recordSet.PrepareAsync();
        return recordSet;
      }
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

  // /// <summary>
  // /// Provides access to a sequence of <see cref="Tuple"/>s
  // /// exposed by its <see cref="Provider"/>.
  // /// </summary>
  // public readonly struct RecordSet : IEnumerable<Tuple>, IAsyncEnumerable<Tuple>
  // {
  //   public readonly EnumerationContext Context;
  //   private readonly ExecutableProvider source;
  //
  //   public RecordSetHeader Header => source.Header;
  //
  //   /// <inheritdoc/>
  //   IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  //
  //   /// <inheritdoc/>
  //   IEnumerator<Tuple> IEnumerable<Tuple>.GetEnumerator() => GetEnumerator();
  //
  //   /// <inheritdoc/>
  //   IAsyncEnumerator<Tuple> IAsyncEnumerable<Tuple>.GetAsyncEnumerator(CancellationToken cancellationToken) =>
  //     GetAsyncEnumerator(cancellationToken);
  //
  //   public RecordSetEnumerator GetEnumerator() =>
  //     Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator)
  //       ? (RecordSetEnumerator) new RecordSetGreedyEnumerator(this)
  //       : new RecordSetLazyEnumerator(this);
  //
  //   public RecordSetEnumerator GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
  //     Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator)
  //       ? (RecordSetEnumerator) new RecordSetGreedyEnumerator(this, cancellationToken)
  //       : new RecordSetLazyEnumerator(this, cancellationToken);
  //
  //   public abstract class RecordSetEnumerator: IEnumerator<Tuple>, IAsyncEnumerator<Tuple>
  //   {
  //     private bool hasValue;
  //     public abstract Tuple Current { get; }
  //
  //     void IEnumerator.Reset() => throw new NotSupportedException();
  //
  //     object IEnumerator.Current => Current;
  //
  //     public bool MoveNext()
  //     {
  //       var result = hasValue;
  //       if (hasValue) {
  //         hasValue = MoveNextImpl();
  //       }
  //
  //       return result;
  //     }
  //
  //     protected abstract bool MoveNextImpl();
  //
  //     public async ValueTask<bool> MoveNextAsync()
  //     {
  //       var result = hasValue;
  //       if (hasValue) {
  //         hasValue = await MoveNextAsyncImpl();
  //       }
  //
  //       return result;
  //     }
  //
  //     internal void Initialize() => hasValue = MoveNextImpl();
  //     internal async ValueTask InitializeAsync() => hasValue = await MoveNextAsyncImpl();
  //
  //     protected abstract ValueTask<bool> MoveNextAsyncImpl();
  //
  //     public abstract void Dispose();
  //
  //     public abstract ValueTask DisposeAsync();
  //   }
  //
  //   private class RecordSetGreedyEnumerator: RecordSetEnumerator
  //   {
  //     private readonly RecordSet recordSet;
  //     private readonly CancellationToken cancellationToken;
  //
  //     private List<Tuple>.Enumerator? underlyingEnumerator;
  //
  //     public override Tuple Current => underlyingEnumerator.HasValue
  //       ? underlyingEnumerator.Value.Current
  //       : throw new InvalidOperationException("Enumeration has not been started.");
  //
  //     protected override bool MoveNextImpl()
  //     {
  //       if (underlyingEnumerator.HasValue) {
  //         return underlyingEnumerator.Value.MoveNext();
  //       }
  //
  //       var tupleList = new List<Tuple>();
  //       using var sourceEnumerator = recordSet.source.GetProviderEnumerator(recordSet.Context);
  //       while (sourceEnumerator.MoveNext()) {
  //         tupleList.Add(sourceEnumerator.Current);
  //       }
  //
  //       underlyingEnumerator = tupleList.GetEnumerator();
  //
  //       return underlyingEnumerator.Value.MoveNext();
  //     }
  //
  //     protected override async ValueTask<bool> MoveNextAsyncImpl()
  //     {
  //       if (underlyingEnumerator.HasValue) {
  //         return underlyingEnumerator.Value.MoveNext();
  //       }
  //
  //       var tupleList = new List<Tuple>();
  //       await using var sourceEnumerator = recordSet.source.GetProviderEnumerator(recordSet.Context);
  //       while (await sourceEnumerator.MoveNextAsync(cancellationToken)) {
  //         tupleList.Add(sourceEnumerator.Current);
  //       }
  //
  //       underlyingEnumerator = tupleList.GetEnumerator();
  //
  //       return underlyingEnumerator.Value.MoveNext();
  //     }
  //
  //     public override void Dispose() => underlyingEnumerator?.Dispose();
  //
  //     public override ValueTask DisposeAsync()
  //     {
  //       underlyingEnumerator?.Dispose();
  //       return default;
  //     }
  //
  //     public RecordSetGreedyEnumerator(RecordSet recordSet, CancellationToken cancellationToken = default)
  //     {
  //       this.recordSet = recordSet;
  //       this.cancellationToken = cancellationToken;
  //     }
  //   }
  //
  //   private class RecordSetLazyEnumerator: RecordSetEnumerator
  //   {
  //     private readonly RecordSet recordSet;
  //     private readonly CancellationToken cancellationToken;
  //     private readonly ExecutableProvider.ExecutableProviderEnumerator sourceEnumerator;
  //
  //     public override Tuple Current => sourceEnumerator.Current;
  //
  //     protected override bool MoveNextImpl() => sourceEnumerator.MoveNext();
  //
  //     protected override ValueTask<bool> MoveNextAsyncImpl() => sourceEnumerator.MoveNextAsync(cancellationToken);
  //
  //     public override void Dispose() => sourceEnumerator.Dispose();
  //
  //     public override ValueTask DisposeAsync() => sourceEnumerator.DisposeAsync();
  //
  //     public RecordSetLazyEnumerator(RecordSet recordSet, CancellationToken cancellationToken = default)
  //     {
  //       this.recordSet = recordSet;
  //       sourceEnumerator = recordSet.source.GetProviderEnumerator(recordSet.Context);
  //       this.cancellationToken = cancellationToken;
  //     }
  //   }
  //
  //   /// <summary>
  //   ///   Way 1: preloading all the data into memory and returning it inside this scope.
  //   /// </summary>
  //   private IEnumerator<Tuple> GetGreedyEnumerator()
  //   {
  //     using var cs = Context.BeginEnumeration();
  //
  //     var items = source.GetReader(Context).ToList();
  //
  //     foreach (var tuple in items) {
  //       yield return tuple;
  //     }
  //
  //     cs?.Complete();
  //   }
  //
  //   /// <summary>
  //   ///   Way 2: batched enumeration with periodical context activation
  //   /// </summary>
  //   private IEnumerator<Tuple> GetLazyEnumerator()
  //   {
  //     using var cs = Context.BeginEnumeration();
  //
  //     foreach (var tuple in source.GetReader(Context)) {
  //       yield return tuple;
  //     }
  //
  //     cs?.Complete();
  //   }
  //
  //   private async IAsyncEnumerator<Tuple> GetAsyncGreedyEnumerator(CancellationToken token)
  //   {
  //     using var cs = Context.BeginEnumeration();
  //     var tuples = new List<Tuple>();
  //
  //     await foreach (var tuple in source.GetReader(Context).WithCancellation(token)) {
  //       token.ThrowIfCancellationRequested();
  //       tuples.Add(tuple);
  //     }
  //
  //     foreach (var tuple in tuples) {
  //       yield return tuple;
  //     }
  //
  //     cs?.Complete();
  //   }
  //
  //   private async IAsyncEnumerator<Tuple> GetAsyncLazyEnumerator(CancellationToken token)
  //   {
  //     using var cs = Context.BeginEnumeration();
  //     await foreach (var tuple in source.GetReader(Context).WithCancellation(token)) {
  //       token.ThrowIfCancellationRequested();
  //       yield return tuple;
  //     }
  //
  //     cs?.Complete();
  //   }
  //
  //   public static async Task<RecordSet> CreateAsync(EnumerationContext enumerationContext, ExecutableProvider provider)
  //   {
  //     var recordSet = new RecordSet(enumerationContext, provider);
  //     await recordSet.Ini
  //   }
  //
  //   public static RecordSet Create(EnumerationContext enumerationContext, ExecutableProvider provider)
  //   {
  //     throw new NotImplementedException();
  //   }
  //
  //   // Constructors
  //
  //   private RecordSet(EnumerationContext context, ExecutableProvider source)
  //   {
  //     Context = context;
  //     this.source = source;
  //   }
  // }
}
