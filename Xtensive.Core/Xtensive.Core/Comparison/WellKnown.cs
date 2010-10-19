// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.08

using System;
using System.Diagnostics;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Various well-known constants related to this namespace.
  /// </summary>
  public static class WellKnown
  {
    /// <summary>
    /// Returns <see cref="char.MaxValue"/>
    /// </summary>
    public static readonly string OrdinalMaxChar = char.MaxValue.ToString();

    /// <summary>
    /// Returns "\uDBFF\uDFFF"
    /// </summary>
    public static readonly string CultureSensitiveMaxChar = "\uDBFF\uDFFF";
  }
}