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
  /// Base class for any <see cref="IValueSerializer{TStream,T}"/>.
  /// </summary>
  /// <typeparam name="TStream">Type of the stream to write to or read from.</typeparam>
  /// <typeparam name="T">Type of value to serialize or deserialize.</typeparam>
  [Serializable]
  public abstract class ValueSerializerBase<TStream, T> : 
    IValueSerializer<TStream, T>,
    IDeserializationCallback
  {
    /// <inheritdoc/>
    public IValueSerializerProvider<TStream> Provider { get; protected set; }

    /// <inheritdoc/>
    public abstract T Deserialize(TStream stream);

    /// <inheritdoc/>
    public abstract void Serialize(TStream stream, T value);

    #region IValueSerializer<TStream> Members

    void IValueSerializer<TStream>.Serialize(TStream stream, object value) 
    {
      Serialize(stream, (T) value);
    }

    object IValueSerializer<TStream>.Deserialize(TStream stream) 
    {
      return Deserialize(stream);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Serializer provider this serializer is bound to.</param>
    protected ValueSerializerBase(IValueSerializerProvider<TStream> provider) 
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      Provider = provider;
    }

    // IDeserializationCallback methods

    /// <see cref="SerializableDocTemplate.OnDeserialization" copy="true" />
    public abstract void OnDeserialization(object sender);
  }
}