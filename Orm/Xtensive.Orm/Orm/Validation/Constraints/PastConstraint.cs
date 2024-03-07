// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.05.27

using Xtensive.Reflection;

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
      if (valueType!=WellKnownTypes.DateTime && valueType!=WellKnownTypes.NullableDateTime)
        ThrowConfigurationError(string.Format(Strings.FieldShouldBeOfTypeX, WellKnownTypes.DateTime.FullName));
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
        SkipOnTransactionCommit = SkipOnTransactionCommit,
        ValidateOnlyIfModified = ValidateOnlyIfModified
      };
    }
  }
}