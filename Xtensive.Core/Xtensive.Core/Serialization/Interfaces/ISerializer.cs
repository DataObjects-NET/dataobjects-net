// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.20

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Object serializer/deserializer.
  /// </summary>
  public interface ISerializer
  {
    /// <summary>
    /// Creates the object.
    /// </summary>
    /// <param name="data">The data that may be required for object creation.</param>
    /// <param name="context">The context for object creation.</param>
    object CreateObject(SerializationData data, SerializationContext context);

    /// <summary>
    /// Populates the provided <see cref="SerializationData"/> with the data needed to serialize the object.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="data">The <see cref="SerializationData"/> to populate with data.</param>
    /// <param name="context">The <see cref="SerializationContext"/> for this serialization.</param>
    void GetObjectData(object obj, SerializationData data, SerializationContext context);

    /// <summary>
    /// Populates the object using the information in the <see cref="SerializationData"/>.
    /// </summary>
    /// <param name="obj">The object to populate.</param>
    /// <param name="data">The data to populate the object.</param>
    /// <param name="context">The serialization context for this deserialization.</param>
    void SetObjectData(object obj, SerializationData data, SerializationContext context);
  }
}