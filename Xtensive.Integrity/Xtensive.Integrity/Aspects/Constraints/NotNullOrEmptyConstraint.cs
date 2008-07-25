// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Checks field value to be not null or empty.
  /// </summary>
  [Serializable]
  public class NotNullOrEmptyConstraintAttribute : FieldConstraintAspect
  {
    /// <inheritdoc/>
    public override void ValidateValue(IValidatable target, object value)
    {
      string stringValue = (string) value;
      
      if (string.IsNullOrEmpty(stringValue))
        throw new ConstraintViolationException(
          string.Format(Resources.Strings.ValueCanNotBeEmpty));
    }

    /// <inheritdoc/>
    public override bool IsTypeSupported(Type type)
    {
      return type == typeof (string);
    }
  }
}