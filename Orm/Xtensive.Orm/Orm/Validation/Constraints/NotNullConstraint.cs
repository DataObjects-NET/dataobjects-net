// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures property value is not <see langword="null" />.
  /// </summary>
  public sealed class NotNullConstraint : PropertyValidator
  {
    public override ValidationResult Validate(Entity target, object fieldValue)
    {
      return fieldValue!=null ? Success() : Error(Strings.ValueShouldNotBeNull, fieldValue);
    }

    public override IPropertyValidator CreateNew()
    {
      return new NotNullConstraint {
        IsImmediate = IsImmediate,
        SkipOnTransactionComitting = SkipOnTransactionComitting
      };
    }
  }
}