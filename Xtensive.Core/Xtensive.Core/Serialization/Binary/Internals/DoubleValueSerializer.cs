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
  internal class DoubleValueSerializer : BinaryValueSerializerBase<double>
  {
    public override Double Deserialize(Stream stream) {
      if (stream.Length - stream.Position < sizeof (Double))
        throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
      byte[] buffer = new byte[sizeof (Double)];
      stream.Read(buffer, 0, sizeof (Double));
      return BitConverter.ToDouble(buffer, 0);
    }

    public override void Serialize(Stream stream, Double value) {
      byte[] buffer = BitConverter.GetBytes(value);
      stream.Write(buffer, 0, sizeof (Double));
    }

    // Constructors

    public DoubleValueSerializer(IBinaryValueSerializerProvider provider)
      : base(provider) {}
  }
}