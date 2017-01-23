// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures that date value is in the past.
  /// </summary>
  public sealed class PastConstraint : PropertyValidator
  {
    public override void Configure(Domain domain, Model.TypeInfo type, Model.FieldInfo field)
    {
      base.Configure(domain, type, field);

      var valueType = field.ValueType;
      if (valueType!=typeof (DateTime) && valueType!=typeof (DateTime?))
        ThrowConfigurationError(string.Format(Strings.FieldShouldBeOfTypeX, typeof (DateTime).FullName));
    }

    public override ValidationResult Validate(Entity target, object fieldValue)
    {
      var isValid = fieldValue==null || (DateTime) fieldValue <= DateTime.Now;
      return isValid ? Success() : Error(Strings.ValueShouldBeInThePast, fieldValue);
    }

    public override IPropertyValidator CreateNew()
    {
      return new PastConstraint {
        IsImmediate = IsImmediate,
        SkipOnTransactionComitting = SkipOnTransactionComitting
      };
    }
  }
}