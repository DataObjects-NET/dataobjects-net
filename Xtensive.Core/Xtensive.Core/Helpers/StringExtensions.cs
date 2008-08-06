// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.18

namespace Xtensive.Core.Helpers
{
  /// <summary>
  /// <see cref="string"/> related extension methods.
  /// </summary>
  public static class StringExtensions
  {
    /// <summary>
    /// Indicates whether the specified  <paramref name="value"/> 
    /// is <see langword="null"/> or is <see cref="string.Empty"/>.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <paramref name="value"/> 
    /// is <see langword="null"/> or is <see cref="string.Empty"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNullOrEmpty(this string value)
    {
      return string.IsNullOrEmpty(value);
    }
    
    /// <summary>
    /// Cuts the specified <paramref name="suffix"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The original string value.</param>
    /// <param name="suffix">The suffix to cut.</param>
    /// <returns>String without <paramref name="suffix"/> if it was found; 
    /// otherwise, original <paramref name="value"/>.</returns>
    public static string TryCutSuffix(this string  value, string suffix)
    {
      if (!value.EndsWith(suffix))
        return value;
      return value.Substring(0, value.Length - suffix.Length);        
    }
    
    /// <summary>
    /// Cuts the specified <paramref name="prefix"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The original string value.</param>
    /// <param name="prefix">The prefix to cut.</param>
    /// <returns>String without <paramref name="prefix"/> if it was found; 
    /// otherwise, original <paramref name="value"/>.</returns>
    public static string TryCutPrefix(this string  value, string prefix)
    {
      if (!value.StartsWith(prefix))
        return value;
      return value.Substring(prefix.Length);        
    }
  }
}