// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Collections;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures field length (or item count) fits in specified range.
  /// </summary>
  public sealed class LengthConstraint : PropertyValidator
  {
    /// <summary>
    /// Gets or sets the minimal allowed length. Default is 0.
    /// </summary>
    public int Min { get; set; }

    /// <summary>
    /// Gets or sets the maximal allowed length. Default is <see cref="int.MaxValue"/>.
    /// </summary>
    public int Max { get; set; }

    public override void Configure(Domain domain, TypeInfo type, FieldInfo field)
    {
      base.Configure(domain, type, field);
      if (field.ValueType!=typeof (string) && typeof (ICollection).IsAssignableFrom(field.ValueType))
        ThrowConfigurationError(string.Format(Strings.FieldShouldBeOfTypeX, typeof (string).FullName));
      if (Max==int.MaxValue && Min==0)
        ThrowConfigurationError(Strings.MaxOrMinPropertyShouldBeSpecified);
    }

    public override ValidationResult Validate(Entity target, object fieldValue)
    {
      if (fieldValue==null)
        return Success();

      int length;

      if (fieldValue is string)
        length = ((string) fieldValue).Length;
      else
        length = ((ICollection) fieldValue).Count;

      if (length >= Min && length <= Max)
        return Success();

      if (Min==0)
        return Error(string.Format(Strings.ValueLengthCanNotBeGreaterThanX, Max), fieldValue);

      if (Max==int.MaxValue)
        return Error(string.Format(Strings.ValueLengthCanNotBeLessThanX, Min), fieldValue);

      return Error(string.Format(Strings.ValueLengthCanNotBeLessThanXOrGreaterThanY, Min, Max), fieldValue);
    }

    public override IPropertyValidator CreateNew()
    {
      return new LengthConstraint {
        IsImmediate = IsImmediate,
        Min = Min,
        Max = Max,
        SkipOnTransactionComitting = SkipOnTransactionComitting
      };
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public LengthConstraint()
    {
      Min = 0;
      Max = int.MaxValue;
    }
  }
}