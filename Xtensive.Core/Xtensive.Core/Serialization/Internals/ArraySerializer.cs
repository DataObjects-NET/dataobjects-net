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
    private const string ItemsPropertyName = "Content";

    public override T[] CreateObject(Type type)
    {
      return new T[0];
    }

    public override void GetObjectData(T[] source, T[] origin, SerializationData data)
    {
      base.GetObjectData(source, origin, data);
      data.AddObjects(ItemsPropertyName, source, true);
    }

    public override T[] SetObjectData(T[] source, SerializationData data)
    {
      var list = data.GetObjects<T>(ItemsPropertyName);
      source = new T[list.Count];
      data.UpdateSource(source);
      for (int i = 0; i < source.Length; i++)
        source[i] = list[i];
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