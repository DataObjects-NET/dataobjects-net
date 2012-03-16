// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.28

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xtensive.Core;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Drivers.SqlServer
{
  /// <summary>
  /// Parser of SQL Server error messages.
  /// </summary>
  public sealed class ErrorMessageParser
  {
    private sealed class PreparedTemplate
    {
      public readonly Regex ParserExpression;
      public readonly ReadOnlyCollection<int> Indexes;

      public PreparedTemplate(string regex, IEnumerable<int> indexes)
      {
        ParserExpression = new Regex(regex, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        Indexes = indexes.ToList().AsReadOnly();
      }
    }

    private const string CaptureExpression = "(.*)";

    private readonly Dictionary<int, PreparedTemplate> templates;

    /// <summary>
    /// Parses the specified <paramref name="message"/> according to its template
    /// and returns formatting arguments.
    /// Template is chosen by <paramref name="code"/>.
    /// This method acts as a reverse for internal string formatting routines in SQL Server.
    /// </summary>
    /// <param name="code">Message code to identify pattern.</param>
    /// <param name="message">Message to parse.</param>
    /// <returns>All placeholder values taken from message with their position numbers.</returns>
    public Dictionary<int, string> Parse(int code, string message)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      PreparedTemplate template;
      if (!templates.TryGetValue(code, out template))
        throw new InvalidOperationException(string.Format(Strings.ExNoMessageTemplateIsRegisteredForCodeX, code));
      // Fill result with empty string for each expected index for more simple API
      var result = template.Indexes.ToDictionary(index => index, index => string.Empty);
      var match = template.ParserExpression.Match(message);
      if (!match.Success)
        return result;
      // Handle special case when whole expression is placeholder
      if (template.ParserExpression.ToString() == CaptureExpression) {
        result[template.Indexes[0]] = match.Value;
        return result;
      }
      var end = Math.Min(match.Groups.Count - 1, template.Indexes.Count);
      for (int i = 0; i < end; i++) {
        var group = match.Groups[i + 1];
        if (group.Success)
          result[template.Indexes[i]] = group.Value;
      }
      return result;
    }

    /// <summary>
    /// Helper method for parsing error messsages.
    /// It extracts anything found in single or double quotes.
    /// </summary>
    /// <returns>Text within the quotes if quotes are present,
    /// otherwise original <paramref name="text"/>.</returns>
    public static string ExtractQuotedText(string text)
    {
      ArgumentValidator.EnsureArgumentNotNull(text, "text");
      var quoted = ExtractQuotedText(text, '\'');
      return quoted==text ? ExtractQuotedText(text, '"') : quoted;
    }

    /// <summary>
    /// Helper method for parsing error messages.
    /// It cuts leading schema prefix (i.e. dbo) from table name.
    /// </summary>
    /// <param name="table">Table name</param>
    /// <returns><paramref name="table"/> with schema prefix removed.</returns>
    public static string CutSchemaPrefix(string table)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      return CutPrefix(table);
    }

    /// <summary>
    /// Helper method for parsing error messages.
    /// It cuts leading database and schema prefix (i.e. master.dbo) from table name.
    /// </summary>
    /// <param name="table">Table name</param>
    /// <returns><paramref name="table"/> with database and schema prefix removed.</returns>
    public static string CutDatabaseAndSchemaPrefix(string table)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      return CutPrefix(CutPrefix(table));
    }

    private static string CutPrefix(string text)
    {
      var startIndex = text.IndexOf('.');
      return startIndex >= 0 ? text.Substring(startIndex + 1) : text;
    }

    private static string ExtractQuotedText(string text, char quoteChar)
    {
      var startIndex = text.IndexOf(quoteChar);
      if (startIndex < 0)
        return text;
      var endIndex = text.LastIndexOf(quoteChar);
      if (endIndex < 0)
        return text;
      if (startIndex==endIndex)
        return text;
      return text.Substring(startIndex + 1, endIndex - startIndex - 1);
    }

    private static bool CollectChunk(StringBuilder regexBuilder, string source, int chunkStartOffset, int chunkNextAfterEndOffset)
    {
      // Skip empty chunks unless regexBuilder is empty
      if (chunkStartOffset==chunkNextAfterEndOffset && regexBuilder.Length > 0)
        return false;

      regexBuilder.Append(Regex.Escape(source.Substring(chunkStartOffset, chunkNextAfterEndOffset - chunkStartOffset)));
      regexBuilder.Append(CaptureExpression);
      return true;
    }

    private static void CollectLastChunk(StringBuilder regexBuilder, string source, int chunkStartOffset)
    {
      regexBuilder.Append(Regex.Escape(source.Substring(chunkStartOffset)));
    }

    private static PreparedTemplate PrepareEnglishTemplate(string template)
    {
      // English messages use '%ls' and '%.*ls' as format placeholders.
      // They are not indexed. Index is taked from argument position.

      const string shortPlaceholder = "%ls";
      const string longPlaceholder = "%.*ls";

      int count = 0;
      int offset = 0;

      var regexBuilder = new StringBuilder(template.Length);
      var shortPlaceholderOffset = template.IndexOf(shortPlaceholder);
      var longPlaceholderOffset = template.IndexOf(longPlaceholder);

      while (shortPlaceholderOffset >= 0 && longPlaceholderOffset >= 0) {
        int placeholderOffset;
        int placeholderLength;
        if (shortPlaceholderOffset < longPlaceholderOffset) {
          placeholderOffset = shortPlaceholderOffset;
          placeholderLength = shortPlaceholder.Length;
        }
        else {
          placeholderOffset = longPlaceholderOffset;
          placeholderLength = longPlaceholder.Length;
        }
        if (CollectChunk(regexBuilder, template, offset, placeholderOffset))
          count++;
        offset = placeholderOffset + placeholderLength;
        shortPlaceholderOffset = template.IndexOf(shortPlaceholder, offset);
        longPlaceholderOffset = template.IndexOf(longPlaceholder, offset);
      }

      while (longPlaceholderOffset >= 0) {
        if (CollectChunk(regexBuilder, template, offset, longPlaceholderOffset))
          count++;
        offset = longPlaceholderOffset + longPlaceholder.Length;
        longPlaceholderOffset = template.IndexOf(longPlaceholder, offset);
      }

      while (shortPlaceholderOffset >= 0) {
        if (CollectChunk(regexBuilder, template, offset, shortPlaceholderOffset))
          count++;
        offset = shortPlaceholderOffset + shortPlaceholder.Length;
        shortPlaceholderOffset = template.IndexOf(shortPlaceholder, offset);
      }

      CollectLastChunk(regexBuilder, template, offset);
      return new PreparedTemplate(regexBuilder.ToString(), Enumerable.Range(1, count));
    }

    private static PreparedTemplate PrepareNonEnglishTemplate(string template)
    {
      // Non-english messages use placeholders in form '%N!' where N is argument index.
      // We parse only single-digit numbers for simplicity.

      const char placeholderPrefix = '%';
      const char placeholderSuffix = '!';
      const int placeholderLength = 3;

      int offset = 0;
      var regexBuilder = new StringBuilder(template.Length);
      var placeholderOffset = template.IndexOf(placeholderPrefix);
      var indexes = new List<int>();

      while (placeholderOffset >= 0 && placeholderOffset + placeholderLength <= template.Length) {
        if (char.IsDigit(template[placeholderOffset + 1]) && template[placeholderOffset + 2]==placeholderSuffix) {
          if (CollectChunk(regexBuilder, template, offset, placeholderOffset))
            indexes.Add(template[placeholderOffset + 1] - '0');
          offset = placeholderOffset + placeholderLength;
        }
        else
          offset++;
        placeholderOffset = template.IndexOf(placeholderPrefix, offset);
      }

      CollectLastChunk(regexBuilder, template, offset);
      return new PreparedTemplate(regexBuilder.ToString(), indexes);
    }


    // Constructors

    public ErrorMessageParser()
    {
      templates = new Dictionary<int, PreparedTemplate>();
    }

    public ErrorMessageParser(IEnumerable<KeyValuePair<int,string>> messageTemplates, bool isEnglish)
    {
      ArgumentValidator.EnsureArgumentNotNull(messageTemplates, "messageTemplates");
      templates = isEnglish
        ? messageTemplates.ToDictionary(item => item.Key, item => PrepareEnglishTemplate(item.Value))
        : messageTemplates.ToDictionary(item => item.Key, item => PrepareNonEnglishTemplate(item.Value));
    }
  }
}