// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Base class for any wrapping <see cref="IValueSerializer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to serialize.</typeparam>
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingValueSerializer<T, TBase> : ValueSerializerBase<T>
  {
    /// <summary>
    /// Serializer for base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<TBase> baseValueSerializer;

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Serializer provider this serializer is bound to.</param>
    public WrappingValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      baseValueSerializer = provider.GetSerializer<TBase>();
    }
  }
}