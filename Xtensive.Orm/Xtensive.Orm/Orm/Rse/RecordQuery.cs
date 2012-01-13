// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.08.22

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Provides a wrapper for <see cref="Provider"/>, as well as wide
  /// range of extension methods (see <see cref="RecordQueryExtensions"/>)
  /// to operate with them.
  /// </summary>
  [Serializable]
  public sealed class RecordQuery
  {
    /// <summary>
    /// Gets the header of the <see cref="RecordQuery"/>.
    /// </summary>
    public RecordSetHeader Header
    {
      get { return Provider.Header; }
    }

    /// <summary>
    /// Gets the provider this <see cref="RecordQuery"/> is bound to.
    /// </summary>
    public CompilableProvider Provider { get; private set; }

    /// <summary>
    /// Creates <see cref="StoreProvider"/> with specified <see cref="RecordSetHeader"/>
    /// and name for saved context data .
    /// </summary>
    /// <param name="header">The result header.</param>
    /// <param name="scope">The result scope.</param>
    /// <param name="name">The result name.</param>
    public static RecordQuery Load(RecordSetHeader header, TemporaryDataScope scope, string name)
    {
      return new StoreProvider(header, scope, name).Result;
    }

//    #region IEnumerable<...> methods
//
//    /// <inheritdoc/>
//    public IEnumerator<Tuple> GetEnumerator()
//    {
//      EnumerationContext ctx;
//      ExecutableProvider compiled;
//      using (EnumerationScope.Open()) {
//        ctx = EnumerationContext.Current;
//        var compilationContext = CompilationContext.Current;
//        if (compilationContext==null)
//          throw new InvalidOperationException();
//        compiled = compilationContext.Compile(Provider);
//        if (ctx.CheckOptions(EnumerationContextOptions.GreedyEnumerator)) {
          // Way 1: preloading all the data into memory and returning it inside this scope
//          foreach (var tuple in compiled.ToList())
//            yield return tuple;
//          yield break;
//        }
//      }
      // Way 2: batched enumeration with periodical context activation
//      EnumerationScope currentScope = null;
//      var batched = compiled.Batch(2).ApplyBeforeAndAfter(
//        () => currentScope = ctx.Activate(), () => currentScope.DisposeSafely());
//      foreach (var batch in batched)
//        foreach (var tuple in batch)
//          yield return tuple;
//    }
//
//    /// <inheritdoc/>
//    IEnumerator IEnumerable.GetEnumerator()
//    {
//      return GetEnumerator();
//    }
//
//    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Provider.ToString();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider"><see cref="Provider"/> property value.</param>
    internal RecordQuery(CompilableProvider provider)
    {
      Provider = provider;
    }
  }
}