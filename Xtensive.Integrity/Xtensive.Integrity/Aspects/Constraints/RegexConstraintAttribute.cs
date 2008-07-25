// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Text.RegularExpressions;
using Xtensive.Core.Internals.DocTemplates;
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
    public override void ValidateValue(IValidatable target, object value)
    {
      string stringValue = (string) value;

      if (!string.IsNullOrEmpty(stringValue) && !regex.IsMatch(stringValue))
        throw new ConstraintViolationException(
          string.Format(Resources.Strings.StringXDoesNotMatchRegexPatternY, value, regex));
    }

    /// <inheritdoc/>
    public override bool IsTypeSupported(Type type)
    {
      return type == typeof (string);
    }

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="regexPattern">The regular expression pattern.</param>
    public RegexConstraintAttribute(string regexPattern)
    {
      regex = new Regex(regexPattern);
    }
  }
}