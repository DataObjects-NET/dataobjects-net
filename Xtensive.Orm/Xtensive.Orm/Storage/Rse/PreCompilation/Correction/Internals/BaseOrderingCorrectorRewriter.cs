// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.28

using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  /// <summary>
  /// Abstract base class for correctors of ordering of records.
  /// </summary>
  public abstract class BaseOrderingCorrectorRewriter : CompilableProviderVisitor
  {
    /// <summary>
    /// Sets the actual ordering for <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="ordering">The actual ordering.</param>
    protected void SetActualOrdering(CompilableProvider provider, DirectionCollection<int> ordering)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      ArgumentValidator.EnsureArgumentNotNull(ordering, "ordering");

      // TODO: Implement protection against hackers
      provider.SetActualOrdering(ordering);
    }
  }
}