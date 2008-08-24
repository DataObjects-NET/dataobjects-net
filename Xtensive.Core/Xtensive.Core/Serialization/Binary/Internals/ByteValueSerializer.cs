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
  internal class ByteValueSerializer : BinaryValueSerializerBase<byte>
  {
    public override Byte Deserialize(Stream stream) {
      if (stream.Length - stream.Position < sizeof (Byte))
        throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
      return (Byte) stream.ReadByte();
    }

    public override void Serialize(Stream stream, Byte value) {
      stream.WriteByte(value);
    }

    // Constructors

    public ByteValueSerializer(IBinaryValueSerializerProvider provider)
      : base(provider) {}
  }
}