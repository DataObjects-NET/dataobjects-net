// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System.IO;

namespace Xtensive.Indexing.Serialization
{
  /// <summary>
  /// Serializes (deserializes) the objects to (from) the stream.
  /// </summary>
  public interface IValueSerializer
  {
    /// <summary>
    /// Gets the provider this serializer is associated with.
    /// </summary>
    IValueSerializerProvider Provider { get; }

    /// <summary>
    /// Serializes an object to the provided stream at the current stream position.
    /// </summary>
    /// <param name="stream">The stream to serialize the data to.</param>
    /// <param name="value">The object to serialize.</param>
    void Serialize(Stream stream, object value);

    /// <summary>
    /// Deserializes the data at the current position of the provided stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize the data from.</param>
    /// <returns>Deserialization result.</returns>
    object Deserialize(Stream stream);
  }
}