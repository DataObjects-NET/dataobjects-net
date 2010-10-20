// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    public static StringBuilder AppendIndented(this StringBuilder builder, int indent, string value)
    {
      return builder.AppendIndented(indent, value, true);
    }

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
      string indentString = new string(IndentChar, indent);

      string[] lines = value.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

      for (int i = 0; i < lines.Length; i++) {       
        string line = lines[i];        

        bool isLast = i==lines.Length-1;
        if (isLast && line.Trim() == string.Empty)
          break;
        
        if (i!=0 || indentFirstLine)
          builder.Append(indentString);

        builder.Append(line);

        if (!isLast)
          builder.AppendLine();
      }

      return builder;
    }

    /// <summary>
    /// Appends the specified <see cref="byte"/> array in hexidecimal representation.
    /// These bytes are written from left to right, high part of byte is written first.
    /// For example {1,2,10} will be appended as 01020A.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="values">The values.</param>
    public static void AppendHexArray(this StringBuilder builder, byte[] values)
    {
      ArgumentValidator.EnsureArgumentNotNull(builder, "builder");
      ArgumentValidator.EnsureArgumentNotNull(values, "values");
      foreach (var item in values) {
        int hi = item >> 4;
        int low = item & 0xF;
        builder.Append(Convert.ToString(hi, 16));
        builder.Append(Convert.ToString(low, 16));
      }
    }
  }
}