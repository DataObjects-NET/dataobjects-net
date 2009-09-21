// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.11.11

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Xtensive.Core.Conversion;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Extension methods for <see cref="Tuple"/>.
  /// </summary>
  public static class TupleFormatExtensions
  {
    private static Regex allExceptLastValueRegex = new Regex("(( (?<value>[^\"]*?),)|( \"(?<value>.*?)\",))",
      RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static Regex lastValueRegex = new Regex("(, \"(?<value>.*?)\"$)|(, (?<value>[^\"]*?)$)",
      RegexOptions.RightToLeft | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static Regex onlyOneValueRegex = new Regex("( \"(?<value>.*)\"$)|( (?<value>[^\"]*)$)",
      RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly FormatHandler formatHandler 
      = new FormatHandler();
    private static readonly ParseHandler parseHandler 
      = new ParseHandler();
    private const string nullValue = "null";

    #region Nested types: FormatData, ParseData

    private struct FormatData
    {
      public Tuple Source;
      public StringBuilder Target;

      public FormatData(Tuple source)
      {
        Source = source;
        Target = new StringBuilder();
      }
    }

    private struct ParseData
    {
      public Tuple Target;
      public string ParsedPart;

      public ParseData(TupleDescriptor descriptor)
      {
        Target = Tuple.Create(descriptor);
        ParsedPart = string.Empty;
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
      for (int i = 0; i < tuple.Count; i++)
        tuple.Descriptor.Execute(formatHandler, ref actionData, i);
      return actionData.Target.ToString();
    }

    /// <summary>
    /// Returns a <see cref="Tuple"/> with specified <paramref name="descriptor"/>
    /// parsed from the <paramref name="source"/> string.
    /// </summary>
    /// <param name="descriptor">The descriptor of <see cref="Tuple"/> to parse.</param>
    /// <param name="source">The string to parse.</param>
    /// <returns>A <see cref="Tuple"/> parsed from the <paramref name="source"/> string.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> string 
    /// can't be parsed to a <see cref="Tuple"/> with specified <paramref name="descriptor"/>.</exception>
    public static Tuple Parse(this TupleDescriptor descriptor, string source)
    {
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "descriptor");
      var actionData = new ParseData(descriptor);
      var res = allExceptLastValueRegex.Matches(source);
      if ((res.Count + 1) != descriptor.Count)
        throw new InvalidOperationException(Strings.ExStringDoesNotCorrespondToDescriptor);

      var regex = res.Count!=0 ? lastValueRegex.Match(source) : onlyOneValueRegex.Match(source);
      for (int i = 0; i <= res.Count; i++) {
        var v = i!=res.Count ? res[i] : regex;
        var value = v.Groups["value"].Value;
        if (((value==string.Empty || value==nullValue) && v.Value.Contains("\""))
          || !(value==string.Empty || value==nullValue)) {
          actionData.ParsedPart = HttpUtility.HtmlDecode(value);
          descriptor.Execute(parseHandler, ref actionData, i);
        }
        else if (value==nullValue)
          actionData.Target.SetValue(i, null);
      }
      return actionData.Target;
    }

    #region Private methods

    private class FormatHandler : ITupleActionHandler<FormatData>
    {
      public bool Execute<TFieldType>(ref FormatData actionData, int fieldIndex)
      {
        try {
          var tuple = actionData.Source;
          var converter = AdvancedConverterProvider.Default.GetConverter<TFieldType, string>();
          TupleFieldState state;
          var value = tuple.GetValue<TFieldType>(fieldIndex, out state);
          string parsedStr = !state.IsAvailable() 
            ? string.Empty 
            : (state.IsNull() ? null : converter.Convert(value));
          var str = parsedStr ?? nullValue;

          actionData.Target.Append(" ");

          if ((str.Equals(string.Empty) && state.IsAvailable()) || str.Length > 50
            || str.StartsWith(" ") || str.EndsWith(" ") || str.Contains("\"") || str.Contains(",")
              || (str.Equals(nullValue) && !(parsedStr==null))) {
            actionData.Target.Append("\"");
            if (fieldIndex==tuple.Count - 1)
              actionData.Target.Append(HttpUtility.HtmlEncode(str)).Append("\"");
            else
              actionData.Target.Append(HttpUtility.HtmlEncode(str)).Append("\",");
          }
          else {
            if (fieldIndex==tuple.Count - 1)
              actionData.Target.Append(HttpUtility.HtmlEncode(str));
            else
              actionData.Target.Append(HttpUtility.HtmlEncode(str)).Append(",");
          }
          return true;
        }
        catch {
          return false;
        }
      }
    }

    private class ParseHandler : ITupleActionHandler<ParseData>
    {
      public bool Execute<TFieldType>(ref ParseData actionData, int fieldIndex)
      {
        try {
          var converter = AdvancedConverterProvider.Default.GetConverter<string, TFieldType>();
          actionData.Target.SetValue(fieldIndex, converter.Convert(actionData.ParsedPart));
          return true;
        }
        catch {
          return false;
        }
      }
    }

    #endregion
  }
}
