// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;

namespace Xtensive.Core.Serialization.Internals
{
  [Serializable]
  internal sealed class ArraySerializer<T> : ObjectSerializerBase<T[]>
  {
    private const string LengthPropertyName = "Length";

    public override T[] CreateObject(Type type)
    {
      return new T[0];
    }

    public override void GetObjectData(T[] source, T[] origin, SerializationData data)
    {
      base.GetObjectData(source, origin, data);
      data.AddValue(LengthPropertyName, source.Length);
      for (int i = 0; i < source.Length; i++)
        data.AddObject(i.ToString(), source[i], true);
    }

    public override T[] SetObjectData(T[] target, SerializationData data)
    {
      int length = data.GetValue<int>(LengthPropertyName);
      target = new T[length];
      for (int i = 0; i < target.Length; i++)
        target[i] = data.GetObject<T>(i.ToString());
      data.EnsureNoSkips();
      return target;
    }

    /// <exception cref="NotSupportedException">Thrown always by this method.</exception>
    public override T[] SetPropertyData(T[] target, SerializationData data, string propertyName)
    {
      throw new NotSupportedException();
    }


    // Constructors

    public ArraySerializer(IObjectSerializerProvider provider)
      : base(provider)
    {
    }
  }
}