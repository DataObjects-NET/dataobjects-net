// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures field length (or item count) fits in specified range.
  /// </summary>
  [Serializable]
  public sealed class LengthConstraint : PropertyConstraintAspect
  {
    private const string MinParameter = "Min";
    private const string MaxParameter = "Max";

    /// <summary>
    /// Gets or sets the minimal allowed length.
    /// Default is 0.
    /// </summary>
    public long Min { get; set; }

    /// <summary>
    /// Gets or sets the maximal allowed length.
    /// Default is <see cref="long.MaxValue"/>.
    /// </summary>
    public long Max { get; set;}

    /// <inheritdoc/>
    public override bool CheckValue(object value)
    {
      if (value==null)
        return true;
      
      long length;

      if (value is string)
        length = ((string) value).Length;
      else 
        length = ((ICollection)value).Count;

      return length >= Min && length <= Max;
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return valueType==typeof (string) || typeof (ICollection).IsAssignableFrom(valueType);
    }

    /// <inheritdoc/>
    protected override void ValidateSelf(bool compileTime)
    {
      if (Max==long.MaxValue && Min==0)
        throw new ArgumentException(
          string.Concat(Strings.ExMaxOrMinPropertyMustBeSpecified));
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessage()
    {
      if (Min==0)
        return Strings.ConstraintMessageValueLengthCanNotBeGreaterThanMax;
      if (Max==long.MaxValue)
        return Strings.ConstraintMessageValueLengthCanNotBeLessThanMin;

      return Strings.ConstraintMessageValueLengthCanNotBeLessThanMinAndGreaterThenMax;
    }

    /// <inheritdoc/>
    protected override void AddCustomMessageParameters(Dictionary<string, object> parameters)
    {
      if (Min != 0)
        parameters.Add(MinParameter, Min);
      if (Max != long.MaxValue)
        parameters.Add(MaxParameter, Max);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public LengthConstraint()
    {
      Max = long.MaxValue;
      Min = 0;
    }
  }
}