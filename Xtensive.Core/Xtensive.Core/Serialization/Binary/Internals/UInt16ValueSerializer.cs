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
  internal class UInt16ValueSerializer : BinaryValueSerializerBase<ushort>
  {
    public override ushort Deserialize(Stream stream) 
    {
      unchecked {
        ushort result = 0;
        for (int i = 0; i < sizeof (ushort); i++)
          result |= (ushort) (stream.ReadByte() << i * 8);
        return result;
      }
    }

    public override void Serialize(Stream stream, ushort value) 
    {
      unchecked {
        for (int i = 0; i < sizeof (ushort); i++)
          stream.WriteByte((byte) (value >> i * 8));
      }
    }

    
    // Constructors

    public UInt16ValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
    }
  }
}