// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures field value fits in the specified range.
  /// </summary>
  [Serializable]
  public sealed class RangeConstraint : PropertyConstraintAspect
  {
    private abstract class RangeValidator
    {
      protected abstract void Initialize(object min, object max);

      public abstract bool Validate(object value);

      public static RangeValidator Create(Type valueType, object min, object max)
      {
        var validatorType = typeof (RangeValidatorImplementation<>).MakeGenericType(valueType);
        var result = (RangeValidator) Activator.CreateInstance(validatorType);
        result.Initialize(min, max);
        return result;
      }
    }

    private sealed class RangeValidatorImplementation<TValue> : RangeValidator
    {
      private Func<TValue, TValue, int> comparer;

      private bool hasMin;
      private bool hasMax;

      private TValue min;
      private TValue max;

      protected override void Initialize(object minObj, object maxObj)
      {
        if (!typeof (IComparable<TValue>).IsAssignableFrom(typeof (TValue)))
          throw new ArgumentException(string.Format(Strings.ExComparerForTypeXIsNotFound, typeof (TValue)));

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

    private const string MinParameter = "Min";
    private const string MaxParameter = "Max";

    [NonSerialized]
    private RangeValidator nonGenericValidator;

    [NonSerialized]
    private ThreadSafeDictionary<Type, RangeValidator> genericValidators;

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

    /// <inheritdoc/>
    public override bool CheckValue(object value)
    {
      if (value==null)
        return true;

      var validator = nonGenericValidator
        ?? genericValidators.GetValue(value.GetType(), RangeValidator.Create, Min, Max);

      return validator.Validate(value);
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return true;
    }

    /// <inheritdoc/>
    protected override void ValidateSelf(bool compileTime)
    {
      if (Max==null && Min==null)
        throw new ArgumentException(Strings.ExMaxOrMinPropertyMustBeSpecified);
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessage()
    {
      if (Max==null && Min==null)
        return string.Empty;
      if (Min==null)
        return Strings.ConstraintMessageValueCanNotBeGreaterThanMax;
      if (Max==null)
        return Strings.ConstraintMessageValueCanNotBeLessThanMin;
      return Strings.ConstraintMessageValueCanNotBeLessThanMinOrGreaterThanMax;
    }

    /// <inheritdoc/>
    protected override void AddCustomMessageParameters(Dictionary<string, object> parameters)
    {
      if (Min!=null)
        parameters.Add(MinParameter, Min);
      if (Max!=null)
        parameters.Add(MaxParameter, Max);
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      var targetType = Property.DeclaringType;
      if (targetType.IsGenericType)
        genericValidators = ThreadSafeDictionary<Type, RangeValidator>.Create(new object());
      else
        nonGenericValidator = RangeValidator.Create(Property.PropertyType, Min, Max);
    }
  }
}