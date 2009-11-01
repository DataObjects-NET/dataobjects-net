// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// <see cref="Serializer{T}"/> provider.
  /// </summary>
  public interface ISerializerProvider
  {
    /// <summary>
    /// Gets the serializer for specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to provide the serializer for.</typeparam>
    /// <returns>Serializer for the specified type.</returns>
    Serializer<T> GetObjectSerializer<T>();

    /// <summary>
    /// Gets the serializer for specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to provide the serializer for.</param>
    /// <returns>Serializer for the specified type.</returns>
    ISerializer GetObjectSerializer(Type type);
  }
}