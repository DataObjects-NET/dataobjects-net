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
  internal class Int32ValueSerializer : BinaryValueSerializerBase<int>
  {
    public override Int32 Deserialize(Stream stream) {
      if (stream.Length - stream.Position < sizeof (Int32))
        throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
      Int32 result = 0;
      for (int i = 0; i < sizeof (Int32); i++)
        result |= stream.ReadByte() << i * 8;
      return result;
    }

    public override void Serialize(Stream stream, Int32 value) {
      for (int i = 0; i < sizeof (Int32); i++)
        stream.WriteByte((byte) (value >> i * 8));
    }

    // Constructors

    public Int32ValueSerializer(IBinaryValueSerializerProvider provider)
      : base(provider) {}
  }
}