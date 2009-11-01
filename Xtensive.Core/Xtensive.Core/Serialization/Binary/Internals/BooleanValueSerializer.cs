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
  internal class BooleanValueSerializer : ValueSerializerBase<bool>
  {
    public override Boolean Deserialize(Stream stream)
    {
      if (stream.Length - stream.Position < sizeof (Boolean))
        throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
      return stream.ReadByte()==0x01;
    }

    public override void Serialize(Stream stream, Boolean value)
    {
      stream.WriteByte(value ? (byte)0x01 : (byte)0x00);
    }


    // Constructors

    public BooleanValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
    }
  }
}