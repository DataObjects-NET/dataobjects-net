// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.20

using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Base class for concrete object serializers.
  /// </summary>
  /// <typeparam name="T">Type of object to serialize/deserialize.</typeparam>
  public abstract class SerializerBase<T> : ISerializer<T>
  {
    private ISerializerProvider provider;

    /// <inheritdoc/>
    public ISerializerProvider Provider
    {
      [DebuggerStepThrough]
      get { return provider; }
    }

    /// <inheritdoc/>
    public abstract T CreateObject(SerializationData data, SerializationContext context);

    /// <inheritdoc/>
    public abstract void GetObjectData(T obj, SerializationData data, SerializationContext context);

    /// <inheritdoc/>
    public abstract void SetObjectData(T obj, SerializationData data, SerializationContext context);


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The provider.</param>
    protected SerializerBase(ISerializerProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      this.provider = provider;
    }
  }
}