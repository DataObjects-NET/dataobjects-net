// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.24

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization
{
  /// <summary>
  /// Descriptor of a provider's ordering behavior.
  /// </summary>
  [Serializable]
  public struct ProviderOrderingDescriptor
  {
    /// <summary>
    /// Gets a value indicating whether the <see cref="CompilableProvider"/> is sensitive to records order.
    /// </summary>
    public readonly bool IsOrderSensitive;

    /// <summary>
    /// Gets a value indicating whether the <see cref="CompilableProvider"/> preserves records order.
    /// </summary>
    public readonly bool PreservesOrder;

    /// <summary>
    /// Gets a value indicating whether we should try to correct order of records 
    /// which are processed by the <see cref="CompilableProvider"/>.
    /// </summary>
    public readonly bool IsOrderingBoundary;

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="isOrderSensitive">value of <see cref="IsOrderSensitive"/>.</param>
    /// <param name="preservesOrder">value of <see cref="PreservesOrder"/>.</param>
    /// <param name="isOrderingBoundary">value of <see cref="IsOrderingBoundary"/>.</param>
    public ProviderOrderingDescriptor(bool isOrderSensitive, bool preservesOrder, bool isOrderingBoundary)
    {
      IsOrderSensitive = isOrderSensitive;
      PreservesOrder = preservesOrder;
      IsOrderingBoundary = isOrderingBoundary;
    }
  }
}