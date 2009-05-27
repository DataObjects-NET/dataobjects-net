// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Threading;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Ensures field value fits in specified range.
  /// </summary>
  [Serializable]
  public class RangeConstraint : PropertyConstraintAspect, IDeserializationCallback
  {
    [NonSerialized]
    private ThreadSafeCached<Func<object, object, int>> cachedComparer = 
      ThreadSafeCached<Func<object, object, int>>.Create(new object());

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
    protected override void ValidateConstraintProperties()
    {
      if (Max==null && Min==null)
        throw new Exception(Strings.ExMaxOrMinPropertyMustBeSpecified);

      if (Max!=null)
        Convert.ChangeType(Max, Property.PropertyType);
      if (Min!=null)
        Convert.ChangeType(Min, Property.PropertyType);

      // Checking if a comparer we need exists
      Func<object, object, int> comparer = null;
      try {
        comparer = GetCachedComparer();
      }
      catch {
      }
      if (comparer==null)
        throw new Exception(
          string.Format(Strings.ExComparerForTypeXIsNotFound, Property.PropertyType));
    }

    /// <inheritdoc/>
    public override bool IsValid(object value)
    {
      var comparer = GetCachedComparer();
      return 
        comparer.Invoke(value, Min) >= 0 && 
        comparer.Invoke(value, Max) <= 0;
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

// ReSharper disable UnusedMember.Local
    private Func<object, object, int> GetComparer<T>()
// ReSharper restore UnusedMember.Local
    {
      if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
        return null;
      var compare = AdvancedComparer<T>.System.Compare;
      if (compare==null)
        return null;
      return (l, r) => r==null ? 0 : compare((T) l, (T) r);
    }

    protected override IEnumerable<KeyValuePair<string, string>> GetMessageParams()
    {
      if (Min != null)
        yield return new KeyValuePair<string, string>("Min", Min.ToString());
      if (Max != null)
        yield return new KeyValuePair<string, string>("Max", Max.ToString());
    }

    protected override string GetDefaultMessage()
    {
      if (Max == null && Min == null)
        return string.Empty;
      if (Min == null)
        return Strings.ConstraintMessageValueCanNotBeGreaterThanMax;
      if (Max == null)
        return Strings.ConstraintMessageValueCanNotBeLessThanMin;
      return Strings.ConstraintMessageValueCanNotBeLessThanMinOrGreaterThanMax;
    }

    protected override void Initialize()
    {
      if (Max!=null)
        Max = Convert.ChangeType(Max, Property.PropertyType);
      if (Min!=null)
        Min = Convert.ChangeType(Min, Property.PropertyType);
    }


    // Constructors

    /// <inheritdoc/>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      if (cachedComparer.SyncRoot==null)
        cachedComparer = ThreadSafeCached<Func<object, object, int>>.Create(new object());
    }
  }
}