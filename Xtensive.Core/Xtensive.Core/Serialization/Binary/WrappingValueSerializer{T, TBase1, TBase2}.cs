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
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingValueSerializer<T, TBase1, TBase2> : BinaryValueSerializerBase<T>
  {
    /// <summary>
    /// Serializer for first base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<Stream, TBase1> BaseValueSerializer1;

    /// <summary>
    /// Serializer for second base (wrapped) type.
    /// </summary>
    protected ValueSerializerStruct<Stream, TBase2> BaseValueSerializer2;

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Serializer provider this serializer is bound to.</param>
    public WrappingValueSerializer(IBinaryValueSerializerProvider provider)
      : base(provider) {
      var serializer1 = provider.GetSerializer<TBase1>() as ValueSerializer<Stream, TBase1>;
      if (serializer1 == null)
        throw new InvalidCastException("Provider returned not a ValueSerializer<Stream, TBase1> object.");
      BaseValueSerializer1 = new ValueSerializerStruct<Stream, TBase1>(serializer1);
      var serializer2 = provider.GetSerializer<TBase2>() as ValueSerializer<Stream, TBase2>;
      if (serializer2 == null)
        throw new InvalidCastException("Provider returned not a ValueSerializer<Stream, TBase2> object.");
      BaseValueSerializer2 = new ValueSerializerStruct<Stream, TBase2>(serializer2);
    }
  }
}