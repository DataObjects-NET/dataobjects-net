// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;

namespace Xtensive.Core.Serialization.Internals
{
  [Serializable]
  internal class ObjectSerializer<T> : ObjectSerializerBase<T>
  {
    protected const string ValuePropertyName = "Value";

    public override T CreateObject(Type type)
    {
      return default(T);
    }

    public override void GetObjectData(T source, T origin, SerializationData data)
    {
      base.GetObjectData(source, origin, data);
      data.AddValue(ValuePropertyName, source);
    }

    public override T SetPropertyData(T target, SerializationData data, string propertyName)
    {
      if (propertyName==ValuePropertyName)
        return data.GetValue<T>(ValuePropertyName);
      return target;
    }


    // Constructors

    public ObjectSerializer(IObjectSerializerProvider provider)
      : base(provider)
    {
    }
  }
}