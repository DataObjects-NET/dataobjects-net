// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using System;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Rse.Providers.Compilable
{
  /// <summary>
  /// Builds an index over the <see cref="UnaryProvider.Source"/> ordering 
  /// them by <see cref="OrderProviderBase.Order"/>.
  /// </summary>
  [Serializable]
  public sealed class ReindexProvider : OrderProviderBase
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="order">The <see cref="OrderProviderBase.Order"/> property value.</param>
    public ReindexProvider(CompilableProvider source, DirectionCollection<int> order)
      : base(ProviderType.Reindex, source, order)
    {
    }
  }
}