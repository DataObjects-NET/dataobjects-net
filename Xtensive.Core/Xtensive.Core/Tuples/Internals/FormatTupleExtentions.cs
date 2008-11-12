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

namespace Xtensive.Core.Tuples.Internals
{
  internal static class FormatTupleExtentions
  {
    public static Regex StrRegex = new Regex("(( (?<value>[^\"]*?),)|( \"(?<value>.*?)\",))",
      RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static Regex EndOfStrRegex = new Regex("(, \"(?<value>.*?)\"$)|(, (?<value>[^\"]*?)$)",
      RegexOptions.RightToLeft | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static Regex OnlyEndOfStrRegex = new Regex("( \"(?<value>.*)\"$)|( (?<value>[^\"]*)$)",
      RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly ForwardConvertionDataHandler forwardConvertionHandler = new ForwardConvertionDataHandler();
    private static readonly BackwardConvertionDataHandler backwardConvertionHandler = new BackwardConvertionDataHandler();
    private const string nullableValue = "null";

    #region Convertion methods

    ///<summary>
    /// Returns string representation of the specified <see cref="Tuple"/>.
    ///</summary>
    ///<param name="source">The specified <see cref="Tuple"/></param>
    ///<returns>String representation of the specified <see cref="Tuple"/>.</returns>
    public static string ConvertToString(Tuple source)
    {
      var actionData = new ForwardConvertionData(source);
      for (int i = 0; i < source.Count; i++)
        source.Descriptor.Execute(forwardConvertionHandler, ref actionData, i);
      return actionData.Target.ToString();
    }

    ///<summary>
    /// Returns the specified <see cref="Tuple"/> from the string representation.
    ///</summary>
    ///<param name="descriptor">The specified <see cref="Tuple"/></param>
    ///<param name="source">The string to convert from.</param>
    ///<returns>The specified <see cref="Tuple"/> from the string representation.</returns>
    public static Tuple ConvertFromString(string source, TupleDescriptor descriptor)
    {
      var actionData = new BackwardConvertionData(descriptor);
      var res = StrRegex.Matches(source);
      if (res.Count + 1!=descriptor.Count)
        throw new InvalidOperationException(Strings.ExStringNotCorrespondDescriptor);

      var regex = res.Count!=0 ? EndOfStrRegex.Match(source) : OnlyEndOfStrRegex.Match(source);

      for (int i = 0; i <= res.Count; i++) {
        var v = i!=res.Count ? res[i] : regex;
        var value = v.Groups["value"].Value;
        if (((value.Equals(string.Empty) || value.Equals(nullableValue)) && v.Value.Contains("\""))
          || !(value.Equals(string.Empty) || value.Equals(nullableValue))) {
          actionData.ParsedStringPart = HttpUtility.HtmlDecode(value);
          descriptor.Execute(backwardConvertionHandler, ref actionData, i);
        }
        else if (value.Equals(nullableValue))
          actionData.Target.SetValue(i, null);
      }
      return actionData.Target;
    }

    #endregion

    #region Private methods

    private struct ForwardConvertionData
    {
      public Tuple Source;
      public StringBuilder Target;

      public ForwardConvertionData(Tuple source)
      {
        Source = source;
        Target = new StringBuilder();
      }
    }

    private struct BackwardConvertionData
    {
      public Tuple Target;
      public string ParsedStringPart;

      public BackwardConvertionData(TupleDescriptor descriptor)
      {
        Target = Tuple.Create(descriptor);
        ParsedStringPart = string.Empty;
      }
    }


    private class ForwardConvertionDataHandler : ITupleActionHandler<ForwardConvertionData>
    {
      public bool Execute<TFieldType>(ref ForwardConvertionData actionData, int fieldIndex)
      {
        try {
          var tuple = actionData.Source;
          var converter = AdvancedConverterProvider.Default.GetConverter<TFieldType, string>();
          string parsedStr = !tuple.IsAvailable(fieldIndex) ? string.Empty : converter.Convert(tuple.GetValue<TFieldType>(fieldIndex));
          var str = parsedStr ?? nullableValue;

          actionData.Target.Append(" ");

          if ((str.Equals(string.Empty) && tuple.IsAvailable(fieldIndex)) || str.Length > 50
            || str.StartsWith(" ") || str.EndsWith(" ") || str.Contains("\"") || str.Contains(",")
              || (str.Equals(nullableValue) && !(parsedStr==null))) {
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

    private class BackwardConvertionDataHandler : ITupleActionHandler<BackwardConvertionData>
    {
      public bool Execute<TFieldType>(ref BackwardConvertionData actionData, int fieldIndex)
      {
        try {
          var converter = AdvancedConverterProvider.Default.GetConverter<string, TFieldType>();
          actionData.Target.SetValue(fieldIndex, converter.Convert(actionData.ParsedStringPart));
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
