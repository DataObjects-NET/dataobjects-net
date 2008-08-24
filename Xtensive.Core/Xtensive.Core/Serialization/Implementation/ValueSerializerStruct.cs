// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// A struct providing faster access for key <see cref="ValueSerializer{TStream,T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IValueSerializer{T}"/> generic argument.</typeparam>
  /// <typeparam name="TStream">Type of the stream to write to or read from.</typeparam>
  [Serializable]
  public struct ValueSerializerStruct<TStream, T>
  {
    /// <summary>
    /// Gets <see cref="ValueSerializerStruct{TStream,T}"/> for <see cref="ValueSerializer{TStream,T}.Default"/> serializer.
    /// </summary>
    public static readonly ValueSerializerStruct<TStream, T> Default =
      new ValueSerializerStruct<TStream, T>(ValueSerializer<TStream, T>.Default);

    /// <summary>
    /// Gets the underlying serializer for this cache.
    /// </summary>
    public readonly ValueSerializer<TStream, T> valueSerializer;

    /// <summary>
    /// Deserializes the data on the provided stream.
    /// </summary>
    public readonly Func<TStream, T> Deserialize;

    /// <summary>
    /// Serializes an object to the provided stream.
    /// </summary>
    public readonly Action<TStream, T> Serialize;

    /// <summary>
    /// Implicit conversion of <see cref="ValueSerializer{TStream,T}"/> to <see cref="ValueSerializerStruct{TStream,T}"/>.
    /// </summary>
    /// <param name="valueSerializer">Serializer to provide the struct for.</param>
    public static implicit operator ValueSerializerStruct<TStream, T>(ValueSerializer<TStream, T> valueSerializer) {
      return new ValueSerializerStruct<TStream, T>(valueSerializer);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="valueSerializer"><see cref="ValueSerializer{TStream,T}"/> to provide the delegates for.</param>
    public ValueSerializerStruct(ValueSerializer<TStream, T> valueSerializer) {
      this.valueSerializer = valueSerializer;
      Deserialize = valueSerializer == null ? null : valueSerializer.Deserialize;
      Serialize = valueSerializer == null ? null : valueSerializer.Serialize;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private ValueSerializerStruct(SerializationInfo info, StreamingContext context) {
      valueSerializer =
        (ValueSerializer<TStream, T>) info.GetValue("ValueSerializer", typeof (ValueSerializer<TStream, T>));
      Deserialize = valueSerializer == null ? null : valueSerializer.Deserialize;
      Serialize = valueSerializer == null ? null : valueSerializer.Serialize;
    }

    /// <inheritdoc/>
    public void GetObjectData(SerializationInfo info, StreamingContext context) {
      info.AddValue("ValueSerializer", valueSerializer);
    }
  }
}