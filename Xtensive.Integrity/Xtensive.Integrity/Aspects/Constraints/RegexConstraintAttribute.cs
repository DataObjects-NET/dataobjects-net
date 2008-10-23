// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Text.RegularExpressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Resources;
using Xtensive.Integrity.Validation.Interfaces;
using Xtensive.Core.Reflection;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Ensures property value matches specified regular expression.
  /// </summary>
  [Serializable]
  public class RegexConstraintAttribute : PropertyConstraintAspect
  {
    /// <summary>
    /// Gets or sets the <see cref="Regex"/> to use.
    /// </summary>
    public Regex Regex { get; set; }
    
    /// <inheritdoc/>
    /// <exception cref="ConstraintViolationException">Value check failed.</exception>
    public override void CheckValue(IValidationAware target, object value)
    {
      string stringValue = (string) value;

      if (!string.IsNullOrEmpty(stringValue) && !Regex.IsMatch(stringValue))
        throw new ConstraintViolationException(string.Format(
          Strings.PropertyValueDoesNotMatchRegexPattern, 
          Property.GetShortName(true), Regex));
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return valueType==typeof (string);
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="regexPattern">The regular expression pattern.</param>
    public RegexConstraintAttribute(string regexPattern)
    {
      Regex = new Regex(regexPattern, RegexOptions.Compiled);
    }
  }
}