// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.03

using System;


namespace Xtensive.Conversion
{
  /// <summary>
  /// Delegate-based implementation.
  /// </summary>
  /// <typeparam name="TFrom">Type to convert from.</typeparam>
  /// <typeparam name="TTo">Type to convert to.</typeparam>
  [Serializable]
  public readonly struct Biconverter<TFrom, TTo> :
    IEquatable<Biconverter<TFrom, TTo>>
  {
    /// <summary>
    /// Gets the "as is" bidirectional converter.
    /// Note: it involves boxing on any conversion (for <see cref="ValueType"/>s).
    /// </summary>
    public static Biconverter<TFrom, TTo> AsIs {
      get {
        return new Biconverter<TFrom, TTo>(
          value => (TTo) (object) value,
          value => (TFrom) (object) value);
      }
    }

    /// <summary>
    /// Gets the delegate converting specified value 
    /// from <typeparamref name="TFrom"/>
    /// to <typeparamref name="TTo"/>.
    /// </summary>
    public readonly Converter<TFrom, TTo> ConvertForward;

    /// <summary>
    /// Gets the delegate converting specified value 
    /// from <typeparamref name="TTo"/>
    /// to <typeparamref name="TFrom"/>.
    /// </summary>
    public readonly Converter<TTo, TFrom> ConvertBackward;

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(Biconverter<TFrom, TTo> obj)
    {
      return Equals(obj.ConvertForward, ConvertForward) && Equals(obj.ConvertBackward, ConvertBackward);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      obj is Biconverter<TFrom, TTo> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return ((ConvertForward!=null ? ConvertForward.GetHashCode() : 0) * 397) ^ (ConvertBackward!=null ? ConvertBackward.GetHashCode() : 0);
      }
    }

    #endregion

    #region Operators: ==, !=

    public static bool operator ==(Biconverter<TFrom, TTo> left, Biconverter<TFrom, TTo> right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(Biconverter<TFrom, TTo> left, Biconverter<TFrom, TTo> right)
    {
      return !left.Equals(right);
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="convertForward">Forward converter.</param>
    /// <param name="convertBackward">Backward converter.</param>
    public Biconverter(Converter<TFrom, TTo> convertForward, Converter<TTo, TFrom> convertBackward)
    {
      ConvertForward  = convertForward;
      ConvertBackward = convertBackward;
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="forwardConverter">Forward converter.</param>
    /// <param name="backwardConverter">Backward converter.</param>
    public Biconverter(IAdvancedConverter<TFrom, TTo> forwardConverter, IAdvancedConverter<TTo, TFrom> backwardConverter)
    {
      ConvertForward  = forwardConverter.Convert;
      ConvertBackward = backwardConverter.Convert;
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="biconverter">The biconverter.</param>
    public Biconverter(IBiconverter<TFrom, TTo> biconverter)
    {
      ConvertForward  = biconverter.ConvertForward;
      ConvertBackward = biconverter.ConvertBackward;
    }
  }
}
