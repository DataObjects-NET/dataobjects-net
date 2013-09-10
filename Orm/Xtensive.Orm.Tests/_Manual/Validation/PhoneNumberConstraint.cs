// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.28

using System;
using System.Text.RegularExpressions;
using Xtensive.Orm.Model;
using Xtensive.Orm.Validation;

namespace Xtensive.Orm.Tests._Manual.Validation
{
  [Serializable]
  public class PhoneNumberConstraint : PropertyValidator
  {
    private const string PhoneNumberPattern = "^[2-9]\\d{2}-\\d{3}-\\d{4}$";

    private static readonly Regex Validator = new Regex(PhoneNumberPattern);

    public override void Configure(Domain domain, TypeInfo type, FieldInfo field)
    {
      base.Configure(domain, type, field);

      if (field.ValueType!=typeof (string))
        ThrowConfigurationError(string.Format(Strings.FieldShouldBeOfTypeX, typeof (string).FullName));
    }

    public override ValidationResult Validate(Entity target, object fieldValue)
    {
      var value = (string) fieldValue;
      var isValid = string.IsNullOrEmpty(value) || Validator.IsMatch(value);
      return isValid ? Success() : Error("Phone number is incorrect", fieldValue);
    }

    public override IPropertyValidator CreateNew()
    {
      return new EmailConstraint {
        IsImmediate = IsImmediate,
      };
    }
  }
}