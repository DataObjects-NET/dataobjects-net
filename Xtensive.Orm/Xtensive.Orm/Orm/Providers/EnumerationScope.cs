// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using System;
using Xtensive.Disposing;
using Xtensive.Storage.Rse.Providers;
using RseEnumerationScope = Xtensive.Storage.Rse.Providers.EnumerationScope;
using RseEnumerationContext = Xtensive.Storage.Rse.Providers.EnumerationContext;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// An implementation of <see cref="Storage.Rse.Providers.EnumerationScope"/> 
  /// suitable for storage.
  /// </summary>
  public class EnumerationScope : RseEnumerationScope
  {
    // Constructors

    /// <inheritdoc/>
    public EnumerationScope(RseEnumerationContext context)
      : base(context)
    {
    }
  }
}