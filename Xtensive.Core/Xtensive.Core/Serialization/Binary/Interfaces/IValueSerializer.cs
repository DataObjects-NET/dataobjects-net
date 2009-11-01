// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System.IO;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Serializes (deserializes) the objects to (from) the stream.
  /// </summary>
  public interface IValueSerializer: IContext<ValueSerializationScope>
  {
    /// <summary>
    /// Serializes an object to the provided stream at the current stream position.
    /// </summary>
    /// <param name="stream">The stream to serialize the data to.</param>
    /// <param name="graph">The object to serialize.</param>
    void Serialize(Stream stream, object graph);

    /// <summary>
    /// Deserializes the data at the current position of the provided stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize the data from.</param>
    /// <returns>Deserialization result.</returns>
    object Deserialize(Stream stream);
  }
}