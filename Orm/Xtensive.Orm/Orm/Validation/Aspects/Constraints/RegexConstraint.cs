// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures property value matches specified regular expression.
  /// </summary>
  [Serializable]
  public sealed class RegexConstraint : PropertyConstraintAspect
  {
    private const string PatternParameter = "Pattern";
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
    public override bool CheckValue(object value)
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

    /// <inheritdoc/>
    protected override void ValidateSelf(bool compileTime)
    {
      if (string.IsNullOrEmpty(Pattern))
        throw new ArgumentException(Strings.ExExpressionPatternIsNotSpecified);
    }

    /// <inheritdoc/>
    protected override string GetDefaultMessage()
    {
      return Strings.PropertyValueDoesNotMatchRegexPattern;
    }

    /// <inheritdoc/>
    protected override void AddCustomMessageParameters(Dictionary<string, object> parameters)
    {
      parameters.Add(PatternParameter, Pattern);
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      regex = new Regex(Pattern, Options | RegexOptions.Compiled);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public RegexConstraint()
    {
      Options = RegexOptions.Compiled;
    }
  }
}