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
  public abstract class ObjectSerializerBase<T> : IObjectSerializer<T>
  {
    private readonly IObjectSerializerProvider provider;

    /// <inheritdoc/>
    [DebuggerHidden]
    public IObjectSerializerProvider Provider {
      get { return provider; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    public virtual bool IsReferable {
      get { return true; }
    }

    #region IObjectSerializer Members

    void IObjectSerializer.GetObjectData(object obj, SerializationData data) {
      GetObjectData((T) obj, data);
    }

    object IObjectSerializer.SetObjectData(object obj, SerializationData data) {
      var tObj = (T) obj;
      return SetObjectData(tObj, data);
    }

    object IObjectSerializer.CreateObject() {
      return CreateObject();
    }

    #endregion

    #region IObjectSerializer<T> members

    /// <inheritdoc/>
    public abstract T CreateObject();

    /// <inheritdoc/>
    public abstract void GetObjectData(T obj, SerializationData data);

    /// <inheritdoc/>
    public abstract T SetObjectData(T obj, SerializationData data);

    #endregion

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The provider.</param>
    protected ObjectSerializerBase(IObjectSerializerProvider provider) {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      this.provider = provider;
    }
  }
}