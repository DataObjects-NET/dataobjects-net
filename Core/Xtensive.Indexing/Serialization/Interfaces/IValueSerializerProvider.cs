// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using System.IO;
using Xtensive.Indexing.Serialization.Implementation;

namespace Xtensive.Indexing.Serialization
{
  /// <summary>
  /// <see cref="ValueSerializer{T}"/> and <see cref="IValueSerializer{T}"/> provider.
  /// </summary>
  public interface IValueSerializerProvider
  {
    /// <summary>
    /// Gets the serializer for specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to provide the serializer for.</typeparam>
    /// <returns>Serializer for the specified type.</returns>
    ValueSerializer<T> GetSerializer<T>();

    /// <summary>
    /// Gets the serializer for specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to provide the serializer for.</param>
    /// <returns>Serializer for the specified type.</returns>
    IValueSerializer GetSerializer(Type type);

    /// <summary>
    /// Gets the serializer for type of <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">The object which type is used to provide the serializer for.</param>
    /// <returns>Serializer for the specified type.</returns>
    IValueSerializer GetSerializerByInstance(object instance);
  }
}