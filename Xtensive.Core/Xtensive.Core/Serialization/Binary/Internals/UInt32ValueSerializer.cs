// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Serialization.Binary
{
  [Serializable]
  internal class UInt32ValueSerializer : BinaryValueSerializerBase<uint>
  {
    public override UInt32 Deserialize(Stream stream) {
      unchecked {
        if (stream.Length - stream.Position < sizeof (UInt32))
          throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
        UInt32 result = 0;
        for (int i = 0; i < sizeof (UInt32); i++)
          result |= (UInt32) stream.ReadByte() << i * 8;
        return result;
      }
    }

    public override void Serialize(Stream stream, UInt32 value) {
      unchecked {
        for (int i = 0; i < sizeof (UInt32); i++)
          stream.WriteByte((byte) (value >> i * 8));
      }
    }

    // Constructors

    public UInt32ValueSerializer(IBinaryValueSerializerProvider provider)
      : base(provider) {}
  }
}