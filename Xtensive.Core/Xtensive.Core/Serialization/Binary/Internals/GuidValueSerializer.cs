// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.15

using System;
using System.IO;
using System.Runtime.Serialization;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Serialization.Binary
{
  [Serializable]
  internal class GuidValueSerializer : BinaryValueSerializerBase<Guid>
  {
    public override Guid Deserialize(Stream stream) {
      if (stream.Length - stream.Position < 16)
        throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
      Byte[] buffer = new byte[16];
      stream.Read(buffer, 0, 16);
      return new Guid(buffer);
    }

    public override void Serialize(Stream stream, Guid value) {
      stream.Write(value.ToByteArray(), 0, 16);
    }

    // Constructors

    public GuidValueSerializer(IBinaryValueSerializerProvider provider)
      : base(provider) {}
  }
}