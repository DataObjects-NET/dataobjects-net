// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Primitive serializer provider
  /// </summary>
  public interface IValueSerializerProvider
  {
    /// <summary>
    /// Gets <see cref="Formatter"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the serializer for.</typeparam>
    /// <returns><see cref="Formatter"/> for the specified type <typeparamref name="T"/>.</returns>
    ValueSerializer<T> GetSerializer<T>();
  }
}