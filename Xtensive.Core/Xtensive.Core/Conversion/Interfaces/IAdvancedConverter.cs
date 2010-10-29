// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

namespace Xtensive.Conversion
{
  /// <summary>
  /// Converts values or instances of type <typeparamref name="TFrom"/> to 
  /// values or instances of type <typeparamref name="TTo"/>.
  /// </summary>
  /// <typeparam name="TFrom">The type of value to convert.</typeparam>
  /// <typeparam name="TTo">The type of converted value.</typeparam>
  public interface IAdvancedConverter<TFrom, TTo> : IAdvancedConverterBase
  {
    ///<summary>
    /// Converts specified value of <typeparamref name="TFrom"/> type
    /// to <typeparamref name="TTo"/> type.
    ///</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>Converted value.</returns>
    TTo Convert(TFrom value);

    /// <summary>
    /// Gets <see langword="true"/> if converter is rough, otherwise gets <see langword="false"/>.
    /// </summary>
    bool IsRough { get; }
  }
}