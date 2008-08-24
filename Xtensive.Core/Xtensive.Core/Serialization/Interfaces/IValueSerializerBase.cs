// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Tagging interface for any serializer supported by <see cref="IValueSerializerProvider{TStream}"/>.
  /// </summary>
  /// <typeparam name="TStream">Type of the stream to write to or read from.</typeparam>
  public interface IValueSerializerBase<TStream>
  {
    /// <summary>
    /// Gets the provider this serializer is associated with.
    /// </summary>
    IValueSerializerProvider<TStream> Provider { get; }
  }
}