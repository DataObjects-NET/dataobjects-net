// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.20

using System;

namespace Xtensive.Indexing.Serialization
{
  /// <summary>
  /// Typed object serializer and deserializer.
  /// </summary>
  /// <typeparam name="T">Type of the object to serialize and deserialize.</typeparam>
  public interface IObjectSerializer<T> : IObjectSerializer
  {
    /// <summary>
    /// Creates the object with "initial" state.
    /// </summary>
    /// <param name="type">The type of the object to create.</param>
    /// <returns>Newly created object.</returns>
    new T CreateObject(Type type);

    /// <summary>
    /// Populates the provided <see cref="SerializationData"/> with the differences
    /// between <paramref name="source"/> and <paramref name="origin"/>.
    /// </summary>
    /// <param name="source">The object to serialize.</param>
    /// <param name="origin">The origin - the "initial" object state (see <see cref="CreateObject"/>) 
    /// which shouldn't be serialized.</param>
    /// <param name="data">The <see cref="SerializationData"/> to populate.</param>
    void GetObjectData(T source, T origin, SerializationData data);

    /// <summary>
    /// Updates the object using the information in the <see cref="SerializationData"/>.
    /// </summary>
    /// <param name="source">The object to update.</param>
    /// <param name="data">The data to update the object from.</param>
    /// <returns>Updated object.</returns>
    T SetObjectData(T source, SerializationData data);
  }
}