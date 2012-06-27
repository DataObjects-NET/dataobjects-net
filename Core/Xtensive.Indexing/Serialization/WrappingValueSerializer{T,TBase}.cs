// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using Xtensive.Indexing.Resources;
using Xtensive.Reflection;

namespace Xtensive.Indexing.Serialization
{
  /// <summary>
  /// Base class for any wrapping <see cref="IValueSerializer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to serialize.</typeparam>
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingValueSerializer<T, TBase> : ValueSerializerBase<T>
  {
    /// <summary>
    /// Serializer for base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<TBase> BaseSerializer;


    // Constructors

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Value serializer for <typeparamref name="TBase"/> is not found.</exception>
    protected WrappingValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      var serializer = GetValueSerializer<TBase>();
      BaseSerializer = new ValueSerializerStruct<TBase>(serializer);
    }
  }
}