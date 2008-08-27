// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// <see cref="IValueSerializer{TStream, T}"/> and <see cref="IValueSerializer{TStream}"/> provider.
  /// </summary>
  /// <typeparam name="TStream">Type of the stream to write to or read from.</typeparam>
  public interface IValueSerializerProvider<TStream>
  {
    /// <summary>
    /// Gets the serializer for specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to provide the serializer for.</typeparam>
    /// <returns>Serializer for the specified type.</returns>
    ValueSerializer<TStream, T> GetSerializer<T>();

    /// <summary>
    /// Gets the serializer for specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to provide the serializer for.</param>
    /// <returns>Serializer for the specified type.</returns>
    IValueSerializer<TStream> GetSerializer(Type type);

    /// <summary>
    /// Gets the serializer for type of <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">The object which type is used to provide the serializer for.</param>
    /// <returns>Serializer for the specified type.</returns>
    IValueSerializer<TStream> GetSerializerByInstance(object instance);
  }
}