// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Collections;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Resources;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Ensures field length (or item count) fits in specified range.
  /// </summary>
  [Serializable]
  public class LengthConstraintAttribute : PropertyConstraintAspect
  {
    /// <summary>
    /// Gets the minimal allowed length.
    /// </summary>
    public long MinLength { get; private set; }

    /// <summary>
    /// Gets the maximal allowed length.
    /// </summary>
    public long MaxLength { get; private set; }

    /// <inheritdoc/>
    public override void CheckValue(IValidationAware target, object value)
    {
      long length;
      if (value==null)
        length = 0;
      else if (value.GetType()==typeof(string))
        length = (value as string).Length;
      else if (value is ICountable)
        length = (value as ICountable).Count;
      else 
        length = ((ICollection)value).Count;

      if (length<MinLength || length>MaxLength)
        throw new ConstraintViolationException(
          string.Format(Strings.ValueLengthMustBeInXYRange, MinLength, MaxLength));
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return
        valueType==typeof (string) ||
        typeof (ICountable).IsAssignableFrom(valueType) ||
        typeof (ICollection).IsAssignableFrom(valueType);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="maxLength"><see cref="MaxLength"/> property value.</param>
    public LengthConstraintAttribute(long maxLength)
      : this(0, maxLength)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="minLength"><see cref="MinLength"/> property value.</param>
    /// <param name="maxLength"><see cref="MaxLength"/> property value.</param>
    public LengthConstraintAttribute(long minLength, long maxLength)
    {
      MinLength = minLength;
      MaxLength = maxLength;
    }
  }
}