// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures that property value is not <see cref="string.Empty"/>.
  /// </summary>
  [Serializable]
  public sealed class NotEmptyConstraint : PropertyConstraintAspect
  {
    /// <inheritdoc/>
    public override bool CheckValue(object value)
    {
      return (string) value!=string.Empty;
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return valueType==typeof (string);
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessage()
    {
      return Strings.ConstraintMessageValueCanNotBeEmpty;
    }
  }
}