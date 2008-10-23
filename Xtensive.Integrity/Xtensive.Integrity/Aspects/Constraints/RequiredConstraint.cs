// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Integrity.Resources;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Ensures property value is not null, and optionally - not empty (see <see cref="NotEmpty"/>).
  /// </summary>
  [Serializable]
  public class RequiredConstraintAttribute : PropertyConstraintAspect
  {
    /// <summary>
    /// Gets or sets a value indicating whether empty values are allowed or not.
    /// For now "empty" means only <see cref="string.Empty"/>.
    /// Default value is <see langword="false" />.
    /// </summary>
    public bool NotEmpty { get; set; }

    /// <inheritdoc/>
    /// <exception cref="ConstraintViolationException">Value check failed.</exception>
    public override void CheckValue(IValidationAware target, object value)
    {
      if (value==null)
        throw new ConstraintViolationException(
          string.Format(Strings.PropertyValueCanNotBeNull,
          Property.GetShortName(true)));
      
      if (!NotEmpty)
        return;

      var stringValue = value as string;
      if (stringValue!=null && stringValue.Length==0)
        throw new ConstraintViolationException(
          string.Format(Strings.PropertyValueCanNotBeEmpty,
          Property.GetShortName(true)));
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return true;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RequiredConstraintAttribute()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="notEmpty"><see cref="NotEmpty"/> property value.</param>
    public RequiredConstraintAttribute(bool notEmpty)
    {
      NotEmpty = notEmpty;
    }
  }
}