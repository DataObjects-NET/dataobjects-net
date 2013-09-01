// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using Xtensive.Collections;

namespace Xtensive.Orm.Rse.Providers
{  
  /// <summary>
  /// Compilable provider that sorts the 
  /// <see cref="UnaryProvider.Source"/> by <see cref="OrderProviderBase.Order"/>.
  /// </summary>
  [Serializable]
  public sealed class SortProvider : OrderProviderBase
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="order">The <see cref="OrderProviderBase.Order"/> property value.</param>
    public SortProvider(CompilableProvider source, DirectionCollection<int> order)
      : base(ProviderType.Sort, source, order)
    {
      Initialize();
    }
  }
}
