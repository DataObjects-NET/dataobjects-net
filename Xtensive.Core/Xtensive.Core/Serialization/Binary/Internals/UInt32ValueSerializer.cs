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
  internal class UInt32ValueSerializer : BinaryValueSerializerBase<uint>
  {
    public override uint Deserialize(Stream stream) 
    {
      unchecked {
        uint result = 0;
        for (int i = 0; i < sizeof (uint); i++)
          result |= (uint) stream.ReadByte() << i * 8;
        return result;
      }
    }

    public override void Serialize(Stream stream, uint value) 
    {
      unchecked {
        for (int i = 0; i < sizeof (uint); i++)
          stream.WriteByte((byte) (value >> i * 8));
      }
    }


    // Constructors

    public UInt32ValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      OutputLength = sizeof (uint);
    }
  }
}