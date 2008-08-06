// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.30

using System.Text;

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// <see cref="StringBuilder"/> related extension methods.
  /// </summary>
  public static class StringBuilderExtensions
  {
    private const char IndentChar = ' ';

    /// <summary>
    /// Appends the line with specified <see cref="string"/> indented by specified count of spaces.
    /// </summary>
    /// <param name="builder">The builder to write line to.</param>
    /// <param name="value">The string value to write.</param>
    /// <param name="indent">Count of spaces to indent.</param>
    /// <returns>A reference to the <paramref name="builder"/> after append operation has completed.</returns>
    public static StringBuilder AppendIndented(this StringBuilder builder, int indent, string value)
    {
      builder.Append(new string(IndentChar, indent));
      builder.Append(value);
      return builder;
    }
  }
}