// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.IO;

namespace Xtensive.Core.Serialization.Binary
{
  [Serializable]
  internal sealed class ArrayValueSerializer<T> : WrappingBinaryValueSerializer<T[], T, int>
  {
    public override T[] Deserialize(Stream stream)
    {
      int length = BaseSerializer2.Deserialize(stream);
      var value = new T[length];
      for (int i = 0; i < value.Length; i++)
        value[i] = BaseSerializer1.Deserialize(stream);
      return value;
    }

    public override void Serialize(Stream stream, T[] value)
    {
      BaseSerializer2.Serialize(stream, value.Length);
      for (int i = 0; i < value.Length; i++)
        BaseSerializer1.Serialize(stream, value[i]);
    }


    // Constructors
    
    public ArrayValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
    }
  }
}