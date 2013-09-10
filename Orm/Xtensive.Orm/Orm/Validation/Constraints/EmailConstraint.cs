// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;
using System.Text.RegularExpressions;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures that email address is in correct format.
  /// </summary>
  public sealed class EmailConstraint : PropertyValidator
  {
    private const string EmailPattern = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

    private static readonly Regex Validator = new Regex(EmailPattern);

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
      return isValid ? Success() : Error(Strings.ValueShouldBeAValidEMail, fieldValue);
    }

    public override IPropertyValidator CreateNew()
    {
      return new EmailConstraint {
        IsImmediate = IsImmediate,
      };
    }
  }
}