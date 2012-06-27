// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.IO;

namespace Xtensive.Indexing.Serialization
{
  [Serializable]
  internal sealed class ArrayValueSerializer<T> : WrappingValueSerializer<T[], T, long>
  {
    public override T[] Deserialize(Stream stream)
    {
      long length = BaseSerializer2.Deserialize(stream);
      var value = new T[length];
      for (int i = 0; i < value.Length; i++)
        value[i] = BaseSerializer1.Deserialize(stream);
      return value;
    }

    public override void Serialize(Stream stream, T[] value)
    {
      BaseSerializer2.Serialize(stream, value.LongLength);
      for (int i = 0; i < value.Length; i++)
        BaseSerializer1.Serialize(stream, value[i]);
    }


    // Constructors
    
    public ArrayValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
    }
  }
}