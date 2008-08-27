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
    public override decimal Deserialize(Stream stream) 
    {
      var buffer = new byte[sizeof (decimal)];
      var intBuffer = new int[sizeof (decimal) / sizeof (int)];
      stream.Read(buffer, 0, sizeof (decimal));
      for (int i = 0; i < sizeof (decimal) / sizeof (int); i++)
        intBuffer[i] = BitConverter.ToInt32(buffer, i * sizeof (int));
      return new decimal(intBuffer);
    }

    public override void Serialize(Stream stream, decimal value) 
    {
      var intBuffer = decimal.GetBits(value);
      for (int i = 0; i < sizeof (decimal) / sizeof (int); i++) {
        var buffer = BitConverter.GetBytes(intBuffer[i]);
        stream.Write(buffer, 0, sizeof (int));
      }
    }


    // Constructors

    public DecimalValueSerializer(IValueSerializerProvider<Stream> provider)
      : base(provider)
    {
      OutputLength = sizeof (decimal);
    }
  }
}