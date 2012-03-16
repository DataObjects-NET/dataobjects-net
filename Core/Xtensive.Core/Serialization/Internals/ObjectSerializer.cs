// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;

namespace Xtensive.Serialization.Internals
{
  internal sealed class ObjectSerializer<T> : ObjectSerializerBase<T>
  {
    public const string ValuePropertyName = "Value";
    private readonly bool isReferable = typeof (T).IsClass;
    private readonly ValueSerializer<T> valueSerializer;

    public override bool IsReferable {
      get {
        return isReferable;
      }
    }

    public override T CreateObject(Type type)
    {
      return default(T);
    }

    public override void GetObjectData(T source, T origin, SerializationData data)
    {
      base.GetObjectData(source, origin, data);
      data.AddValue(ValuePropertyName, source, valueSerializer);
    }

    public override T SetObjectData(T source, SerializationData data)
    {
      return data.GetValue(ValuePropertyName, valueSerializer);
    }

    // Constructors

    public ObjectSerializer(IObjectSerializerProvider provider)
      : base(provider)
    {
      valueSerializer = GetValueSerializer<T>();
    }
  }
}