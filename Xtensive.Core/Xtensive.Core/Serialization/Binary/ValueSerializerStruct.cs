// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// A struct providing faster access for key <see cref="ValueSerializer{T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IValueSerializer{T}"/> generic argument.</typeparam>
  [Serializable]
  public struct ValueSerializerStruct<T>
  {
    /// <summary>
    /// Gets <see cref="ValueSerializerStruct{T}"/> for <see cref="ValueSerializer{T}.Default"/> serializer.
    /// </summary>
    public static readonly ValueSerializerStruct<T> Default = new ValueSerializerStruct<T>(ValueSerializer<T>.Default);


    /// <summary>
    /// Gets the underlying serializer for this cache.
    /// </summary>
    public readonly ValueSerializer<T> valueSerializer;

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
      this.valueSerializer = valueSerializer;
      Deserialize = valueSerializer==null ? null : valueSerializer.Deserialize;
      Serialize = valueSerializer==null ? null : valueSerializer.Serialize;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private ValueSerializerStruct(System.Runtime.Serialization.SerializationInfo info, StreamingContext context)
    {
      valueSerializer = (ValueSerializer<T>) info.GetValue("ValueSerializer", typeof (ValueSerializer<T>));
      Deserialize = valueSerializer==null ? null : valueSerializer.Deserialize;
      Serialize = valueSerializer==null ? null : valueSerializer.Serialize;
    }

    /// <inheritdoc/>
    public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, StreamingContext context)
    {
      info.AddValue("ValueSerializer", valueSerializer);
    }
  }
}