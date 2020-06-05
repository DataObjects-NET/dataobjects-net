// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.10

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Core;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Provides access to a sequence of <see cref="Tuple"/>s
  /// exposed by its <see cref="Provider"/>.
  /// </summary>
  public readonly struct RecordSet : IEnumerable<Tuple>, IAsyncEnumerable<Tuple>
  {
    public readonly EnumerationContext Context;
    private readonly ExecutableProvider source;

    public RecordSetHeader Header => source.Header;

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<Tuple> IEnumerable<Tuple>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    IAsyncEnumerator<Tuple> IAsyncEnumerable<Tuple>.GetAsyncEnumerator(CancellationToken cancellationToken) =>
      GetAsyncEnumerator(cancellationToken);

    public RecordSetEnumerator GetEnumerator() =>
      Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator)
        ? (RecordSetEnumerator) new RecordSetGreedyEnumerator(this)
        : new RecordSetLazyEnumerator(this);

    public RecordSetEnumerator GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
      Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator)
        ? (RecordSetEnumerator) new RecordSetGreedyEnumerator(this, cancellationToken)
        : new RecordSetLazyEnumerator(this, cancellationToken);

    public abstract class RecordSetEnumerator: IEnumerator<Tuple>, IAsyncEnumerator<Tuple>
    {
      private bool hasValue;
      public abstract Tuple Current { get; }

      void IEnumerator.Reset() => throw new NotSupportedException();

      object IEnumerator.Current => Current;

      public bool MoveNext()
      {
        var result = hasValue;
        if (hasValue) {
          hasValue = MoveNextImpl();
        }

        return result;
      }

      protected abstract bool MoveNextImpl();

      public async ValueTask<bool> MoveNextAsync()
      {
        var result = hasValue;
        if (hasValue) {
          hasValue = await MoveNextAsyncImpl();
        }

        return result;
      }

      internal void Initialize() => hasValue = MoveNextImpl();
      internal async ValueTask InitializeAsync() => hasValue = await MoveNextAsyncImpl();

      protected abstract ValueTask<bool> MoveNextAsyncImpl();

      public abstract void Dispose();

      public abstract ValueTask DisposeAsync();
    }

    private class RecordSetGreedyEnumerator: RecordSetEnumerator
    {
      private readonly RecordSet recordSet;
      private readonly CancellationToken cancellationToken;

      private List<Tuple>.Enumerator? underlyingEnumerator;

      public override Tuple Current => underlyingEnumerator.HasValue
        ? underlyingEnumerator.Value.Current
        : throw new InvalidOperationException("Enumeration has not been started.");

      protected override bool MoveNextImpl()
      {
        if (underlyingEnumerator.HasValue) {
          return underlyingEnumerator.Value.MoveNext();
        }

        var tupleList = new List<Tuple>();
        using var sourceEnumerator = recordSet.source.GetProviderEnumerator(recordSet.Context);
        while (sourceEnumerator.MoveNext()) {
          tupleList.Add(sourceEnumerator.Current);
        }

        underlyingEnumerator = tupleList.GetEnumerator();

        return underlyingEnumerator.Value.MoveNext();
      }

      protected override async ValueTask<bool> MoveNextAsyncImpl()
      {
        if (underlyingEnumerator.HasValue) {
          return underlyingEnumerator.Value.MoveNext();
        }

        var tupleList = new List<Tuple>();
        await using var sourceEnumerator = recordSet.source.GetProviderEnumerator(recordSet.Context);
        while (await sourceEnumerator.MoveNextAsync(cancellationToken)) {
          tupleList.Add(sourceEnumerator.Current);
        }

        underlyingEnumerator = tupleList.GetEnumerator();

        return underlyingEnumerator.Value.MoveNext();
      }

      public override void Dispose() => underlyingEnumerator?.Dispose();

      public override ValueTask DisposeAsync()
      {
        underlyingEnumerator?.Dispose();
        return default;
      }

      public RecordSetGreedyEnumerator(RecordSet recordSet, CancellationToken cancellationToken = default)
      {
        this.recordSet = recordSet;
        this.cancellationToken = cancellationToken;
      }
    }

    private class RecordSetLazyEnumerator: RecordSetEnumerator
    {
      private readonly RecordSet recordSet;
      private readonly CancellationToken cancellationToken;
      private readonly ExecutableProvider.ExecutableProviderEnumerator sourceEnumerator;

      public override Tuple Current => sourceEnumerator.Current;

      protected override bool MoveNextImpl() => sourceEnumerator.MoveNext();

      protected override ValueTask<bool> MoveNextAsyncImpl() => sourceEnumerator.MoveNextAsync(cancellationToken);

      public override void Dispose() => sourceEnumerator.Dispose();

      public override ValueTask DisposeAsync() => sourceEnumerator.DisposeAsync();

      public RecordSetLazyEnumerator(RecordSet recordSet, CancellationToken cancellationToken = default)
      {
        this.recordSet = recordSet;
        sourceEnumerator = recordSet.source.GetProviderEnumerator(recordSet.Context);
        this.cancellationToken = cancellationToken;
      }
    }

    /// <summary>
    ///   Way 1: preloading all the data into memory and returning it inside this scope.
    /// </summary>
    private IEnumerator<Tuple> GetGreedyEnumerator()
    {
      using var cs = Context.BeginEnumeration();

      var items = source.GetReader(Context).ToList();

      foreach (var tuple in items) {
        yield return tuple;
      }

      cs?.Complete();
    }

    /// <summary>
    ///   Way 2: batched enumeration with periodical context activation
    /// </summary>
    private IEnumerator<Tuple> GetLazyEnumerator()
    {
      using var cs = Context.BeginEnumeration();

      foreach (var tuple in source.GetReader(Context)) {
        yield return tuple;
      }

      cs?.Complete();
    }

    private async IAsyncEnumerator<Tuple> GetAsyncGreedyEnumerator(CancellationToken token)
    {
      using var cs = Context.BeginEnumeration();
      var tuples = new List<Tuple>();

      await foreach (var tuple in source.GetReader(Context).WithCancellation(token)) {
        token.ThrowIfCancellationRequested();
        tuples.Add(tuple);
      }

      foreach (var tuple in tuples) {
        yield return tuple;
      }

      cs?.Complete();
    }

    private async IAsyncEnumerator<Tuple> GetAsyncLazyEnumerator(CancellationToken token)
    {
      using var cs = Context.BeginEnumeration();
      await foreach (var tuple in source.GetReader(Context).WithCancellation(token)) {
        token.ThrowIfCancellationRequested();
        yield return tuple;
      }

      cs?.Complete();
    }

    public static async Task<RecordSet> CreateAsync(EnumerationContext enumerationContext, ExecutableProvider provider)
    {
      var recordSet = new RecordSet(enumerationContext, provider);
      await recordSet.Ini
    }

    public static RecordSet Create(EnumerationContext enumerationContext, ExecutableProvider provider)
    {
      throw new NotImplementedException();
    }

    // Constructors

    private RecordSet(EnumerationContext context, ExecutableProvider source)
    {
      Context = context;
      this.source = source;
    }
  }
}