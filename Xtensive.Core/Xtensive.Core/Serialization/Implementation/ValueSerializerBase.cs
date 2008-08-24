// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Base class for any <see cref="IValueSerializer{T}"/>.
  /// </summary>
  /// <typeparam name="T">Type of object to serialize or deserialize.</typeparam>
  /// <typeparam name="TStream">Type of the stream to write to or read from.</typeparam>
  [Serializable]
  public abstract class ValueSerializerBase<TStream, T> :
    IValueSerializer<TStream, T>,
    IDeserializationCallback
  {
    private IValueSerializerProvider<TStream> provider;

    /// <inheritdoc/>
    [DebuggerHidden]
    public IValueSerializerProvider<TStream> Provider {
      get { return provider; }
    }

    /// <inheritdoc/>
    public abstract T Deserialize(TStream stream);

    /// <inheritdoc/>
    public abstract void Serialize(TStream stream, T value);

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Serializer provider this serializer is bound to.</param>
    protected ValueSerializerBase(IValueSerializerProvider<TStream> provider) {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      this.provider = provider;
    }

    /// <see cref="SerializableDocTemplate.OnDeserialization" copy="true" />
    public virtual void OnDeserialization(object sender) {
      if (provider == null || provider.GetType() == typeof (ValueSerializerProvider<TStream>))
        provider = ValueSerializerProvider<TStream>.Default;
    }

    #region IValueSerializer<TStream> Members

    void IValueSerializer<TStream>.Serialize(TStream stream, object value) {
      Serialize(stream, (T) value);
    }

    object IValueSerializer<TStream>.Deserialize(TStream stream) {
      return Deserialize(stream);
    }

    #endregion
  }
}