// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.05.27

using System;
using Xtensive.Orm.Model;
using Xtensive.Reflection;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures that property value is not <see cref="string.Empty"/>.
  /// </summary>
  public sealed class NotEmptyConstraint : PropertyValidator
  {
    public override void Configure(Domain domain, TypeInfo type, FieldInfo field)
    {
      base.Configure(domain, type, field);

      if (field.ValueType!=WellKnownTypes.String)
        ThrowConfigurationError(string.Format(Strings.FieldShouldBeOfTypeX, WellKnownTypes.String.FullName));
    }

    public override ValidationResult Validate(Entity target, object fieldValue)
    {
      var value = (string) fieldValue;
      return value!=string.Empty ? Success() : Error(Strings.ValueShouldNotBeEmpty, fieldValue);
    }

    public override IPropertyValidator CreateNew()
    {
      return new NotEmptyConstraint {
        IsImmediate = IsImmediate,
        SkipOnTransactionCommit = SkipOnTransactionCommit,
        ValidateOnlyIfModified = ValidateOnlyIfModified
      };
    }
  }
}