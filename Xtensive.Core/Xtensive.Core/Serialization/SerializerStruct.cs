// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.27

using System;
using System.Runtime.Serialization;

namespace Xtensive.Core.Serialization
{
  public struct SerializerStruct<T> : ISerializable
  {
    public static SerializerStruct<T> Default = new SerializerStruct<T>(Serializer<T>.Default);

    public Serializer<T> Serializer;

    public Action<T , SerializationData, SerializationContext> GetObjectData;

    public Action<T, SerializationData, SerializationContext> SetObjectData;

    public Func<SerializationData, SerializationContext, T> CreateObject;

    public static implicit operator SerializerStruct<T>(Serializer<T> serializer)
    {
      return new SerializerStruct<T>(serializer);
    }


    // Constructors

    private SerializerStruct(SerializationInfo info, StreamingContext context)
    {
      Serializer = (Serializer<T>)info.GetValue("ObjectSerializer", typeof (Serializer<T>));
      GetObjectData = Serializer.GetObjectData;
      SetObjectData = Serializer.SetObjectData;
      CreateObject = Serializer.CreateObject;
    }

    public SerializerStruct(Serializer<T> serializer)
    {
      Serializer = serializer;
      GetObjectData = serializer.GetObjectData;
      SetObjectData = serializer.SetObjectData;
      CreateObject = serializer.CreateObject;
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Serializer", Serializer);
    }
  }
}