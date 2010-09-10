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
using Xtensive.Storage.Rse.Providers;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Core;

namespace Xtensive.Storage.Rse
{
  public class RecordSet : IEnumerable<Tuple>
  {
    public EnumerationContext Context { get; private set; }
    public ExecutableProvider Source { get; private set; }
    public RecordSetHeader Header { get { return Source.Header; } }

    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator()
    {
      ExecutableProvider compiled;
      using (Context.Activate()) {
        if (Context.CheckOptions(EnumerationContextOptions.GreedyEnumerator)) {
          //  Way 1: preloading all the data into memory and returning it inside this scope
          foreach (var tuple in Source.ToList())
            yield return tuple;
          yield break;
        }
      }
      //  Way 2: batched enumeration with periodical context activation
      EnumerationScope currentScope = null;
      var batched = Source.Batch(2).ApplyBeforeAndAfter(
        () => currentScope = Context.Activate(), () => currentScope.DisposeSafely());
      foreach (var batch in batched)
        foreach (var tuple in batch)
          yield return tuple;

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