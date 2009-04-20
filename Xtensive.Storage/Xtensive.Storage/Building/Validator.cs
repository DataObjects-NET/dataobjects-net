// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.12

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xtensive.Storage.Building
{
  internal static class Validator
  {
    private static readonly Dictionary<ValidationRule, Regex> regexps = new Dictionary<ValidationRule, Regex>(5);

    /// <summary>
    /// Determines whether the specified name is valid.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="rule">The validation rule.</param>
    /// <returns>
    ///   <see langword="true"/> if the specified name is valid; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNameValid(string name, ValidationRule rule)
    {
      switch (rule) {
      case ValidationRule.Type:
      case ValidationRule.Service:
      case ValidationRule.Field:
      case ValidationRule.Column:
      case ValidationRule.Index:

        return !string.IsNullOrEmpty(name) && regexps[rule].IsMatch(name);
      }
      throw new ArgumentOutOfRangeException("rule",
        string.Format("Unknown validation rule '{0}'.", rule));
    }


    // Type initializer

    static Validator()
    {
      Regex nameRe = new Regex(@"^[A-z][A-z0-9\-\._]*$", RegexOptions.Compiled);
      regexps.Add(ValidationRule.Type, nameRe);
      regexps.Add(ValidationRule.Field, new Regex(@"^[A-z][A-z0-9\-_]*$", RegexOptions.Compiled));
      regexps.Add(ValidationRule.Column, nameRe);
      regexps.Add(ValidationRule.Index, nameRe);
    }
  }
}