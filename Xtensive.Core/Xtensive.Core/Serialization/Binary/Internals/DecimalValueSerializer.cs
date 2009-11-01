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
  internal class DecimalValueSerializer : ValueSerializerBase<decimal>
  {
    public override Decimal Deserialize(Stream stream)
    {
      if (stream.Length - stream.Position < sizeof (Decimal))
        throw new SerializationException(Strings.ExDeserializationStreamLengthIncorrect);
      Byte[] buffer = new byte[sizeof (Decimal)];
      Int32[] intRepresentation = new int[sizeof (Decimal)/sizeof (Int32)];
      stream.Read(buffer, 0, sizeof (Decimal));
      for (int i = 0; i < sizeof (Decimal)/sizeof (Int32); i++) {
        intRepresentation[i] = BitConverter.ToInt32(buffer, i*sizeof (Int32));
      }
      return new Decimal(intRepresentation);
    }

    public override void Serialize(Stream stream, Decimal value)
    {
      Int32[] intRepresentation = Decimal.GetBits(value);
      for (int i = 0; i < sizeof (Decimal)/sizeof (Int32); i++) {
        Byte[] buffer = BitConverter.GetBytes(intRepresentation[i]);
        stream.Write(buffer, 0, sizeof (Int32));
      }
    }


    // Constructors

    public DecimalValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
    }
  }
}