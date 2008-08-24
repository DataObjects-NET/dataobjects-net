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
  internal class SingleValueSerializer : BinaryValueSerializerBase<float>
  {
    public override Single Deserialize(Stream stream) {
      if (stream.Length - stream.Position < sizeof (Single))
        throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
      byte[] buffer = new byte[sizeof (Single)];
      stream.Read(buffer, 0, sizeof (Single));
      return BitConverter.ToSingle(buffer, 0);
    }

    public override void Serialize(Stream stream, Single value) {
      byte[] buffer = BitConverter.GetBytes(value);
      stream.Write(buffer, 0, sizeof (Single));
    }

    // Constructors

    public SingleValueSerializer(IBinaryValueSerializerProvider provider)
      : base(provider) {}
  }
}