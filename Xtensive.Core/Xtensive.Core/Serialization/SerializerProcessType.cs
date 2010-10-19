// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

namespace Xtensive.Serialization
{
  /// <summary>
  /// Enumerates possible serialization process types.
  /// </summary>
  public enum SerializerProcessType
  {
    /// <summary>
    /// Serializer neither serializes nor deserializes the graph now.
    /// </summary>
    None = 0,

    /// <summary>
    /// Serializer is serializing the graph.
    /// </summary>
    Serialization = 1,

    /// <summary>
    /// Serializer is deserializing the graph.
    /// </summary>
    Deserialization = -1,
  }
}