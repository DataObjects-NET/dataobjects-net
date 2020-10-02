// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;
using System.Text.RegularExpressions;
using Xtensive.Orm.Model;
using Xtensive.Reflection;

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

      if (field.ValueType!=WellKnownTypes.String)
        ThrowConfigurationError(string.Format(Strings.FieldShouldBeOfTypeX, WellKnownTypes.String.FullName));
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
        SkipOnTransactionCommit = SkipOnTransactionCommit,
        ValidateOnlyIfModified = ValidateOnlyIfModified
      };
    }
  }
}