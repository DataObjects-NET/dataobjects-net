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
  internal sealed class UInt32ValueSerializer : ValueSerializerBase<uint>
  {
    public override uint Deserialize(Stream stream) 
    {
      var buffer = new byte[sizeof (uint)];
      stream.Read(buffer, 0, sizeof (uint));
      return BitConverter.ToUInt32(buffer, 0);
    }

    public override void Serialize(Stream stream, uint value) 
    {
      stream.Write(BitConverter.GetBytes(value), 0, sizeof (uint));
    }


    // Constructors

    public UInt32ValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (uint);
    }
  }
}