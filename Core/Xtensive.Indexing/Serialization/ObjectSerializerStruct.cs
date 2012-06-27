// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.27

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Serialization
{
  /// <summary>
  /// A struct providing faster access for key <see cref="ObjectSerializer{T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IObjectSerializer{T}"/> generic argument.</typeparam>
  [Serializable]
  public struct ObjectSerializerStruct<T> : ISerializable
  {
    /// <summary>
    /// Gets the underlying serializer for this cache.
    /// </summary>
    public ObjectSerializer<T> Serializer;

    /// <summary>
    /// Populates the provided <see cref="SerializationData"/> with the data needed to serialize the object.
    /// </summary>
    public Action<T, T, SerializationData> GetObjectData;

    /// <summary>
    /// Populates the object using the information in the <see cref="SerializationData"/>.
    /// </summary>
    public Func<T, SerializationData, T> SetObjectData;

    /// <summary>
    /// Creates the object.
    /// </summary>
    public Func<Type, T> CreateObject;

    /// <summary>
    /// Implicit conversion of <see cref="ObjectSerializer{T}"/> to <see cref="ObjectSerializerStruct{T}"/>.
    /// </summary>
    /// <param name="serializer">Serializer to provide the struct for.</param>
    public static implicit operator ObjectSerializerStruct<T>(ObjectSerializer<T> serializer) 
    {
      return new ObjectSerializerStruct<T>(serializer);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="serializer"><see cref="ObjectSerializer{T}"/> to provide the delegates for.</param>
    public ObjectSerializerStruct(ObjectSerializer<T> serializer) 
    {
      ArgumentValidator.EnsureArgumentNotNull(serializer, "serializer");
      Serializer = serializer;
      GetObjectData = serializer.GetObjectData;
      SetObjectData = serializer.SetObjectData;
      CreateObject = serializer.CreateObject;
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true"/>
    private ObjectSerializerStruct(SerializationInfo info, StreamingContext context) 
    {
      Serializer = (ObjectSerializer<T>) info.GetValue("Serializer", typeof (ObjectSerializer<T>));
      GetObjectData = Serializer.GetObjectData;
      SetObjectData = Serializer.SetObjectData;
      CreateObject = Serializer.CreateObject;
    }

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true"/>
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) 
    {
      info.AddValue("Serializer", Serializer);
    }
  }
}