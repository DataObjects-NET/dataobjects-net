// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.08

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// <see cref="Expression"/> related extension methods.
  /// </summary>
  public static class ExpressionExtensions
  {
    /// <summary>
    /// Formats the <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">The expression to format.</param>
    /// <param name="beautify">If set to <see langword="true"/>, the result will be much more human-readable.</param>
    /// <returns>A string containing formatted expression.</returns>
    public static string ToString(this Expression expression, bool beautify)
    {
      if (!beautify)
        return expression.ToString();
      string result = expression.ToString();
      result = Regex.Replace(result, @"value\([^)]+DisplayClass[^)]+\)\.", "");
      return result;
    }
  }
}