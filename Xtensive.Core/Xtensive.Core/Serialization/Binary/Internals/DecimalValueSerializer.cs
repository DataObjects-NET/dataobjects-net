// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.15

using System;
using System.IO;

namespace Xtensive.Core.Serialization.Binary
{
  [Serializable]
  internal class DecimalValueSerializer : BinaryValueSerializerBase<decimal>
  {
    public override Decimal Deserialize(Stream stream) 
    {
      var buffer = new byte[sizeof (Decimal)];
      var intBuffer = new int[sizeof (Decimal) / sizeof (int)];
      stream.Read(buffer, 0, sizeof (Decimal));
      for (int i = 0; i < sizeof (Decimal) / sizeof (int); i++)
        intBuffer[i] = BitConverter.ToInt32(buffer, i * sizeof (int));
      return new Decimal(intBuffer);
    }

    public override void Serialize(Stream stream, Decimal value) 
    {
      var intBuffer = Decimal.GetBits(value);
      for (int i = 0; i < sizeof (Decimal) / sizeof (int); i++) {
        Byte[] buffer = BitConverter.GetBytes(intBuffer[i]);
        stream.Write(buffer, 0, sizeof (int));
      }
    }


    // Constructors

    public DecimalValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
    }
  }
}