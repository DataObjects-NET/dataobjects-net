// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.09

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Linq;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Enumerates specified array of <see cref="Tuple"/> instances.
  /// </summary>
  [Serializable]
  public sealed class RawProvider : LocationAwareProvider
  {
    private readonly RecordSetHeader header;
    private Func<IEnumerable<Tuple>> compiledSource;

    /// <summary>
    /// Raw data source - an array of tuples.
    /// </summary>
    public Expression<Func<IEnumerable<Tuple>>> Source { get; private set; }

    /// <summary>
    /// Gets the compiled <see cref="Source"/>.
    /// </summary>
    public Func<IEnumerable<Tuple>> CompiledSource {
      get {
        if (compiledSource==null)
          compiledSource = Source.CachingCompile();
        return compiledSource;
      }
    }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Source.ToString(true);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="header">The <see cref="Provider.Header"/> property value.</param>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    public RawProvider(RecordSetHeader header, Expression<Func<IEnumerable<Tuple>>> source)
      : base(ProviderType.Raw, RseCompiler.DefaultClientLocation)
    {
      Source = source;
      this.header = header;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="header">The <see cref="Provider.Header"/> property value.</param>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    public RawProvider(RecordSetHeader header, IEnumerable<Tuple> source)
      : this(header, () => source)
    {
    }
  }
}