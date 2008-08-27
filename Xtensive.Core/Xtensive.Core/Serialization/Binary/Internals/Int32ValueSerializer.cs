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
  internal class Int32ValueSerializer : BinaryValueSerializerBase<int>
  {
    public override int Deserialize(Stream stream) 
    {
      unchecked {
        int result = 0;
        for (int i = 0; i < sizeof (int); i++)
          result |= stream.ReadByte() << i * 8;
        return result;
      }
    }

    public override void Serialize(Stream stream, int value) 
    {
      unchecked {
        for (int i = 0; i < sizeof (int); i++)
          stream.WriteByte((byte) (value >> i * 8));
      }
    }


    // Constructors

    public Int32ValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      OutputLength = sizeof (int);
    }
  }
}