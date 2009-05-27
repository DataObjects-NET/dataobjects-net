// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Aspects.Constraints
{
  /// <summary>
  /// Ensures property value matches specified regular expression.
  /// </summary>
  [Serializable]
  public class RegexConstraint : PropertyConstraintAspect
  {
    private Regex regex;

    /// <summary>
    /// Gets or sets the regular expression pattern.
    /// </summary>
    public string Pattern { get; set; }

    /// <summary>
    /// Gets or sets the options, default value is <see cref="RegexOptions.Compiled"/>.
    /// </summary>
    public RegexOptions Options { get; set; }

    /// <inheritdoc/>
    public override bool IsValid(object value)
    {
      string stringValue = (string) value;
      return 
        string.IsNullOrEmpty(stringValue) || 
        regex.IsMatch(stringValue);
    }

    /// <inheritdoc/>
    public override bool IsSupported(Type valueType)
    {
      return valueType==typeof (string);
    }

    protected override IEnumerable<KeyValuePair<string, string>> GetMessageParams()
    {
      yield return new KeyValuePair<string, string>("Pattern", Pattern);
    }

    protected override string GetDefaultMessage()
    {
      return Strings.ConstraintMessageValueFormatIsIncorrect;
    }

    protected override void ValidateConstraintProperties()
    {
      if (string.IsNullOrEmpty(Pattern))
        throw new Exception(Strings.ExExpressionPatternIsNotSpecified);
    }

    protected override void Initialize()
    {
      base.Initialize();
      regex = new Regex(Pattern, Options | RegexOptions.Compiled);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RegexConstraint()
    {
      Message = Strings.PropertyValueDoesNotMatchRegexPattern;
      Options = RegexOptions.Compiled;
    }
  }
}