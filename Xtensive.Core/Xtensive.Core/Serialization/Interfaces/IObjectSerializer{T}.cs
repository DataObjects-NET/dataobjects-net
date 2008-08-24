// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.20

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Object serializer and deserializer.
  /// </summary>
  /// <typeparam name="T">Type of object to serialize and deserialize.</typeparam>
  public interface IObjectSerializer<T> : IObjectSerializer
  {
    /// <summary>
    /// Creates the object.
    /// </summary>
    new T CreateObject();

    /// <summary>
    /// Populates the provided <see cref="SerializationData"/> with the data needed to serialize the object.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="data">The <see cref="SerializationData"/> to populate with data.</param>
    void GetObjectData(T obj, SerializationData data);

    /// <summary>
    /// Populates the object using the information in the <see cref="SerializationData"/>.
    /// </summary>
    /// <param name="obj">The object to populate.</param>
    /// <param name="data">The data to populate the object.</param>
    /// <returns>Changed object.</returns>
    T SetObjectData(T obj, SerializationData data);
  }
}