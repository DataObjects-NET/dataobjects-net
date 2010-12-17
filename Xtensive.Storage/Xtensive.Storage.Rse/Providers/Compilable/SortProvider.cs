// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Compilable
{  
  /// <summary>
  /// Compilable provider that sorts the 
  /// <see cref="UnaryProvider.Source"/> by <see cref="OrderProviderBase.Order"/>.
  /// </summary>
  [Serializable]  
  public sealed class SortProvider : OrderProviderBase
  {
    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
//      SetActualOrdering(ExpectedOrder);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="order">The <see cref="OrderProviderBase.Order"/> property value.</param>
    public SortProvider(CompilableProvider source, DirectionCollection<int> order)
      : base(ProviderType.Sort, source, order)
    {
    }
  }
}
