// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.25

using System.IO;

namespace Xtensive.Indexing.Serialization
{
  /// <summary>
  /// Serializer contract.
  /// </summary>
  public interface ISerializer
  {
    /// <summary>
    /// Serializes the differences between <paramref name="source"/> object and the 
    /// <paramref name="origin"/> to the specified <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to serialize to.</param>
    /// <param name="source">The object graph to serialize.</param>
    /// <param name="origin">The original graph.</param>
    void Serialize(Stream stream, object source, object origin);

    /// <summary>
    /// Serializes the <paramref name="source"/> object 
    /// to the specified <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to serialize to.</param>
    /// <param name="source">The object graph to serialize.</param>
    void Serialize(Stream stream, object source);

    /// <summary>
    /// Deserializes the changes stored by <see cref="Serialize(Stream,object,object)"/>
    /// from the specified <paramref name="stream"/>
    /// by applying them to <paramref name="origin"/>.
    /// </summary>
    /// <param name="stream">The stream to populate the data from.</param>
    /// <param name="origin">The origin - the graph to update.</param>
    /// <returns>The deserialized (updated) graph.</returns>
    object Deserialize(Stream stream, object origin);

    /// <summary>
    /// Deserializes the object from the specified <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to populate the data from.</param>
    /// <returns>The deserialized graph.</returns>
    object Deserialize(Stream stream);

    /// <summary>
    /// Clones the specified <paramref name="source"/> object using this serializer.
    /// </summary>
    /// <param name="source">The graph to clone.</param>
    /// <returns>Cloned graph.</returns>
    object Clone(object source);
  }
}