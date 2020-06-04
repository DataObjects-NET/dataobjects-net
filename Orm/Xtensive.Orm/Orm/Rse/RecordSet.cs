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
    private readonly ExecutableProvider source;
    public readonly EnumerationContext Context;

    public RecordSetHeader Header => source.Header;

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator() =>
      Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator) ? GetGreedyEnumerator() : GetLazyEnumerator();

    /// <inheritdoc/>
    public IAsyncEnumerator<Tuple> GetAsyncEnumerator(CancellationToken cancellationToken) =>
      Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator)
        ? GetAsyncGreedyEnumerator(cancellationToken)
        : GetAsyncLazyEnumerator(cancellationToken);

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
      throw new NotImplementedException();
    }

    public static RecordSet Create(EnumerationContext enumerationContext, ExecutableProvider provider)
    {
      throw new NotImplementedException();
    }

    // Constructors

    public RecordSet(EnumerationContext context, ExecutableProvider source)
    {
      Context = context;
      this.source = source;
    }
  }
}