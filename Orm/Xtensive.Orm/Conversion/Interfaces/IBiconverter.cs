// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.07

namespace Xtensive.Conversion
{
  /// <summary>
  /// Provides bidirectional conversion support.
  /// </summary>
  /// <typeparam name="TFrom">The 1st type to convert.</typeparam>
  /// <typeparam name="TTo">The 2nd type to convert.</typeparam>
  public interface IBiconverter<TFrom, TTo>
  {
    /// <summary>
    /// Converts the value forward.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>Conversion result.</returns>
    TTo ConvertForward(TFrom value);

    /// <summary>
    /// Converts the value backward.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>Conversion result.</returns>
    TFrom ConvertBackward(TTo value);
  }
}