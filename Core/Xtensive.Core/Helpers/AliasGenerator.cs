// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System;

namespace Xtensive.Helpers
{
  /// <summary>
  /// Universal alias generator.
  /// </summary>
  [Serializable]
  public sealed class AliasGenerator
  {
    /// <summary>
    /// Default alias template. Value is "{0}{1}". Where {0} - template parameter for prefix and {1} - template parameter for suffix.
    /// </summary>
    public const string DefaultAliasTemplate = "{0}{1}"; // prefix + suffix

    // prefix "u" is used for user defined aliases renaming, i.e. "alias" -> "ualias".
    // should not contain this prefix.
    private readonly static string[] DefaultPrefixSequence =
      new[]
      {
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "v",
        "w", "x", "y", "z"
      };

    private readonly string[] prefixSequence;
    private readonly string aliasTemplate;
    private byte prefixIndex;
    private int suffixNumber;

    /// <summary>
    /// Generates new alias.
    /// </summary>
    public string Next()
    {
      VerifyState();
      var prefix = prefixSequence[prefixIndex++];
      var suffix = (suffixNumber > 0) ? suffixNumber.ToString() : string.Empty;
      string result = string.Format(aliasTemplate, prefix, suffix);
      return result;
    }

    private void VerifyState()
    {
      if (prefixIndex >= prefixSequence.Length) {
        prefixIndex = 0;
        suffixNumber++;
      }
    }

    /// <summary>
    /// Creates generator with default settings.
    /// </summary>
    /// <returns></returns>
    public static AliasGenerator Create()
    {
      return new AliasGenerator();
    }

    /// <summary>
    /// Creates generator using specified alias template.
    /// </summary>
    /// <param name="aliasTemplate">Alias template. Could use two template parameters: {0} - for prefix and {1} for suffix.</param>
    public static AliasGenerator Create(string aliasTemplate)
    {
      return new AliasGenerator(aliasTemplate);
    }

    /// <summary>
    /// Creates generator using specified prefix sequence.
    /// </summary>
    /// <param name="overriddenPrefixes">The overridden prefix sequence.</param>
    public static AliasGenerator Create(string [] overriddenPrefixes)
    {
      return new AliasGenerator(overriddenPrefixes);
    }

    /// <summary>
    /// Creates generator using specified <paramref name="overriddenPrefixes"/> and <paramref name="aliasTemplate"/>.
    /// </summary>
    /// <param name="overriddenPrefixes">The overridden prefix sequence.</param>
    /// <param name="aliasTemplate">The alias template.</param>
    public static AliasGenerator Create(string[] overriddenPrefixes, string aliasTemplate)
    {
      return new AliasGenerator(overriddenPrefixes, aliasTemplate);
    }


    // Constructors

    private AliasGenerator()
      : this (DefaultAliasTemplate)
    {}

    private AliasGenerator(string aliasTemplate)
      : this (DefaultPrefixSequence, aliasTemplate)
    {}

    private AliasGenerator(string[] prefixes)
      : this (prefixes, DefaultAliasTemplate)
    {}

    private AliasGenerator(string[] prefixes, string aliasTemplate)
    {
      prefixSequence = prefixes;
      this.aliasTemplate = aliasTemplate;
    }
  }
}