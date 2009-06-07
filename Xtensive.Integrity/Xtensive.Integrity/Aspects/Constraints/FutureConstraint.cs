// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Ensures that date value is in the future.
  /// </summary>
  [Serializable]
  public class FutureConstraint : PropertyConstraintAspect
  {
    /// <inheritdoc/>
    public override bool CheckValue(object value)
    {
      return (DateTime) value > DateTime.Now;
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return valueType == typeof (DateTime);
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessage()
    {
      return Strings.ConstraintMessageValueMustBeInTheFuture;
    }
  }
}