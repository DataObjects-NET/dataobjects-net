// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.10

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Core;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Provides access to a sequence of <see cref="Tuple"/>s
  /// exposed by its <see cref="Provider"/>.
  /// </summary>
  public class RecordSet : IEnumerable<Tuple>
  {
    public EnumerationContext Context { get; private set; }
    public ExecutableProvider Source { get; private set; }
    public RecordSetHeader Header { get { return Source.Header; } }

    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator()
    {
      if (Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator))
        return GetGreedyEnumerator();
      return GetBatchedEnumerator();
    }

    /// <summary>
    ///   Way 1: preloading all the data into memory and returning it inside this scope.
    /// </summary>
    private IEnumerator<Tuple> GetGreedyEnumerator()
    {
      using (var cs = Context.BeginEnumeration())
      using (Context.Activate()) {
        foreach (var tuple in Source.ToList())
          yield return tuple;
        if (cs != null)
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
        if (cs != null)
          cs.Complete();
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    public RecordSet(EnumerationContext context, ExecutableProvider source)
    {
      Context = context;
      Source = source;
    }
  }
}