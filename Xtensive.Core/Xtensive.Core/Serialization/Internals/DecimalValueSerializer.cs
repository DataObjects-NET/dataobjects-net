// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.15

using System;
using System.IO;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Serialization
{
  [Serializable]
  internal sealed class DecimalValueSerializer : ValueSerializerBase<decimal>
  {
    public override decimal Deserialize(Stream stream) 
    {
      var intBuffer = new int[sizeof (decimal) / sizeof (int)];
      var buffer = new byte[sizeof (decimal)];
      int length = sizeof (decimal);
      stream.Read(buffer, 0, length);
      for (int i = 0; i < sizeof (decimal) / sizeof (int); i++)
        intBuffer[i] = BitConverter.ToInt32(buffer, i * sizeof (int));
      return new decimal(intBuffer);
    }

    public override void Serialize(Stream stream, decimal value) 
    {
      var buffer = new byte[sizeof (decimal)];
      var intBuffer = decimal.GetBits(value);
      for (int i = 0; i < sizeof (decimal) / sizeof (int); i++)
        BitConverter.GetBytes(intBuffer[i]).Copy(buffer, i * sizeof (int));
      stream.Write(buffer, 0, OutputLength);
    }


    // Constructors

    public DecimalValueSerializer(IValueSerializerProvider provider)
      : base(provider)
    {
      OutputLength = sizeof (decimal);
    }
  }
}