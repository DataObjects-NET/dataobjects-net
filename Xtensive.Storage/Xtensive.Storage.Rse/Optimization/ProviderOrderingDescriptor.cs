// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.24

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Optimization
{
  /// <summary>
  /// Descriptor of a provider's ordering behavior.
  /// </summary>
  [Serializable]
  public struct ProviderOrderingDescriptor
  {
    /// <summary>
    /// Provider performs ordering of records.
    /// </summary>
    public readonly bool IsOrdering;

    /// <summary>
    /// Gets a value indicating whether this instance is sensitive to records order.
    /// </summary>
    public readonly bool IsOrderSensitive;

    /// <summary>
    /// Gets a value indicating whether this instance preserves records order.
    /// </summary>
    public readonly bool PreservesOrder;

    /// <summary>
    /// Gets a value indicating whether records order must be kept after this instance.
    /// </summary>
    public readonly bool ResetsOrdering;

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="isOrdering">value of <see cref="IsOrdering"/>.</param>
    /// <param name="isOrderSensitive">value of <see cref="IsOrderSensitive"/>.</param>
    /// <param name="preservesOrder">value of <see cref="PreservesOrder"/>.</param>
    /// <param name="resetsOrdering">value of <see cref="ResetsOrdering"/>.</param>
    public ProviderOrderingDescriptor(bool isOrdering, bool isOrderSensitive, bool preservesOrder,
      bool resetsOrdering)
    {
      if (isOrdering && !preservesOrder)
        throw new ArgumentException(
          String.Format(Strings.ExValueOfParameterWCantBeXIfValueOfParameterYIsZ, "preservesOrder",
          false, "isOrdering", true));
      if (isOrdering && resetsOrdering)
        throw new ArgumentException(
          String.Format(Strings.ExValueOfParameterWCantBeXIfValueOfParameterYIsZ, "resetsOrdering",
          true, "isOrdering", true));
      IsOrdering = isOrdering;
      IsOrderSensitive = isOrderSensitive;
      PreservesOrder = preservesOrder;
      ResetsOrdering = resetsOrdering;
    }
  }
}