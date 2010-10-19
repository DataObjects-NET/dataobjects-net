// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.28

using System;

namespace Xtensive.Serialization.Implementation
{
  /// <summary>
  /// Typed version of <see cref="Token"/>.
  /// </summary>
  /// <typeparam name="T">Type of the value.</typeparam>
  [Serializable]
  public sealed class Token<T> : Token
  {
    /// <summary>
    /// Gets or sets the value of the token.
    /// </summary>
    public new T Value {
      get { return (T) base.Value; }
    }


    // Constructors

    /// <inheritdoc/>
    public Token(T value, int identifier)
      : base(value, identifier)
    {
      SerializationContext.Current.TokenManager.Add(this);
    }
  }
}