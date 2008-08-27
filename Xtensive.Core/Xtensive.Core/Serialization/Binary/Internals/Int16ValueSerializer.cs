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
  internal class Int16ValueSerializer : BinaryValueSerializerBase<short>
  {
    public override short Deserialize(Stream stream) 
    {
      unchecked {
        short result = 0;
        for (int i = 0; i < sizeof (short); i++)
          result |= (short) (stream.ReadByte() << i * 8);
        return result;
      }
    }

    public override void Serialize(Stream stream, short value) 
    {
      unchecked {
        for (int i = 0; i < sizeof (short); i++)
          stream.WriteByte((byte) (value >> i * 8));
      }
    }

    
    // Constructors

    public Int16ValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      OutputLength = sizeof (short);
    }
  }
}