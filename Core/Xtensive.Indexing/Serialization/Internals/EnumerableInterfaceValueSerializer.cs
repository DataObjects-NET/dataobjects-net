// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.Collections.Generic;
using System.IO;

namespace Xtensive.Indexing.Serialization
{
  [Serializable]
  internal sealed class EnumerableInterfaceValueSerializer<TEnumerable, T> : WrappingValueSerializer<TEnumerable, T, long>
    where TEnumerable: class, IEnumerable<T>
  {
    public override TEnumerable Deserialize(Stream stream)
    {
      long length = BaseSerializer2.Deserialize(stream);
      var value = new T[length];
      for (int i = 0; i < value.Length; i++)
        value[i] = BaseSerializer1.Deserialize(stream);
      return (TEnumerable) (object) value;
    }

    public override void Serialize(Stream stream, TEnumerable value)
    {
      long position = stream.Position;
      BaseSerializer2.Serialize(stream, (long) 0);
      long length = 0;
      foreach (var item in value) {
        BaseSerializer1.Serialize(stream, item);
        length++;
      }
      long endPosition = stream.Position;
      try {
        stream.Position = position;
        BaseSerializer2.Serialize(stream, length);
      }
      finally {
        stream.Position = endPosition;
      }
    }


    // Constructors
    
    public EnumerableInterfaceValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
    }
  }
}