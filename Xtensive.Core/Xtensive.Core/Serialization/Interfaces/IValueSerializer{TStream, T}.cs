// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Serializes (deserializes) the objects 
  /// of the specified type <typeparamref name="T"/>
  /// to (from) the stream.
  /// </summary>
  /// <typeparam name="TStream">Type of the stream to write to or read from.</typeparam>
  /// <typeparam name="T">Type of object to serialize or deserialize.</typeparam>
  public interface IValueSerializer<TStream, T> : IValueSerializer<TStream>
  {
    /// <summary>
    /// Serializes an object to the provided stream at the current stream position.
    /// </summary>
    /// <param name="stream">The stream to serialize the data to.</param>
    /// <param name="value">The object to serialize.</param>
    void Serialize(TStream stream, T value);

    /// <summary>
    /// Deserializes the data at the current position of the provided stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize the data from.</param>
    /// <returns>Deserialization result.</returns>
    new T Deserialize(TStream stream);
  }
}