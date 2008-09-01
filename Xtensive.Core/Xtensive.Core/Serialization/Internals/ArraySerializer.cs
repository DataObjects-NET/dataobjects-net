// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Serialization.Internals
{
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

    public override T[] SetObjectData(T[] source, SerializationData data)
    {
      int length = data.GetValue<int>(LengthPropertyName);
      source = new T[length];
      data.UpdateSource(source);
      for (int i = 0; i < source.Length; i++)
        source[i] = data.GetObject<T>(i.ToString());
      return source;
    }


    // Constructors

    /// <exception cref="InvalidOperationException">Similar value serializer exists,
    /// so it should be preferred.</exception>
    public ArraySerializer(IObjectSerializerProvider provider)
      : base(provider)
    {
      if (Provider.ValueSerializerProvider.GetSerializer<T[]>()!=null)
        // Let's "discard" ourselves if there is appropriate value serializer
        throw new InvalidOperationException(
          Strings.ExInvalidObjectSerializerSimilarValueSerializerExists); 
    }
  }
}