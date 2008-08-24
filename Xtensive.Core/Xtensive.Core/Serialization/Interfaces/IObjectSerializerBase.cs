// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by:
// Created:

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Tagging interface for any serializer supported by 
  /// <see cref="IObjectSerializerProvider"/>.
  /// </summary>
  public interface IObjectSerializerBase
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
  }
}