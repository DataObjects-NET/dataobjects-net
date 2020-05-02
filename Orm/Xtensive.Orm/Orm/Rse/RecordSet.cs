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
  public class RecordSet : IEnumerable<Tuple>, IAsyncEnumerable<Tuple>
  {
    public EnumerationContext Context { get; private set; }
    public ExecutableProvider Source { get; private set; }
    public RecordSetHeader Header { get { return Source.Header; } }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator() =>
      Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator) ? GetGreedyEnumerator() : GetBatchedEnumerator();

    /// <inheritdoc/>
    public IAsyncEnumerator<Tuple> GetAsyncEnumerator(CancellationToken cancellationToken) =>
      Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator)
        ? GetAsyncGreedyEnumerator(cancellationToken)
        : GetAsyncLazyEnumerator(cancellationToken);

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public async Task<IEnumerator<Tuple>> GetEnumeratorAsync(CancellationToken token)
    {
      if (Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator))
        return await GetGreedyEnumeratorAsync(token).ConfigureAwait(false);
      return await GetBatchedEnumeratorAsync(token).ConfigureAwait(false);
    }

    /// <summary>
    ///   Way 1: preloading all the data into memory and returning it inside this scope.
    /// </summary>
    private IEnumerator<Tuple> GetGreedyEnumerator()
    {
      using (var cs = Context.BeginEnumeration()) {
        List<Tuple> items;

        using (Context.Activate())
          items = Source.ToList();

        foreach (var tuple in items)
          yield return tuple;

        if (cs!=null)
          cs.Complete();
      }
    }

    /// <summary>
    ///   Way 2: batched enumeration with periodical context activation
    /// </summary>
    private IEnumerator<Tuple> GetBatchedEnumerator()
    {
      EnumerationScope currentScope = null;
      var batched = Source.Batch(2).ApplyBeforeAndAfter(
        () => currentScope = Context.Activate(),
        () => currentScope.DisposeSafely());
      using (var cs = Context.BeginEnumeration()) {
        foreach (var batch in batched)
          foreach (var tuple in batch)
            yield return tuple;
        if (cs!=null)
          cs.Complete();
      }
    }

    /// <summary>
    /// Way 1: asynchroniously preloading all the data into memory and returning it inside this scope.
    /// </summary>
    private async Task<IEnumerator<Tuple>> GetGreedyEnumeratorAsync(CancellationToken token)
    {
      var cs = Context.BeginEnumeration();
      List<Tuple> items;
      Action<object> afterEnumrationAction =
        o => {
          if (o==null)
            return;
          var completableScope = (ICompletableScope)o;
          completableScope.Complete();
          completableScope.Dispose();
        };

      using (Context.Activate()) {
        var enumerator = await Source.GetEnumeratorAsync(Context, token).ConfigureAwait(false);
        items = enumerator.ToEnumerable().ToList();
      }

      return items.ToEnumerator(afterEnumrationAction, cs);
    }

    /// <summary>
    /// Way 2: batched async enumeration.
    /// </summary>
    private async Task<IEnumerator<Tuple>> GetBatchedEnumeratorAsync(CancellationToken token)
    {
      var enumerator = await Source.GetEnumeratorAsync(Context, token).ConfigureAwait(false);
      var batched = enumerator.ToEnumerable().Batch(2);

      var cs = Context.BeginEnumeration();
      Action<object> afterEnumerationAction =
        o => {
          if (o==null)
            return;
          var completableScope = (ICompletableScope)o;
          completableScope.Complete();
          completableScope.Dispose();
        };
      return GetTwoLevelEnumerator(batched, afterEnumerationAction, cs);
    }

    private static IEnumerator<Tuple> GetTwoLevelEnumerator(IEnumerable<IEnumerable<Tuple>> enumerable, Action<object> afterEnumerationAction, object parameterForAction)
    {
      try {
        foreach (var batch in enumerable)
          foreach (var tuple in batch) {
            yield return tuple;
          }
      }
      finally {
        afterEnumerationAction.Invoke(parameterForAction);
      }
    }

    private async IAsyncEnumerator<Tuple> GetAsyncGreedyEnumerator(CancellationToken token)
    {
      using var cs = Context.BeginEnumeration();
      var tuples = new List<Tuple>();

      using (Context.Activate()) {
        await foreach (var tuple in Source) {
          token.ThrowIfCancellationRequested();
          tuples.Add(tuple);
        }
      }

      foreach (var tuple in tuples) {
        yield return tuple;
      }

      cs?.Complete();
    }

    private async IAsyncEnumerator<Tuple> GetAsyncLazyEnumerator(CancellationToken token)
    {
      using var cs = Context.BeginEnumeration();
      using (Context.Activate()) {
        await foreach (var tuple in Source) {
          token.ThrowIfCancellationRequested();
          yield return tuple;
        }
      }

      cs?.Complete();
    }

    // Constructors

    public RecordSet(EnumerationContext context, ExecutableProvider source)
    {
      Context = context;
      Source = source;
    }
  }
}