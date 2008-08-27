// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;
using System.IO;

namespace Xtensive.Core.Serialization.Binary
{
  [Serializable]
  internal class ByteValueSerializer : BinaryValueSerializerBase<byte>
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

    public ByteValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
    }
  }
}