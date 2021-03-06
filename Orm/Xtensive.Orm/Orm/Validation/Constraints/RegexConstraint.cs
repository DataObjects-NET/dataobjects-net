// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Text.RegularExpressions;
using Xtensive.Reflection;

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

      if (field.ValueType!=WellKnownTypes.String)
        ThrowConfigurationError(string.Format(Strings.FieldShouldBeOfTypeX, WellKnownTypes.String.FullName));

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
