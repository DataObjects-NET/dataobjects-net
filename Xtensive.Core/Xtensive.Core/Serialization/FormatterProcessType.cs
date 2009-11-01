// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.26

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Enumerates possible <see cref="Formatter"/> process types.
  /// </summary>
  public enum FormatterProcessType
  {
    /// <summary>
    /// <see cref="Formatter"/> is serializing the graph.
    /// </summary>
    Serialization = 1,

    /// <summary>
    /// <see cref="Formatter"/> is deserializing the graph.
    /// </summary>
    Deserialization = -1,
  }
}