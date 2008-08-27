// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.20

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Base class for any <see cref="IObjectSerializer{T}"/>.
  /// </summary>
  /// <typeparam name="T">Type of object to serialize / deserialize.</typeparam>
  [Serializable]
  public abstract class ObjectSerializerBase<T> : 
    IObjectSerializer<T>,
    IDeserializationCallback
  {
    /// <inheritdoc/>
    public IObjectSerializerProvider Provider { get; protected set; }

    /// <inheritdoc/>
    public virtual bool IsReferable {
      [DebuggerStepThrough]
      get { return true; }
    }

    /// <inheritdoc/>
    public abstract T CreateObject(Type type);

    /// <inheritdoc/>
    public virtual void GetObjectData(T source, T origin, SerializationData data)
    {
      GetObjectHeader(source, data);
    }

    /// <summary>
    /// Adds <paramref name="source"/> type and reference to the <paramref name="data"/>.
    /// </summary>
    /// <param name="source">The object to add the header for.</param>
    /// <param name="data">The <see cref="SerializationData"/> to update.</param>
    protected virtual void GetObjectHeader(T source, SerializationData data)
    {
      var type = GetObjectType(source);
      data.SerializedType = type;
      if (IsReferable)
        // It isn't a reference type, so it must have SerializedReference
        data.SerializedReference = data.Reference;
    }

    /// <summary>
    /// Gets the type of the object (used in <see cref="GetObjectData"/>).
    /// </summary>
    /// <param name="source">The object to get the type of.</param>
    /// <returns>The type of the object.</returns>
    public virtual Type GetObjectType(T source)
    {
      return source.GetType();
    }

    /// <inheritdoc/>
    public virtual T SetObjectData(T target, SerializationData data)
    {
      foreach (var propertyName in data)
        target = SetPropertyData(target, data, propertyName);
      data.EnsureNoSkips();
      return target;
    }

    /// <summary>
    /// Updates (sets) the particular property by specified <paramref name="data"/>.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="data">The serialization data.</param>
    /// <param name="propertyName">Name of the property to update.</param>
    /// <returns>An object with updated property.</returns>
    public abstract T SetPropertyData(T target, SerializationData data, string propertyName);

    #region IObjectSerializer Members

    /// <inheritdoc/>
    object IObjectSerializer.CreateObject(Type type) 
    {
      return CreateObject(type);
    }

    /// <inheritdoc/>
    void IObjectSerializer.GetObjectData(SerializationData data) 
    {
      GetObjectData((T) data.Source, (T) data.Origin, data);
    }

    /// <inheritdoc/>
    void IObjectSerializer.SetObjectData(SerializationData data) 
    {
      data.Source = SetObjectData((T) data.Origin, data);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">The provider.</param>
    protected ObjectSerializerBase(IObjectSerializerProvider provider) 
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      Provider = provider;
    }

    // IDeserializationCallback methods

    /// <see cref="SerializableDocTemplate.OnDeserialization" copy="true" />
    public virtual void OnDeserialization(object sender)
    {
      if (Provider==null || Provider.GetType()==typeof (ObjectSerializerProvider))
        Provider = ObjectSerializerProvider.Default;
    }
  }
}