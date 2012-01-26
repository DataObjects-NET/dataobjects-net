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
using Xtensive.Core;
using Xtensive.Core.Conversion;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using System.Linq;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Extension methods for <see cref="Tuple"/>.
  /// </summary>
  public static class TupleFormatExtensions
  {
    private const string NullValue = "null";
    private const char Quote = '"';
    private const char Escape = '\\';
    private const char Comma = ',';

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
    /// Returns string representation of the specified <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The <see cref="Tuple"/> to format.</param>
    /// <returns>
    /// String representation of the specified <paramref name="source"/>.
    /// </returns>
    public static string Format(this Tuple source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "tuple");
      var descriptor = source.Descriptor;
      var count = source.Count;
      var targets = new string[count];
      for (int i = 0; i < count; i++)
        FormatHandler.Get(descriptor[i]).Execute(source, targets, i);
      return targets.RevertibleJoin(Escape, Comma);
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
      var target = Tuple.Create(descriptor);
      var count = target.Count;
      var sources = source.RevertibleSplit(Escape, Comma).ToArray();
      for (int i = 0; i < count; i++)
        ParseHandler.Get(descriptor[i]).Execute(sources, target, i);
      return target;
    }

    #region Handler classes

    private abstract class FormatHandler
    {
      public abstract void Execute(Tuple source, string[] targets, int fieldIndex);

      public static FormatHandler Get(Type fieldType)
      {
        return (FormatHandler) Activator.CreateInstance(typeof (FormatHandler<>).MakeGenericType(fieldType));
      }
    }

    private sealed class FormatHandler<TFieldType> : FormatHandler
    {
      public override void Execute(Tuple source, string[] targets, int fieldIndex)
      {
        var fieldState = source.GetFieldState(fieldIndex);
        string result;
        if (!fieldState.IsAvailable())
          result = string.Empty;
        else if (fieldState.IsNull())
          result = NullValue;
        else {
          result = AdvancedConverterProvider.Default
            .GetConverter<TFieldType, string>()
            .Convert(source.GetValue<TFieldType>(fieldIndex, out fieldState));
          if (result.IsNullOrEmpty() || result==NullValue || result.Contains(Quote))
            result = Quote + (result ?? string.Empty).Escape(Escape, new char[Quote]) + Quote; // Quoting
        }
        targets[fieldIndex] = result;
      }
    }

    private abstract class ParseHandler
    {
      public abstract void Execute(string[] source, Tuple target, int fieldIndex);

      public static ParseHandler Get(Type fieldType)
      {
        return (ParseHandler) Activator.CreateInstance(typeof (ParseHandler<>).MakeGenericType(fieldType));
      }
    }

    private sealed class ParseHandler<TFieldType> : ParseHandler
    {
      public override void Execute(string[] sources, Tuple target, int fieldIndex)
      {
        string source = sources[fieldIndex];
        if (source.IsNullOrEmpty())
          return;
        if (source==NullValue) {
          target.SetValue(fieldIndex, null);
          return;
        }
        if (source[0]==Quote && source[source.Length - 1]==Quote)
          source = source.Substring(1, source.Length - 2).Unescape(Escape);
        var result = AdvancedConverterProvider.Default
          .GetConverter<string, TFieldType>()
          .Convert(source);
        target.SetValue(fieldIndex, result);
      }
    }

    #endregion
  }
}
