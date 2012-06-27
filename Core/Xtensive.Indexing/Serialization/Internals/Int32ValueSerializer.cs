// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;
using System.IO;

namespace Xtensive.Indexing.Serialization
{
  [Serializable]
  internal sealed class Int32ValueSerializer : ValueSerializerBase<int>
  {
    public override int Deserialize(Stream stream) 
    {
      var buffer = new byte[sizeof (int)];
      stream.Read(buffer, 0, sizeof (int));
      return BitConverter.ToInt32(buffer, 0);
    }

    public override void Serialize(Stream stream, int value) 
    {
      stream.Write(BitConverter.GetBytes(value), 0, sizeof (int));
    }


    // Constructors

    public Int32ValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (int);
    }
  }
}