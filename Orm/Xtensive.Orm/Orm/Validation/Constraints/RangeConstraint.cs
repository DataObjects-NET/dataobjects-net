// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using Xtensive.Comparison;
using Xtensive.Orm.Model;
using Xtensive.Reflection;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures field value fits in the specified range.
  /// </summary>
  public sealed class RangeConstraint : PropertyValidator
  {
    private abstract class ValidationHandler
    {
      protected abstract void Configure(object min, object max);

      public abstract bool Validate(object value);

      public static ValidationHandler Create(Type valueType, object min, object max)
      {
        var validatorType = typeof (ValidationHandler<>).MakeGenericType(valueType);
        var result = (ValidationHandler) Activator.CreateInstance(validatorType);
        result.Configure(min, max);
        return result;
      }
    }

    private sealed class ValidationHandler<TValue> : ValidationHandler
    {
      private Func<TValue, TValue, int> comparer;

      private bool hasMin;
      private bool hasMax;

      private TValue min;
      private TValue max;

      protected override void Configure(object minObj, object maxObj)
      {
        comparer = AdvancedComparer<TValue>.System.Compare;

        if (minObj!=null) {
          min = (TValue) Convert.ChangeType(minObj, typeof (TValue));
          hasMin = true;
        }

        if (maxObj!=null) {
          max = (TValue) Convert.ChangeType(maxObj, typeof (TValue));
          hasMax = true;
        }
      }

      public override bool Validate(object value)
      {
        var typedValue = (TValue) value;

        return (!hasMin || comparer.Invoke(typedValue, min) >= 0)
          && (!hasMax || comparer.Invoke(typedValue, max) <= 0);
      }
    }

    private ValidationHandler handler;

    /// <summary>
    /// Gets or sets the minimal allowed value.
    /// <see langword="null" /> means "ignore this boundary".
    /// Default value is <see langword="null" />.
    /// </summary>
    public object Min { get; set; }

    /// <summary>
    /// Gets or sets the maximal allowed value.
    /// <see langword="null" /> means "ignore this boundary".
    /// Default value is <see langword="null" />.
    /// </summary>
    public object Max { get; set; }

    public override void Configure(Domain domain, TypeInfo type, FieldInfo field)
    {
      base.Configure(domain, type, field);

      if (Min==null && Max==null)
        ThrowConfigurationError(Strings.MaxOrMinPropertyShouldBeSpecified);

      if (!field.ValueType.IsOfGenericInterface(typeof (IComparable<>)))
        ThrowConfigurationError(Strings.FieldShouldBeOfComparableType);

      handler = ValidationHandler.Create(field.ValueType, Min, Max);
    }

    public override ValidationResult Validate(Entity target, object fieldValue)
    {
      if (handler.Validate(fieldValue))
        return Success();
      if (Min==null)
        return Error(string.Format(Strings.ValueShouldNotBeGreaterThanMax, Max), fieldValue);
      if (Max==null)
        return Error(string.Format(Strings.ValueShouldNotBeLessThanMin, Min), fieldValue);
      return Error(string.Format(Strings.ValueShouldNotBeLessThanMinOrGreaterThanMax, Min, Max), fieldValue);
    }

    public override IPropertyValidator CreateNew()
    {
      return new RangeConstraint {
        IsImmediate = IsImmediate,
        Min = Min,
        Max = Max,
      };
    }
  }
}