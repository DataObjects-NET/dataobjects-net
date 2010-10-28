// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.14

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that returns one record if it matches specified <see cref="Key"/> from <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public sealed class SeekProvider : UnaryProvider
  {
    private Func<Tuple> compiledKey;

    /// <summary>
    /// Seek parameter.
    /// </summary>
    public Expression<Func<Tuple>> Key { get; private set; }

    /// <summary>
    /// Gets the compiled <see cref="Key"/>.
    /// </summary>
    public Func<Tuple> CompiledKey {
      get {
        if (compiledKey==null)
          compiledKey = Key.CachingCompile();
        return compiledKey;
      }
      internal set {
        compiledKey = value;
      }
    }


    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Key.ToString(true);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="key">The <see cref="Key"/> property value.</param>
    public SeekProvider(CompilableProvider source, Expression<Func<Tuple>> key)
      : base(ProviderType.Seek, source)
    {
      Key = key;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="key">Wrapped to <see cref="Key"/> property value.</param>
    public SeekProvider(CompilableProvider source, Tuple key)
      : base(ProviderType.Seek, source)
    {
      Key = () => key;
    }
  }
}