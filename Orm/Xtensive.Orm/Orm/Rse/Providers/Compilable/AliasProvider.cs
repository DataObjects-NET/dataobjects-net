// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Collections;

using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Aliases the <see cref="UnaryProvider.Source"/> with specified <see cref="Alias"/>.
  /// </summary>
  [Serializable]
  public sealed class AliasProvider : UnaryProvider
  {
    /// <summary>
    /// Alias of the result.
    /// </summary>
    public string Alias { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return base.BuildHeader().Alias(Alias);
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return Alias;
    }


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="alias">The <see cref="Alias"/> property value.</param>
    public AliasProvider(CompilableProvider source, string alias)
      : base(ProviderType.Alias, source)
    {
      Alias = alias;
      Initialize();
    }
  }
}