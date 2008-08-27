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
  internal class UInt64ValueSerializer : BinaryValueSerializerBase<ulong>
  {
    public override ulong Deserialize(Stream stream) 
    {
      unchecked {
        ulong result = 0;
        for (int i = 0; i < sizeof (ulong); i++)
          result |= (ulong) stream.ReadByte() << i * 8;
        return result;
      }
    }

    public override void Serialize(Stream stream, ulong value) 
    {
      unchecked {
        for (int i = 0; i < sizeof (ulong); i++)
          stream.WriteByte((byte) (value >> i * 8));
      }
    }

    
    // Constructors

    public UInt64ValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      OutputLength = sizeof (ulong);
    }
  }
}