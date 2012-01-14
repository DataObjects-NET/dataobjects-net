// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using Xtensive.Reflection;
using Xtensive.Resources;

namespace Xtensive.Serialization
{
  /// <summary>
  /// Base class for any wrapping <see cref="IValueSerializer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to serialize.</typeparam>
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingValueSerializer<T, TBase1, TBase2> : ValueSerializerBase<T>
  {
    /// <summary>
    /// Serializer for first base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<TBase1> BaseSerializer1;

    /// <summary>
    /// Serializer for second base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<TBase2> BaseSerializer2;


    // Constructors

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Value serializer for 
    /// <typeparamref name="TBase1"/> or <typeparamref name="TBase2"/> is not found.</exception>
    protected WrappingValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      var serializer1 = GetValueSerializer<TBase1>();
      BaseSerializer1 = new ValueSerializerStruct<TBase1>(serializer1);
      var serializer2 = GetValueSerializer<TBase2>();
      BaseSerializer2 = new ValueSerializerStruct<TBase2>(serializer2);
    }
  }
}