// Copyright (C) 2008-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2008.07.30

using System;
using System.Text;
using Xtensive.Core;

namespace Xtensive.Core
{  
  /// <summary>
  /// <see cref="StringBuilder"/> related extension methods.
  /// </summary>
  public static class StringBuilderExtensions
  {
    private const char IndentChar = ' ';

    /// <summary>
    /// Appends the specified <see cref="string"/> indented by specified count of spaces.
    /// </summary>
    /// <param name="builder">The builder to write indented string to.</param>
    /// <param name="indent">Count of spaces to indent.</param>
    /// <param name="value">The string value to write.</param>    
    /// <returns>
    /// A reference to the <paramref name="builder"/> after append operation has completed.
    /// </returns>
    public static StringBuilder AppendIndented(this StringBuilder builder, int indent, string value) =>
      builder.AppendIndented(indent, value, true);

    /// <summary>
    /// Appends the specified <see cref="string"/> indented by specified count of spaces.
    /// </summary>
    /// <param name="builder">The builder to write indented string to.</param>
    /// <param name="indent">Count of spaces to indent.</param>
    /// <param name="value">The string value to write.</param>
    /// <param name="indentFirstLine">if set to <see langword="true"/> first line of string will be indented, otherwise not.</param>
    /// <returns>
    /// A reference to the <paramref name="builder"/> after append operation has completed.
    /// </returns>
    public static StringBuilder AppendIndented(this StringBuilder builder, int indent, string value, bool indentFirstLine)
    {
      var indentString = new string(IndentChar, indent);
      var lines = value.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

      for (var i = 0; i < lines.Length; i++) {
        var line = lines[i];

        var isLast = i == lines.Length - 1;
        if (isLast && line.Trim() == string.Empty) {
          break;
        }

        if (i != 0 || indentFirstLine) {
          _ = builder.Append(indentString);
        }

        _ = builder.Append(line);

        if (!isLast) {
          _ = builder.AppendLine();
        }
      }

      return builder;
    }

    /// <summary>
    /// Appends the specified <see cref="byte"/> array in hexidecimal representation in lower case.
    /// These bytes are written from left to right, high part of byte is written first.
    /// For example {1,2,10} will be appended as 01020a.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="values">The values.</param>
    /// <returns>Original <paramref name="builder"/>.</returns>
    public static StringBuilder AppendHexArray(this StringBuilder builder, byte[] values)
    {
      ArgumentNullException.ThrowIfNull(builder, "builder");
      ArgumentNullException.ThrowIfNull(values, "values");

      const string lowerHexChars = "0123456789abcdef";
      foreach (var item in values) {
        _ = builder.Append(lowerHexChars[item >> 4])
          .Append(lowerHexChars[item & 0xF]);
      }
      return builder;
    }

    /// <summary>
    /// Appends the specified <see cref="byte"/> array in hexidecimal representation in lower case.
    /// These bytes are written from left to right, high part of byte is written first.
    /// For example {1,2,10} will be appended as 01020a.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="values">The values.</param>
    /// <returns>Original <paramref name="builder"/>.</returns>
    public static StringBuilder AppendHexArray(this StringBuilder builder, in ReadOnlySpan<byte> values)
    {
      ArgumentNullException.ThrowIfNull(builder, "builder");

      const string lowerHexChars = "0123456789abcdef";
      foreach (var item in values) {
        _ = builder.Append(lowerHexChars[item >> 4])
          .Append(lowerHexChars[item & 0xF]);
      }
      return builder;
    }
  }
}