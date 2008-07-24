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
  }
}