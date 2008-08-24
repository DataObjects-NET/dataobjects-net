// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.IO;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Base class for any wrapping <see cref="IValueSerializer{Stream, T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to serialize.</typeparam>
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingValueSerializer<T, TBase> : BinaryValueSerializerBase<T>
  {
    /// <summary>
    /// Serializer for base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<Stream, TBase> baseValueSerializer;

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Serializer provider this serializer is bound to.</param>
    public WrappingValueSerializer(IBinaryValueSerializerProvider provider)
      : base(provider) {
      var serializer = provider.GetSerializer<TBase>() as ValueSerializer<Stream, TBase>;
      if (serializer == null)
        throw new InvalidCastException("Provider returned not a ValueSerializer<Stream, TBase> object.");
      baseValueSerializer = new ValueSerializerStruct<Stream, TBase>(serializer);
    }
  }
}