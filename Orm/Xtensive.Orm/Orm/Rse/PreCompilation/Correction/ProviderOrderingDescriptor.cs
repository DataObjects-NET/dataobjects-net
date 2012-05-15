// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.24

using System;

using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Rse.PreCompilation.Correction
{
  /// <summary>
  /// Descriptor of a provider's ordering behavior.
  /// </summary>
  [Serializable]
  public struct ProviderOrderingDescriptor
  {
    /// <summary>
    /// Gets a value indicating whether the <see cref="CompilableProvider"/> 
    /// is sensitive to records order.
    /// </summary>
    public readonly bool IsOrderSensitive;

    /// <summary>
    /// Gets a value indicating whether the <see cref="CompilableProvider"/> 
    /// preserves records order.
    /// </summary>
    public readonly bool PreservesOrder;

    /// <summary>
    /// Gets a value indicating whether the provider is order breaker,
    /// such as <see cref="UnionProvider"/> or <see cref="ConcatProvider"/>.
    /// </summary>
    public readonly bool BreaksOrder;

    /// <summary>
    /// Gets a value indicating whether the <see cref="CompilableProvider"/> 
    /// sorts records.
    /// </summary>
    public readonly bool IsSorter;


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="isOrderSensitive">value of <see cref="IsOrderSensitive"/>.</param>
    /// <param name="preservesOrder">value of <see cref="PreservesOrder"/>.</param>
    /// <param name="isOrderingBoundary">value of <see cref="BreaksOrder"/>.</param>
    /// <param name="isSorter">value of <see cref="IsSorter"/>.</param>
    public ProviderOrderingDescriptor(bool isOrderSensitive, bool preservesOrder, bool isOrderingBoundary, bool isSorter)
    {
      IsOrderSensitive = isOrderSensitive;
      PreservesOrder = preservesOrder;
      BreaksOrder = isOrderingBoundary;
      IsSorter = isSorter;
    }
  }
}