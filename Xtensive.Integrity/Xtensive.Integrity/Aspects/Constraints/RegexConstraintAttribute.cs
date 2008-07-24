// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Text.RegularExpressions;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Checks field value with specified regular expression.
  /// </summary>
  [Serializable]
  public class RegexConstraintAttribute : FieldConstraintAspect
  {
    private readonly Regex regex;
    
    /// <inheritdoc/>
    public override void Validate(IValidationAware target, object value)
    {
      string stringValue = (string) value;

      if (!string.IsNullOrEmpty(stringValue) && !regex.IsMatch(stringValue))
        throw new Exception(
          string.Format(Resources.Strings.StringXDoesNotMatchRegexPatternY, value, stringValue));
    }

    /// <inheritdoc/>
    public override bool IsTypeSupported(Type type)
    {
      return type == typeof (string);
    }
    
    public RegexConstraintAttribute(string regexPattern)
    {
      regex = new Regex(regexPattern);
    }
  }
}