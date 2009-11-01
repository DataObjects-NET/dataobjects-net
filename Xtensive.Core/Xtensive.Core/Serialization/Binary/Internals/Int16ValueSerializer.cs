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
  internal class Int16ValueSerializer : ValueSerializerBase<short>
  {
    public override Int16 Deserialize(Stream stream)
    {
      unchecked{
        if (stream.Length - stream.Position < sizeof (Int16))
          throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
        Int16 result = 0;
        for (int i = 0; i < sizeof (Int16); i++)
          result |= (Int16)(stream.ReadByte() << i*8);
        return result;
      }
    }

    public override void Serialize(Stream stream, Int16 value)
    {
      unchecked{
        for (int i = 0; i < sizeof (Int16); i++)
          stream.WriteByte((byte)(value >> i*8));
      }
    }


    // Constructors

    public Int16ValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
    }
  }
}