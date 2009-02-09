// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.08

using System;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Xtensive.Core.Linq;

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
    /// <param name="inCSharpNotation">If set to <see langword="true"/>, 
    /// the result will be returned in C# notation 
    /// (<see cref="ExpressionWriter"/> will be used).</param>
    /// <returns>A string containing formatted expression.</returns>
    public static string ToString(this Expression expression, bool inCSharpNotation)
    {
      if (!inCSharpNotation)
        return expression.ToString();

      return ExpressionWriter.Write(expression);

//      // The old code
//      string result = expression.ToString();
//      result = Regex.Replace(result, @"value\([^)]+DisplayClass[^)]+\)\.", "");
//      return result;
    }
  }
}