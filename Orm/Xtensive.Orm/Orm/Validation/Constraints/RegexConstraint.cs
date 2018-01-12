// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Text.RegularExpressions;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Ensures property value matches specified regular expression.
  /// </summary>
  public sealed class RegexConstraint : PropertyValidator
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

    public override void Configure(Domain domain, Model.TypeInfo type, Model.FieldInfo field)
    {
      base.Configure(domain, type, field);

      if (field.ValueType!=typeof (string))
        ThrowConfigurationError(string.Format(Strings.FieldShouldBeOfTypeX, typeof (string).FullName));

      if (string.IsNullOrEmpty(Pattern))
        ThrowConfigurationError(Strings.RegularExpressionPatternIsNotSpecified);

      try {
        regex = new Regex(Pattern, Options);
      }
      catch (ArgumentException exception) {
        ThrowConfigurationError(
          string.Format(Strings.FailedToCreateRegularExpressionFromPatternX, Pattern),
          exception);
      }
    }

    public override ValidationResult Validate(Entity target, object fieldValue)
    {
      var value = (string) fieldValue;
      return value==null || regex.IsMatch(value)
        ? Success()
        : Error(string.Format(Strings.ValueDoesNotMatchRegexPatternX, Pattern), fieldValue);
    }

    public override IPropertyValidator CreateNew()
    {
      return new RegexConstraint(Pattern, Options) {
        IsImmediate = IsImmediate,
        SkipOnTransactionCommit = SkipOnTransactionCommit,
        ValidateOnlyIfModified = ValidateOnlyIfModified
      };
    }

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    [Obsolete("Use overload with pattern instead.")]
    public RegexConstraint()
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="pattern"><see cref="Regex"/> pattern.</param>
    public RegexConstraint(string pattern)
    {
      Pattern = pattern;
      Options = RegexOptions.Compiled;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="pattern"><see cref="Regex"/> pattern.</param>
    /// <param name="options"><see cref="Regex"/> options.</param>
    public RegexConstraint(string pattern, RegexOptions options)
    {
      Pattern = pattern;
      Options = options;
    }
  }
}