// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Tagging interface for any serializer supported by
  /// <see cref="ValueSerializerProvider"/>.
  /// </summary>
  public interface IValueSerializerBase
  {
    /// <summary>
    /// Gets the provider this serializer is associated with.
    /// </summary>
    IValueSerializerProvider Provider { get; }
  }
}