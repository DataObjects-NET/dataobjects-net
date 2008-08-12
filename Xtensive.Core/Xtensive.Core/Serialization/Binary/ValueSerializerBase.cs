// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Base class for any <see cref="IValueSerializer{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type to serialize.</typeparam>
  [Serializable]
  public abstract class ValueSerializerBase<T> : 
    IValueSerializer<T>,
    IDeserializationCallback
  {
    private IValueSerializerProvider provider;

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public IValueSerializerProvider Provider
    {
      get { return provider; }
    }

    /// <inheritdoc/>
    public abstract T Deserialize(Stream stream);

    /// <inheritdoc/>
    public abstract void Serialize(Stream stream, T value);

  
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Serializer provider this serializer is bound to.</param>
    public ValueSerializerBase(IValueSerializerProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      this.provider = provider;
    }

    /// <see cref="SerializableDocTemplate.OnDeserialization" copy="true" />
    public virtual void OnDeserialization(object sender)
    {
      if (provider==null || provider.GetType()==typeof (ValueSerializerProvider))
        provider = ValueSerializerProvider.Default;
    }
  }
}