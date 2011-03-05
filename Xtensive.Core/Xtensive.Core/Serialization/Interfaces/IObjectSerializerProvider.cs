// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;

namespace Xtensive.Serialization
{
  /// <summary>
  /// <see cref="IObjectSerializer{T}"/> and <see cref="IObjectSerializer"/> provider.
  /// </summary>
  public interface IObjectSerializerProvider
  {
    /// <summary>
    /// Gets the serializer for specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to provide the serializer for.</typeparam>
    /// <returns>Serializer for the specified type.</returns>
    ObjectSerializer<T> GetSerializer<T>();

    /// <summary>
    /// Gets the serializer for specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to provide the serializer for.</param>
    /// <returns>Serializer for the specified type.</returns>
    IObjectSerializer GetSerializer(Type type);

    /// <summary>
    /// Gets the serializer for type of <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">The object which type is used to provide the serializer for.</param>
    /// <returns>Serializer for the specified type.</returns>
    IObjectSerializer GetSerializerByInstance(object instance);

    /// <summary>
    /// Gets the value serializer provider.
    /// </summary>
    IValueSerializerProvider ValueSerializerProvider { get; }
  }
}