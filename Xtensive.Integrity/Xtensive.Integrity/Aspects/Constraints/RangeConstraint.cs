// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Core.Threading;
using Xtensive.Integrity.Resources;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Ensures field value fits in specified range.
  /// </summary>
  [Serializable]
  public class RangeConstraintAttribute : PropertyConstraintAspect, IDeserializationCallback
  {
    [NonSerialized]
    private ThreadSafeCached<Func<object, object, int>> cachedComparer = 
      ThreadSafeCached<Func<object, object, int>>.Create(new object());

    /// <summary>
    /// Gets or sets the minimal allowed value.
    /// <see langword="null" /> means "ignore this boundary".
    /// Default value is <see langword="null" />.
    /// </summary>
    public object MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximal allowed value.
    /// <see langword="null" /> means "ignore this boundary".
    /// Default value is <see langword="null" />.
    /// </summary>
    public object MaxValue { get; set; }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(object element)
    {
      if (!base.CompileTimeValidate(element))
        return false;

      // Checking if a comparer we need exists
      Func<object, object, int> comparer = null;
      try {
        comparer = GetCachedComparer();
        ErrorLog.Debug("Comparer: {0} for {1}", comparer, Property.GetShortName(true));
      }
      catch {}
      if (comparer==null) {
        ErrorLog.Write(SeverityType.Error, Strings.AspectExNoComparer,
          AspectHelper.FormatType(GetType()),
          AspectHelper.FormatMember(Property.DeclaringType, Property),
          AspectHelper.FormatType(Property.PropertyType));
        return false;
      }

      return true;
    }

    /// <inheritdoc/>
    /// <exception cref="ConstraintViolationException">Value check failed.</exception>
    public override void CheckValue(IValidationAware target, object value)
    {
      var comparer = GetCachedComparer();

      if (comparer.Invoke(value, MinValue)<0 || comparer.Invoke(value, MaxValue)>0)
        throw new ConstraintViolationException(string.Format(
          Strings.PropertyValueMustBeInXYRange, 
          Property.GetShortName(true), MinValue, MaxValue));
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return valueType==Property.PropertyType;
    }

    private Func<object, object, int> GetCachedComparer()
    {
      return cachedComparer.GetValue(
        _this => _this
          .GetType()
          .GetMethod("GetComparer",
            BindingFlags.NonPublic | BindingFlags.Instance, null, ArrayUtils<Type>.EmptyArray, null)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_this.Property.PropertyType})
          .Invoke(_this, null) as Func<object, object, int>, 
        this);
    }

// ReSharper disable UnusedPrivateMember
    private Func<object, object, int> GetComparer<T>()
// ReSharper restore UnusedPrivateMember
    {
      if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
        return null;
      var compare = AdvancedComparer<T>.System.Compare;
      if (compare==null)
        return null;
      return (l, r) => r==null ? 0 : compare((T) l, (T) r);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RangeConstraintAttribute()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="maxValue"><see cref="MaxValue"/> property value.</param>
    public RangeConstraintAttribute(object maxValue)
    {
      MaxValue = maxValue;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="minValue"><see cref="MinValue"/> property value.</param>
    /// <param name="maxValue"><see cref="MaxValue"/> property value.</param>
    public RangeConstraintAttribute(object minValue, object maxValue)
    {
      MinValue = minValue;
      MaxValue = maxValue;
    }

    /// <inheritdoc/>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      if (cachedComparer.SyncRoot==null)
        cachedComparer = ThreadSafeCached<Func<object, object, int>>.Create(new object());
    }
  }
}