// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
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
    /// Gets the minimal allowed value.
    /// </summary>
    public object MinValue { get; private set; }

    /// <summary>
    /// Gets the maximal allowed value.
    /// </summary>
    public object MaxValue { get; private set; }

    /// <inheritdoc/>
    public override void CheckValue(IValidationAware target, object value)
    {
      var comparer = cachedComparer.GetValue(
        _this => _this
          .GetType()
          .GetMethod("GetComparer",
            BindingFlags.NonPublic | BindingFlags.Instance, null, ArrayUtils<Type>.EmptyArray, null)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_this.Property.PropertyType})
          .Invoke(_this, null) as Func<object, object, int>, 
        this);

      if (comparer.Invoke(value, MinValue)<0 || comparer.Invoke(value, MaxValue)>0)
        throw new ConstraintViolationException(
          string.Format(Strings.ValueMustBeInXYRange, MinValue, MaxValue));
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return valueType==Property.PropertyType;
    }

// ReSharper disable UnusedPrivateMember
    private Func<object, object, int> GetComparer<T>()
// ReSharper restore UnusedPrivateMember
    {
      var compare = AdvancedComparer<T>.System.Compare;
      return (l, r) => compare((T) l, (T) r);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="maxValue"><see cref="MaxValue"/> property value.</param>
    public RangeConstraintAttribute(object maxValue)
      : this(0, maxValue)
    {
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