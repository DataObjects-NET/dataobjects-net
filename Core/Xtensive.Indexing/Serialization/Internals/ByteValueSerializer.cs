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
  internal sealed class ByteValueSerializer : ValueSerializerBase<byte>
  {
    public override Byte Deserialize(Stream stream) 
    {
      return (Byte) stream.ReadByte();
    }

    public override void Serialize(Stream stream, Byte value) 
    {
      stream.WriteByte(value);
    }

    
    // Constructors

    public ByteValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (byte);
    }
  }
}