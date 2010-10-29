// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System.IO;

namespace Xtensive.Serialization
{
  /// <summary>
  /// Serializes (deserializes) the objects 
  /// of the specified type <typeparamref name="T"/>
  /// to (from) the stream.
  /// </summary>
  /// <typeparam name="T">Type of object to serialize or deserialize.</typeparam>
  public interface IValueSerializer<T> : IValueSerializer
  {
    /// <summary>
    /// Serializes an object to the provided stream at the current stream position.
    /// </summary>
    /// <param name="stream">The stream to serialize the data to.</param>
    /// <param name="value">The object to serialize.</param>
    void Serialize(Stream stream, T value);

    /// <summary>
    /// Deserializes the data at the current position of the provided stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize the data from.</param>
    /// <returns>Deserialization result.</returns>
    new T Deserialize(Stream stream);
  }
}