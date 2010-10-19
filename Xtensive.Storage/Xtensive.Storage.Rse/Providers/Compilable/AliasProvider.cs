// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
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
    public override string ParametersToString()
    {
      return Alias;
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="alias">The <see cref="Alias"/> property value.</param>
    public AliasProvider(CompilableProvider source, string alias)
      : base(ProviderType.Alias, source)
    {
      Alias = alias;
    }
  }
}