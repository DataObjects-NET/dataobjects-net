// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Implementation
{
  /// <summary>
  /// A struct providing faster access for key <see cref="ValueSerializer{TStream,T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IValueSerializer{T}"/> generic argument.</typeparam>
  /// <typeparam name="TStream">Type of the stream to write to or read from.</typeparam>
  [Serializable]
  public struct ValueSerializerStruct<TStream, T> : ISerializable
  {
    /// <summary>
    /// Gets the underlying serializer for this cache.
    /// </summary>
    public readonly ValueSerializer<TStream, T> ValueSerializer;

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
    public static implicit operator ValueSerializerStruct<TStream, T>(ValueSerializer<TStream, T> valueSerializer) 
    {
      return new ValueSerializerStruct<TStream, T>(valueSerializer);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="valueSerializer"><see cref="ValueSerializer{TStream,T}"/> to provide the delegates for.</param>
    public ValueSerializerStruct(ValueSerializer<TStream, T> valueSerializer) 
    {
      ValueSerializer = valueSerializer;
      Deserialize = valueSerializer == null ? null : valueSerializer.Deserialize;
      Serialize = valueSerializer == null ? null : valueSerializer.Serialize;
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true"/>
    private ValueSerializerStruct(SerializationInfo info, StreamingContext context) 
    {
      ValueSerializer =
        (ValueSerializer<TStream, T>) info.GetValue("ValueSerializer", typeof (ValueSerializer<TStream, T>));
      Deserialize = ValueSerializer == null ? null : ValueSerializer.Deserialize;
      Serialize = ValueSerializer == null ? null : ValueSerializer.Serialize;
    }

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true"/>
    public void GetObjectData(SerializationInfo info, StreamingContext context) 
    {
      info.AddValue("ValueSerializer", ValueSerializer);
    }
  }
}