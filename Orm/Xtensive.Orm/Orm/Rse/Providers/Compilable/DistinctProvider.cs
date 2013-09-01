// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.27

using System;


namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Distinct provider
  /// </summary>
  [Serializable]
  public sealed class DistinctProvider : UnaryProvider
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    public DistinctProvider(CompilableProvider source)
      : base(ProviderType.Distinct, source)
    {
      Initialize();
    }
  }
}