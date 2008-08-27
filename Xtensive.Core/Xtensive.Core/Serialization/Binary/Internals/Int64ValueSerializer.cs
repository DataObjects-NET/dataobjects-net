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
  internal class Int64ValueSerializer : BinaryValueSerializerBase<long>
  {
    public override long Deserialize(Stream stream) 
    {
      unchecked {
        long result = 0;
        for (int i = 0; i < sizeof (long); i++)
          result |= (long) stream.ReadByte() << i * 8;
        return result;
      }
    }

    public override void Serialize(Stream stream, long value) 
    {
      unchecked {
        for (int i = 0; i < sizeof (long); i++)
          stream.WriteByte((byte) (value >> i * 8));
      }
    }

    // Constructors

    public Int64ValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
    }
  }
}