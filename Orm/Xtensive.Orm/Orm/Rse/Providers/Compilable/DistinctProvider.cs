// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.27

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Rse.Providers.Compilable
{
  /// <summary>
  /// Distinct provider
  /// </summary>
  [Serializable]
  public sealed class DistinctProvider : UnaryProvider
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    public DistinctProvider(CompilableProvider source)
      : base(ProviderType.Distinct, source)
    {
    }
  }
}