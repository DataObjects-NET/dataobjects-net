// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.08.22

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Provides access to a sequence of <see cref="Tuple"/>s
  /// exposed by its <see cref="Provider"/>, as well as wide
  /// range of extension methods (see <see cref="RecordSetExtensions"/>)
  /// to operate with them.
  /// </summary>
  [Serializable]
  public sealed class RecordSet : IEnumerable<Tuple>
  {
    /// <summary>
    /// Gets the header of the <see cref="RecordSet"/>.
    /// </summary>
    public RecordSetHeader Header
    {
      get { return Provider.Header; }
    }

    /// <summary>
    /// Gets the provider this <see cref="RecordSet"/> is bound to.
    /// </summary>
    public CompilableProvider Provider { get; private set; }

    /// <summary>
    /// Creates <see cref="StoreProvider"/> with specified <see cref="RecordSetHeader"/>
    /// and name for saved context data .
    /// </summary>
    /// <param name="header">The result header.</param>
    /// <param name="scope">The result scope.</param>
    /// <param name="name">The result name.</param>
    public static RecordSet Load(RecordSetHeader header, TemporaryDataScope scope, string name)
    {
      return new StoreProvider(header, scope, name).Result;
    }

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator()
    {
      EnumerationContext ctx;
      ExecutableProvider compiled;
      using (EnumerationScope.Open()) {
        ctx = EnumerationContext.Current;
        var compilationContext = CompilationContext.Current;
        if (compilationContext==null)
          throw new InvalidOperationException();
        compiled = compilationContext.Compile(Provider);
        if (ctx.PreloadEnumerator) {
          // If MARS not supported, enumerate all data into memory first.
          foreach (var tuple in compiled.ToList())
            yield return tuple;
          yield break;
        }
      }
      EnumerationScope currentScope = null;
      var batched = compiled.Batch(2).ApplyBeforeAndAfter(
        () => currentScope = ctx.Activate(), () => currentScope.DisposeSafely());
      foreach (var batch in batched)
        foreach (var tuple in batch)
          yield return tuple;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

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
    internal RecordSet(CompilableProvider provider)
    {
      Provider = provider;
    }
  }
}