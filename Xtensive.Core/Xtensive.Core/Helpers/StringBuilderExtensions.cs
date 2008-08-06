// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.30

using System;
using System.Diagnostics;
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
  }
}