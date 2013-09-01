// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.14

using System;
using System.Linq.Expressions;
using Xtensive.Core;

using Xtensive.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Compilable provider that returns one record if it matches specified <see cref="Key"/> from <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public sealed class SeekProvider : UnaryProvider
  {
    /// <summary>
    /// Seek parameter.
    /// </summary>
    public Func<Tuple> Key { get; private set; }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return Key.Method.ToString();
    }


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="key">The <see cref="Key"/> property value.</param>
    public SeekProvider(CompilableProvider source, Func<Tuple> key)
      : base(ProviderType.Seek, source)
    {
      Key = key;
      Initialize();
    }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="key">Wrapped to <see cref="Key"/> property value.</param>
    public SeekProvider(CompilableProvider source, Tuple key)
      : base(ProviderType.Seek, source)
    {
      Key = () => key;
      Initialize();
    }
  }
}