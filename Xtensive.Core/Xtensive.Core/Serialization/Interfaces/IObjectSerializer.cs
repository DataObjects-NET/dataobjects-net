// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.20

using System;

namespace Xtensive.Serialization
{
  /// <summary>
  /// Untyped object serializer / deserializer.
  /// </summary>
  public interface IObjectSerializer
  {
    /// <summary>
    /// Gets the provider this serializer is associated with.
    /// </summary>
    IObjectSerializerProvider Provider { get; }

    /// <summary>
    /// Gets the value specified that serializable type can be referred 
    /// by a set of other objects in serialized graph.
    /// </summary>
    bool IsReferable { get; }

    /// <summary>
    /// Creates the object with "initial" state.
    /// </summary>
    /// <param name="type">The type of the object to create.</param>
    /// <returns>Newly created object.</returns>
    object CreateObject(Type type);

    /// <summary>
    /// Populates the provided <paramref name="data"/> with the differences
    /// between <see cref="SerializationData.Source"/> and <see cref="SerializationData.Origin"/>.
    /// </summary>
    /// <param name="data">The <see cref="SerializationData"/> to populate.</param>
    void GetObjectData(SerializationData data);

    /// <summary>
    /// Populates the <see cref="SerializationData.Source"/> object 
    /// from <see cref="SerializationData.Origin"/> by applying the
    /// changes stored in <paramref name="data"/> to it.
    /// </summary>
    /// <param name="data">The <see cref="SerializationData"/> to use.</param>
    /// <returns>Updated object.</returns>
    void SetObjectData(SerializationData data);
  }
}