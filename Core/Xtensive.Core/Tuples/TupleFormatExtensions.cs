// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.11

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Xtensive.Conversion;
using Xtensive.Core;
using Xtensive.Resources;
using Xtensive.Tuples;
using Xtensive.Reflection;
using Tuple = Xtensive.Tuples.Tuple;
using System.Linq;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Extension methods for <see cref="Tuple"/>.
  /// </summary>
  public static class TupleFormatExtensions
  {
    private static readonly FormatHandler formatHandler 
      = new FormatHandler();
    private static readonly ParseHandler parseHandler 
      = new ParseHandler();
    private const string NullValue = "null";
    private const char Quote = '"';
    private const char Escape = '\\';
    private const char Comma = ',';

    #region Nested types: FormatData, ParseData

    private struct FormatData
    {
      public Tuple Source;
      public string[] Target;

      public FormatData(Tuple source)
      {
        Source = source;
        Target = new string[source.Count];
      }
    }

    private struct ParseData
    {
      public string[] Source;
      public Tuple Target;

      public ParseData(string[] parts, TupleDescriptor targetDescriptor)
      {
        Source = parts;
        Target = Tuple.Create(targetDescriptor);
      }
    }

    #endregion

    /// <summary>
    /// Converts the <paramref name="source"/> <see cref="Tuple"/> to 
    /// its string representation.
    /// </summary>
    /// <param name="source">The tuple to convert.</param>
    /// <param name="format">Indicates whether to use <see cref="Format"/>,
    /// or <see cref="Tuple.ToString"/> method.</param>
    /// <returns>String representation of <paramref name="source"/> <see cref="Tuple"/>.</returns>
    public static string ToString(this Tuple source, bool format)
    {
      return format ? source.Format() : source.ToString();
    }

    /// <summary>
    /// Returns string representation of the specified <paramref name="tuple"/>.
    /// </summary>
    /// <param name="tuple">The <see cref="Tuple"/> to format.</param>
    /// <returns>
    /// String representation of the specified <paramref name="tuple"/>.
    /// </returns>
    public static string Format(this Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");
      var actionData = new FormatData(tuple);
      var count = tuple.Count;
      var delegates = DelegateHelper.CreateDelegates<ExecutionSequenceHandler<FormatData>>(
        formatHandler, typeof(FormatHandler), "Execute", tuple.Descriptor.fieldTypes);
      DelegateHelper.ExecuteDelegates(delegates, ref actionData, Direction.Positive);
      return actionData.Target.RevertibleJoin(Escape, Comma);
    }

    /// <summary>
    /// Returns a <see cref="Tuple"/> with specified <paramref name="descriptor"/>
    /// parsed from the <paramref name="source"/> string.
    /// </summary>
    /// <param name="descriptor">The descriptor of <see cref="Tuple"/> to parse.</param>
    /// <param name="source">The string to parse.</param>
    /// <returns>A <see cref="Tuple"/> parsed from the <paramref name="source"/> string.</returns>
    /// <exception cref="System.InvalidOperationException"><paramref name="source"/> string 
    /// can't be parsed to a <see cref="Tuple"/> with specified <paramref name="descriptor"/>.</exception>
    public static Tuple Parse(this TupleDescriptor descriptor, string source)
    {
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "descriptor");
      var actionData = new ParseData(source.RevertibleSplit(Escape, Comma).ToArray(), descriptor);
      var tuple = actionData.Target;
      var count = tuple.Count;
      var delegates = DelegateHelper.CreateDelegates<ExecutionSequenceHandler<ParseData>>(
        parseHandler, typeof(ParseHandler), "Execute", tuple.Descriptor.fieldTypes);
      DelegateHelper.ExecuteDelegates(delegates, ref actionData, Direction.Positive);
      return actionData.Target;
    }

    #region Private methods

    private class FormatHandler
    {
      public bool Execute<TFieldType>(ref FormatData actionData, int fieldIndex)
      {
        var tuple = actionData.Source;
        var fieldState = tuple.GetFieldState(fieldIndex);
        string result;
        if (!fieldState.IsAvailable())
          result = string.Empty;
        else if (fieldState.IsNull())
          result = NullValue;
        else {
          result = AdvancedConverterProvider.Default
            .GetConverter<TFieldType, string>()
            .Convert(tuple.GetValue<TFieldType>(fieldIndex, out fieldState));
          if (result.IsNullOrEmpty() || result==NullValue || result.Contains(Quote))
            result = Quote + (result ?? string.Empty).Escape(Escape, new char[Quote]) + Quote; // Quoting
        }
        actionData.Target[fieldIndex] = result;
        return false;
      }
    }

    private class ParseHandler
    {
      public bool Execute<TFieldType>(ref ParseData actionData, int fieldIndex)
      {
        string source = actionData.Source[fieldIndex];
        if (source.IsNullOrEmpty())
          return false;
        if (source==NullValue) {
          actionData.Target.SetValue(fieldIndex, null);
          return false;
        }
        if (source[0]==Quote && source[source.Length -1 ] == Quote)
          source = source.Substring(1, source.Length - 2).Unescape(Escape);
        actionData.Target.SetValue(fieldIndex, 
          AdvancedConverterProvider.Default
            .GetConverter<string, TFieldType>()
            .Convert(source));
        return false;
      }
    }

    #endregion
  }
}
