// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// A struct providing faster access for key <see cref="ValueSerializer{T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IValueSerializer{T}"/> generic argument.</typeparam>
  [Serializable]
  public struct ValueSerializerStruct<T> : ISerializable
  {
    /// <summary>
    /// Gets <see cref="ValueSerializerStruct{T}"/> for <see cref="ValueSerializer{T}.Default"/> hasher.
    /// </summary>
    public static readonly ValueSerializerStruct<T> Default = 
      new ValueSerializerStruct<T>(ValueSerializer<T>.Default);

    /// <summary>
    /// Gets the underlying serializer for this cache.
    /// </summary>
    public readonly ValueSerializer<T> ValueSerializer;

    /// <summary>
    /// Deserializes the data on the provided stream.
    /// </summary>
    public readonly Func<Stream, T> Deserialize;

    /// <summary>
    /// Serializes an object to the provided stream.
    /// </summary>
    public readonly Action<Stream, T> Serialize;

    /// <summary>
    /// Implicit conversion of <see cref="ValueSerializer{T}"/> to <see cref="ValueSerializerStruct{T}"/>.
    /// </summary>
    /// <param name="valueSerializer">Serializer to provide the struct for.</param>
    public static implicit operator ValueSerializerStruct<T>(ValueSerializer<T> valueSerializer) 
    {
      return new ValueSerializerStruct<T>(valueSerializer);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="valueSerializer"><see cref="ValueSerializer{T}"/> to provide the delegates for.</param>
    public ValueSerializerStruct(ValueSerializer<T> valueSerializer) 
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
        (ValueSerializer<T>) info.GetValue("ValueSerializer", typeof (ValueSerializer<T>));
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